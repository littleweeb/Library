using LittleWeebLibrary.EventArguments;
using LittleWeebLibrary.GlobalInterfaces;
using LittleWeebLibrary.Models;
using LittleWeebLibrary.Settings;
using LittleWeebLibrary.StaticClasses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace LittleWeebLibrary.Handlers
{
    public interface IDownloadHandler
    {
        event EventHandler<DownloadUpdateEventArgs> OnDownloadUpdateEvent;
        string AddDownload(JsonDownloadInfo download);
        string AddDownloads(List<JsonDownloadInfo> download);
        Task<string> AbortDownload(string id = null, string filePath = null);
        string RemoveDownload(string id = null, string filePath = null);
        string GetCurrentlyDownloading();
        string StopQueue();
    }

    public class DownloadHandler : IDownloadHandler, ISettingsInterface
    {
        public event EventHandler<DownloadUpdateEventArgs> OnDownloadUpdateEvent;
       

        private readonly IIrcClientHandler IrcClientHandler;
        private readonly IDebugHandler DebugHandler;

        private bool Stop;
        private bool DownloadProcesOnGoing = false;
        private JsonDownloadInfo CurrentlyDownloading;
        private IrcSettings IrcSettings;
        private List<JsonDownloadInfo> DownloadQueue;

        public DownloadHandler(IIrcClientHandler ircClientHandler, IDebugHandler debugHandler)
        {
            debugHandler.TraceMessage("DownloadHandler", DebugSource.CONSTRUCTOR, DebugType.ENTRY_EXIT);
            DebugHandler = debugHandler;

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

        public string AddDownload(JsonDownloadInfo download)
        {
            DebugHandler.TraceMessage("AddDownload Called.", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage(download.ToString(), DebugSource.TASK, DebugType.PARAMETERS);
            try
            {
                if (download.filesize.Contains("."))
                {
                    download.filesize = ((int)(double.Parse(download.filesize, System.Globalization.CultureInfo.InvariantCulture) * 1024)).ToString();
                }
                if (UtilityMethods.GetFreeSpace(IrcSettings.fullfilepath) > (int.Parse(download.filesize) * 1024 * 1024))
                {

                    download.downloadIndex = DownloadQueue.Count - 1;

                    if (!DownloadQueue.Contains(download) || CurrentlyDownloading != download)
                    {


                        DownloadQueue.Add(download);
                        DebugHandler.TraceMessage("Added download to queue: " + download.ToString(), DebugSource.TASK, DebugType.INFO);

                        JsonSuccess succes = new JsonSuccess()
                        {
                            message = "Succesfully added download to download queue."
                        };

                        return succes.ToJson();
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
                        return error.ToJson();
                    }

                }
                else
                {
                    DebugHandler.TraceMessage("Could not add download with filesize: " + download.filesize + " due to insufficient space required: " + (UtilityMethods.GetFreeSpace(IrcSettings.fullfilepath) / 1024 / 1024).ToString(), DebugSource.TASK, DebugType.WARNING);

                    JsonError error = new JsonError()
                    {
                        type = "unsufficient_space_error",
                        errormessage = "Could not add download with filesize: " + download.filesize + " due to insufficient space required: " + (UtilityMethods.GetFreeSpace(IrcSettings.fullfilepath) / 1024 / 1024).ToString(),
                        errortype = "warning"
                    };
                    return error.ToJson();
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
                return error.ToJson();
            }
        }

        public string RemoveDownload(string id = null, string filepath = null)
        {

            DebugHandler.TraceMessage("Remove Called.", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage("ID: " + id, DebugSource.TASK, DebugType.PARAMETERS);
            DebugHandler.TraceMessage("FILEPATH: " + filepath, DebugSource.TASK, DebugType.PARAMETERS);

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
                        return error.ToJson();
                    }
                    index++;
                }

                JsonSuccess succes = new JsonSuccess()
                {
                    message = "Succesfully removed download from download queue by download json."
                };

                return succes.ToJson();
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
                return error.ToJson();
            }
        }


        public string StopQueue()
        {

            DebugHandler.TraceMessage("StopQueue Called.", DebugSource.TASK, DebugType.ENTRY_EXIT);

            Stop = true;

            JsonSuccess succes = new JsonSuccess()
            {
                message = "Succesfully told queue to stop running."
            };

            return succes.ToJson();
        }

        public string GetCurrentlyDownloading()
        {
            DebugHandler.TraceMessage("GetCurrentlyDownloading Called.", DebugSource.TASK, DebugType.ENTRY_EXIT);
            return CurrentlyDownloading.ToJson();
        }

        private void OnIrcClientDownloadEvent(object sender, IrcClientDownloadEventArgs args)
        {

            DebugHandler.TraceMessage("OnIrcClientDownloadEvent Called.", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage(args.ToString(), DebugSource.TASK, DebugType.PARAMETERS);


            if (CurrentlyDownloading != null)
            {

                if (args.DownloadStatus == "PARSING" || args.DownloadStatus == "WAITING")
                {
                    OnDownloadUpdateEvent?.Invoke(this, new DownloadUpdateEventArgs()
                    {
                        id = CurrentlyDownloading.id,
                        animeid = CurrentlyDownloading.animeInfo.anime_id,
                        animeTitle = CurrentlyDownloading.animeInfo.anime_title,
                        animeCoverSmall = CurrentlyDownloading.animeInfo.anime_cover_small,
                        animeCoverOriginal = CurrentlyDownloading.animeInfo.anime_cover_original,
                        episodeNumber = CurrentlyDownloading.episodeNumber,
                        bot = CurrentlyDownloading.bot,
                        pack = CurrentlyDownloading.pack,
                        progress = args.DownloadProgress.ToString(),
                        speed = args.DownloadSpeed.ToString(),
                        status = args.DownloadStatus,
                        filename = CurrentlyDownloading.filename,
                        filesize = CurrentlyDownloading.filesize,
                        fullfilepath = CurrentlyDownloading.fullfilepath,
                        downloadIndex = CurrentlyDownloading.downloadIndex
                    });
                }
                else
                {
                    OnDownloadUpdateEvent?.Invoke(this, new DownloadUpdateEventArgs()
                    {
                        id = CurrentlyDownloading.id,
                        animeid = CurrentlyDownloading.animeInfo.anime_id,
                        animeTitle = CurrentlyDownloading.animeInfo.anime_title,
                        animeCoverSmall = CurrentlyDownloading.animeInfo.anime_cover_small,
                        animeCoverOriginal = CurrentlyDownloading.animeInfo.anime_cover_original,
                        episodeNumber = CurrentlyDownloading.episodeNumber,
                        bot = CurrentlyDownloading.bot,
                        pack = CurrentlyDownloading.pack,
                        progress = args.DownloadProgress.ToString(),
                        speed = args.DownloadSpeed.ToString(),
                        status = args.DownloadStatus,
                        filename = args.FileName,
                        filesize = args.FileSize.ToString(),
                        fullfilepath = args.FileLocation,
                        downloadIndex = CurrentlyDownloading.downloadIndex
                    });

                }


                if (args.DownloadStatus.Contains("COMPLETED"))
                {

                    OnDownloadUpdateEvent?.Invoke(this, new DownloadUpdateEventArgs()
                    {
                        id = CurrentlyDownloading.id,
                        animeid = CurrentlyDownloading.animeInfo.anime_id,
                        animeTitle = CurrentlyDownloading.animeInfo.anime_title,
                        animeCoverSmall = CurrentlyDownloading.animeInfo.anime_cover_small,
                        animeCoverOriginal = CurrentlyDownloading.animeInfo.anime_cover_original,
                        episodeNumber = CurrentlyDownloading.episodeNumber,
                        bot = CurrentlyDownloading.bot,
                        pack = CurrentlyDownloading.pack,
                        progress = args.DownloadProgress.ToString(),
                        speed = args.DownloadSpeed.ToString(),
                        status = args.DownloadStatus,
                        filename = CurrentlyDownloading.filename,
                        filesize = CurrentlyDownloading.filesize,
                        fullfilepath = CurrentlyDownloading.fullfilepath,
                        downloadIndex = CurrentlyDownloading.downloadIndex
                    });


                    if (CurrentlyDownloading.id != string.Empty)
                    {
                        RemoveDownload(CurrentlyDownloading.id);

                        CurrentlyDownloading = new JsonDownloadInfo();
                    }


                    DownloadProcesOnGoing = false;
                }
                else if (args.DownloadStatus.Contains("FAILED") || args.DownloadStatus.Contains("ABORTED"))
                {

                    if (CurrentlyDownloading.id != string.Empty)
                    {
                        RemoveDownload(CurrentlyDownloading.id);

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

        public async Task<string> AbortDownload(string id = null, string filePath = null)
        {

            DebugHandler.TraceMessage("AbortDownload Called.", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage("ID: " + id + ", FILEPATH: " + filePath, DebugSource.TASK, DebugType.PARAMETERS);

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

                return succes.ToJson();
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
                return error.ToJson();
            }

        }

        public string AddDownloads(List<JsonDownloadInfo> download)
        {
            DebugHandler.TraceMessage("AddDownloads Called.", DebugSource.TASK, DebugType.ENTRY_EXIT);

            try
            {

                long totalSizeNeeded = 0;
                foreach (JsonDownloadInfo downloadinfo in download)
                {
                    DebugHandler.TraceMessage(downloadinfo.ToString(), DebugSource.TASK, DebugType.PARAMETERS);
                    if (downloadinfo.filesize.Contains("."))
                    {
                        downloadinfo.filesize = ((int)(double.Parse(downloadinfo.filesize, System.Globalization.CultureInfo.InvariantCulture) * 1024)).ToString();
                        totalSizeNeeded += (int)(double.Parse(downloadinfo.filesize, System.Globalization.CultureInfo.InvariantCulture) * 1024);
                    }
                    else
                    {
                        totalSizeNeeded += int.Parse(downloadinfo.filesize);
                    }
                }

                if (UtilityMethods.GetFreeSpace(IrcSettings.fullfilepath) > (totalSizeNeeded * 1024 * 1024))
                {
                    DownloadQueue.AddRange(download);


                    DebugHandler.TraceMessage("Succesfully added " + download.Count + " downloads to download queue.", DebugSource.TASK, DebugType.INFO);

                    JsonSuccess succes = new JsonSuccess()
                    {
                        message = "Succesfully added " + download.Count + " downloads to download queue."
                    };

                    return succes.ToJson();
                }
                else
                {

                    DebugHandler.TraceMessage("Could not add downloads with filesize: " + totalSizeNeeded + " due to insufficient space required: " + (UtilityMethods.GetFreeSpace(IrcSettings.fullfilepath) / 1024 / 1024).ToString(), DebugSource.TASK, DebugType.WARNING);


                    JsonError error = new JsonError()
                    {
                        type = "unsufficient_space_error",
                        errormessage = "Could not add download with filesize: " + totalSizeNeeded + " due to insufficient space required: " + (UtilityMethods.GetFreeSpace(IrcSettings.fullfilepath) / 1024 / 1024).ToString(),
                        errortype = "warning"
                    };
                    return error.ToJson();
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
                return error.ToJson();
            }
        }
    }
}
