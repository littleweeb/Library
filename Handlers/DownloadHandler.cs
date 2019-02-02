using LittleWeebLibrary.EventArguments;
using LittleWeebLibrary.GlobalInterfaces;
using LittleWeebLibrary.Models;
using LittleWeebLibrary.Settings;
using LittleWeebLibrary.StaticClasses;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace LittleWeebLibrary.Handlers
{
    public interface IDownloadHandler
    {
        event EventHandler<DownloadUpdateEventArgs> OnDownloadUpdateEvent;
        Task<JObject> AddDownload(JsonDownloadInfo download);
        Task<JObject> AddDownloads(List<JsonDownloadInfo> download);
        Task<JObject> AbortDownload(string id = null);
        Task<JObject> RemoveDownload(string id = null, string filePath = null);
        JObject GetCurrentlyDownloading();
        JObject StopQueue();
    }

    public class DownloadHandler : IDownloadHandler, ISettingsInterface
    {
        public event EventHandler<DownloadUpdateEventArgs> OnDownloadUpdateEvent;
       

        private readonly IIrcClientHandler IrcClientHandler;
        private readonly IFileHistoryHandler FileHistoryHandler;
        private readonly IFileHandler FileHandler;
        private readonly IDebugHandler DebugHandler;

        private bool Stop;
        private bool DownloadProcesOnGoing = false;
        private JsonDownloadInfo CurrentlyDownloading;
        private IrcSettings IrcSettings;
        private List<JsonDownloadInfo> DownloadQueue;

        public DownloadHandler(IIrcClientHandler ircClientHandler, IFileHistoryHandler fileHistoryHandler, IFileHandler fileHandler, IDebugHandler debugHandler)
        {
            debugHandler.TraceMessage("DownloadHandler", DebugSource.CONSTRUCTOR, DebugType.ENTRY_EXIT);
            DebugHandler = debugHandler;
            FileHistoryHandler = fileHistoryHandler;
            FileHandler = fileHandler;

            try
            {
                DownloadQueue = new List<JsonDownloadInfo>();
                IrcClientHandler = ircClientHandler;
                IrcClientHandler.OnIrcClientDownloadEvent += OnIrcClientDownloadEvent;
                CurrentlyDownloading = new JsonDownloadInfo();
                Stop = false;

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                DownloadQueueHandler();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
            catch (Exception e)
            {
                DebugHandler.TraceMessage(e.ToString(), DebugSource.CONSTRUCTOR, DebugType.ERROR);
            }

        }

        public async Task<JObject> AddDownload(JsonDownloadInfo download)
        {
            DebugHandler.TraceMessage("AddDownload Called.", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage(download.ToString(), DebugSource.TASK, DebugType.PARAMETERS);

            JObject toReturn = new JObject();

            await Task.Run(() =>
            {
                try
                {
                    if (UtilityMethods.GetFreeSpaceKbits(IrcSettings.fullfilepath) > long.Parse(download.filesize)) //kbits -> bytes
                    {
                        download.id = IdGenerator(download.anime_id, download.bot, download.pack, download.episodeNumber, download.season).ToString();
                        download.downloadDirectory = Path.Combine(IrcSettings.fullfilepath, download.anime_name);

                        if (!DownloadQueue.Exists(x => x.id == download.id) || CurrentlyDownloading.id != download.id)
                        {
                            download.downloadIndex = DownloadQueue.Count - 1;
                            DownloadQueue.Add(download);
                            DebugHandler.TraceMessage("Added download to queue: " + download.ToString(), DebugSource.TASK, DebugType.INFO);

                            JsonDownloadQueue current = new JsonDownloadQueue()
                            {
                                downloadQueue = DownloadQueue
                            };

                            toReturn = current.ToJObject();
                        }
                        else
                        {

                            DebugHandler.TraceMessage("Could not add download: " + download.ToString() + ", already exist in queue or is already being downloaded ", DebugSource.TASK, DebugType.WARNING);
                            JsonError error = new JsonError()
                            {
                                type = "file_already_being_downloaded_error",
                                errormessage = "Could not add download: " + download.ToString() + ", already exist in queue or is already being downloaded ",
                                errortype = "warning",
                                exception = "none"
                            };
                            toReturn = error.ToJObject();
                        }

                    }
                    else
                    {
                        DebugHandler.TraceMessage("Could not add download with filesize: " + download.filesize + " due to insufficient space required: " + UtilityMethods.GetFreeSpaceKbits(IrcSettings.fullfilepath).ToString(), DebugSource.TASK, DebugType.WARNING);

                        JsonError error = new JsonError()
                        {
                            type = "unsufficient_space_error",
                            errormessage = "Could not add download with filesize: " + download.filesize + " due to insufficient space required: " + UtilityMethods.GetFreeSpaceKbits(IrcSettings.fullfilepath).ToString().ToString(),
                            errortype = "warning"
                        };
                        toReturn = error.ToJObject();
                    }

                }
                catch (Exception e)
                {
                    DebugHandler.TraceMessage("Could not add download with filesize: " + e.ToString(), DebugSource.TASK, DebugType.ERROR);

                    JsonError error = new JsonError()
                    {
                        type = "add_download_error",
                        errormessage = "Could not add download to queue.",
                        errortype = "exception",
                        exception = e.ToString()
                    };
                    toReturn = error.ToJObject();
                }
            });

            return toReturn;
        }

        public async Task<JObject> RemoveDownload(string id = null, string filepath = null)
        {

            DebugHandler.TraceMessage("Remove Called.", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage("ID: " + id, DebugSource.TASK, DebugType.PARAMETERS);
            DebugHandler.TraceMessage("FILEPATH: " + filepath, DebugSource.TASK, DebugType.PARAMETERS);

            JObject toReturn = new JObject();

            await Task.Run(async () =>
            {

                try
                {

                    int index = 0;
                    foreach (JsonDownloadInfo queuedDownload in DownloadQueue)
                    {
                        if (id != null)
                        {
                            if (queuedDownload.id == id)
                            {
                                DebugHandler.TraceMessage("Removed download at index: " + index.ToString() + " using id: " + id, DebugSource.TASK, DebugType.INFO);
                                DownloadQueue.RemoveAt(index);
                                break;
                            }
                        }
                        else if (filepath != null)
                        {
                            if (Path.Combine(queuedDownload.fullfilepath, queuedDownload.filename) == filepath)
                            {
                                DebugHandler.TraceMessage("Removed download at index: " + index.ToString() + " using filepath: " + filepath, DebugSource.TASK, DebugType.INFO);
                                DownloadQueue.RemoveAt(index);
                                break;
                            }
                        }
                        else
                        {
                            DebugHandler.TraceMessage("Could not remove download from queue, neither id or filepath is defined.", DebugSource.TASK, DebugType.WARNING);
                            JsonError error = new JsonError()
                            {
                                type = "remove_download_error",
                                errormessage = "Could not remove download from queue, neither id or filepath is defined.",
                                errortype = "warning"
                            };
                            toReturn = error.ToJObject();
                        }
                        index++;
                    }

                    if (CurrentlyDownloading.id != string.Empty)
                    {
                        await AbortDownload(id);
                    }

                    JsonDownloadQueue current = new JsonDownloadQueue()
                    {
                        downloadQueue = DownloadQueue
                    };

                    toReturn = current.ToJObject();
                }
                catch (Exception e)
                {

                    DebugHandler.TraceMessage("Could not remove download from queue by download json: " + e.ToString(), DebugSource.TASK, DebugType.ERROR);

                    JsonError error = new JsonError()
                    {
                        type = "remove_download_error",
                        errormessage = "Could not remove download from queue by download json.",
                        errortype = "exception"
                    };
                    toReturn = error.ToJObject();
                }
            });

            return toReturn;

        }


        public JObject StopQueue()
        {

            DebugHandler.TraceMessage("StopQueue Called.", DebugSource.TASK, DebugType.ENTRY_EXIT);

            Stop = true;

            JsonSuccess succes = new JsonSuccess()
            {
                message = "Succesfully told queue to stop running."
            };

            return succes.ToJObject();
        }

        public JObject GetCurrentlyDownloading()
        {
            DebugHandler.TraceMessage("GetCurrentlyDownloading Called.", DebugSource.TASK, DebugType.ENTRY_EXIT);
            return CurrentlyDownloading.ToJObject();
        }

        private async void OnIrcClientDownloadEvent(object sender, IrcClientDownloadEventArgs args)
        {

            DebugHandler.TraceMessage("OnIrcClientDownloadEvent Called.", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage(args.ToString(), DebugSource.TASK, DebugType.PARAMETERS);

            if (CurrentlyDownloading != null)
            {
                if (args.DownloadStatus == "PARSING" || args.DownloadStatus == "WAITING")
                {

                    CurrentlyDownloading.progress = args.DownloadProgress.ToString();
                    CurrentlyDownloading.speed = args.DownloadSpeed.ToString();
                    CurrentlyDownloading.status = args.DownloadStatus;
                    CurrentlyDownloading.filename = args.FileName;

                    OnDownloadUpdateEvent?.Invoke(this, new DownloadUpdateEventArgs()
                    {
                        downloadInfo = CurrentlyDownloading
                    });
                }
                else
                {
                    CurrentlyDownloading.progress = args.DownloadProgress.ToString();
                    CurrentlyDownloading.speed = args.DownloadSpeed.ToString();
                    CurrentlyDownloading.status = args.DownloadStatus;
                    CurrentlyDownloading.filename = args.FileName;
                    CurrentlyDownloading.fullfilepath = args.FileLocation;

                    OnDownloadUpdateEvent?.Invoke(this, new DownloadUpdateEventArgs()
                    {
                        downloadInfo = CurrentlyDownloading
                    });

                }


                if (args.DownloadStatus.Contains("COMPLETED"))
                {


                    CurrentlyDownloading.progress = args.DownloadProgress.ToString();
                    CurrentlyDownloading.speed = args.DownloadSpeed.ToString();
                    CurrentlyDownloading.status = args.DownloadStatus;
                    CurrentlyDownloading.filename = args.FileName;
                    CurrentlyDownloading.fullfilepath = args.FileLocation;

                    OnDownloadUpdateEvent?.Invoke(this, new DownloadUpdateEventArgs()
                    {
                        downloadInfo = CurrentlyDownloading
                    });


                    await FileHistoryHandler.AddFileToFileHistory(new JsonDownloadedInfo()
                    {
                        anime_id = CurrentlyDownloading.anime_id,
                        anime_name = CurrentlyDownloading.anime_name,
                        id = CurrentlyDownloading.id,
                        season = CurrentlyDownloading.season,
                        episodeNumber = CurrentlyDownloading.episodeNumber,
                        bot = CurrentlyDownloading.bot,
                        pack = CurrentlyDownloading.pack,
                        filename = CurrentlyDownloading.filename,
                        filesize = CurrentlyDownloading.filesize,
                        fullfilepath = CurrentlyDownloading.fullfilepath
                    });

                    if (CurrentlyDownloading.id != string.Empty)
                    {
                        await RemoveDownload(CurrentlyDownloading.id);
                        CurrentlyDownloading = new JsonDownloadInfo();
                    }

                    DownloadProcesOnGoing = false;
                }
                else if (args.DownloadStatus.Contains("FAILED") || args.DownloadStatus.Contains("ABORTED"))
                {
                    if (CurrentlyDownloading.id != string.Empty)
                    {
                        await RemoveDownload(CurrentlyDownloading.id);

                        CurrentlyDownloading = new JsonDownloadInfo();
                    }
                    DownloadProcesOnGoing = false;
                }

            }
            else
            {
                DebugHandler.TraceMessage("Got download event, but no CurrrentlyDownloading object has been set!", DebugSource.TASK, DebugType.WARNING);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Await.Warning", "CS4014:Await.Warning")]
        private async Task DownloadQueueHandler()
        {
            DebugHandler.TraceMessage("DownloadQueueHandler Called.", DebugSource.TASK, DebugType.ENTRY_EXIT);

            while (!Stop)
            {

                if (DownloadQueue.Count > 0 && !DownloadProcesOnGoing)
                {
                    DownloadProcesOnGoing = true;
                    CurrentlyDownloading = DownloadQueue[0];

                    DebugHandler.TraceMessage("Requesting start of the following download: " + CurrentlyDownloading.ToJson(), DebugSource.TASK, DebugType.INFO);

                    IrcClientHandler.StartDownload(CurrentlyDownloading);

                }

                await Task.Delay(1000);
            }
        }

        public void SetIrcSettings(IrcSettings settings)
        {
            DebugHandler.TraceMessage("SetIrcSettings Called.", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage(settings.ToString(), DebugSource.TASK, DebugType.PARAMETERS);
            IrcSettings = settings;
        }

        public void SetLittleWeebSettings(LittleWeebSettings settings)
        {
            DebugHandler.TraceMessage("SetLittleWeebSettings Called.", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage(settings.ToString(), DebugSource.TASK, DebugType.PARAMETERS);
        }

        public async Task<JObject> AbortDownload(string id = null)
        {

            DebugHandler.TraceMessage("AbortDownload Called.", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage("ID: " + id , DebugSource.TASK, DebugType.PARAMETERS);

            try
            {

                if (IrcClientHandler.IsDownloading() && DownloadProcesOnGoing)
                {
                    if (CurrentlyDownloading.id == id)
                    {
                        DebugHandler.TraceMessage("Stopping the current download!", DebugSource.TASK, DebugType.INFO);
                        IrcClientHandler.StopDownload();
                    }
                }

                while (IrcClientHandler.IsDownloading())
                {
                    DebugHandler.TraceMessage("Current download still running!", DebugSource.TASK, DebugType.INFO);
                    await Task.Delay(100);
                }

                DebugHandler.TraceMessage("Current download stopped!", DebugSource.TASK, DebugType.INFO);
                JsonSuccess succes = new JsonSuccess()
                {
                    message = "Succesfully aborted download from download queue by download json."
                };

                return succes.ToJObject();
            }
            catch (Exception e)
            {
                DebugHandler.TraceMessage("Could not remove download from queue by download json and stop the download.", DebugSource.TASK, DebugType.WARNING);
                DebugHandler.TraceMessage(e.ToString(), DebugSource.TASK, DebugType.WARNING);
                JsonError error = new JsonError()
                {
                    type = "remove_download_error",
                    errormessage = "Could not remove download from queue by download json.",
                    errortype = "exception"
                };
                return error.ToJObject();
            }

        }

        public async Task<JObject> AddDownloads(List<JsonDownloadInfo> download)
        {
            DebugHandler.TraceMessage("AddDownloads Called.", DebugSource.TASK, DebugType.ENTRY_EXIT);
            JObject toReturn = new JObject();

            await Task.Run(() =>
            {
                try
                {

                    long totalSizeNeeded = 0;
                    foreach (JsonDownloadInfo downloadinfo in download)
                    {
                        DebugHandler.TraceMessage(downloadinfo.ToString(), DebugSource.TASK, DebugType.PARAMETERS);
                        totalSizeNeeded += int.Parse(downloadinfo.filesize);
                    }

                    if (UtilityMethods.GetFreeSpaceKbits(IrcSettings.fullfilepath) > totalSizeNeeded)
                    {
                        DownloadQueue.AddRange(download);

                        DebugHandler.TraceMessage("Succesfully added " + download.Count + " downloads to download queue.", DebugSource.TASK, DebugType.INFO);

                        JsonDownloadQueue current = new JsonDownloadQueue()
                        {
                            downloadQueue = DownloadQueue
                        };
                        toReturn = current.ToJObject();
                    }
                    else
                    {

                        DebugHandler.TraceMessage("Could not add downloads with filesize: " + totalSizeNeeded + " due to insufficient space required: " + (UtilityMethods.GetFreeSpaceKbits(IrcSettings.fullfilepath)).ToString(), DebugSource.TASK, DebugType.WARNING);

                        JsonError error = new JsonError()
                        {
                            type = "unsufficient_space_error",
                            errormessage = "Could not add download with filesize: " + totalSizeNeeded + " due to insufficient space required: " + (UtilityMethods.GetFreeSpaceKbits(IrcSettings.fullfilepath)).ToString(),
                            errortype = "warning"
                        };
                        toReturn = error.ToJObject();
                    }

                }
                catch (Exception e)
                {
                    DebugHandler.TraceMessage("Could not add downloads: " + e.ToString(), DebugSource.TASK, DebugType.WARNING);
                    JsonError error = new JsonError()
                    {
                        type = "add_download_error",
                        errormessage = "Could not add download to queue.",
                        errortype = "exception",
                        exception = e.ToString()
                    };
                    toReturn = error.ToJObject();
                }
            });

            return toReturn;
        }

        private int IdGenerator(string animeId, string botName, string packnum, string episode, string season)
        {
            string combined = "";
            combined += animeId;
            combined += botName;
            combined += packnum;
            combined += episode;
            combined += season;
            return combined.GetHashCode();
        }
    }
}
