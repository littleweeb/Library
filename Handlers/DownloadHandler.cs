using LittleWeebLibrary.EventArguments;
using LittleWeebLibrary.GlobalInterfaces;
using LittleWeebLibrary.Models;
using LittleWeebLibrary.Settings;
using LittleWeebLibrary.StaticClasses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LittleWeebLibrary.Handlers
{
    public interface IDownloadHandler
    {
        event EventHandler<DownloadUpdateEventArgs> OnDownloadUpdateEvent;
        string AddDownload(JsonDownloadInfo download);
        string RemoveDownload(string id = null, string filePath = null);
        string GetCurrentlyDownloading();
        string StopQueue();
    }

    public class DownloadHandler : IDownloadHandler, IDebugEvent, ISettingsInterface
    {
        public event EventHandler<DownloadUpdateEventArgs> OnDownloadUpdateEvent;
        public event EventHandler<BaseDebugArgs> OnDebugEvent;

        private List<JsonDownloadInfo> DownloadQueue;
        private IIrcClientHandler IrcClientHandler;
        private bool Stop;
        private bool IsDownloading;
        private JsonDownloadInfo CurrentlyDownloading;
        private IrcSettings IrcSettings;

        public DownloadHandler(IIrcClientHandler ircClientHandler)
        {
            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugMessage = "Constructor called.",
                DebugSource = this.GetType().Name,
                DebugSourceType = 0,
                DebugType = 0
            });


            try
            {
                DownloadQueue = new List<JsonDownloadInfo>();
                IrcClientHandler = ircClientHandler;
                IrcClientHandler.OnIrcClientDownloadEvent += OnIrcClientDownloadEvent;
                CurrentlyDownloading = new JsonDownloadInfo();
                Stop = false;

                Task.Run(async () => await DownloadQueueHandler());
            }
            catch (Exception e)
            {
                OnDebugEvent?.Invoke(this, new BaseDebugArgs()
                {
                    DebugSource = this.GetType().Name,
                    DebugMessage = e.ToString(),
                    DebugSourceType = 0,
                    DebugType = 4
                });
            }

        }

        public string AddDownload(JsonDownloadInfo download)
        {
            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugMessage = "AddDownload Called.",
                DebugSource = this.GetType().Name,
                DebugSourceType = 1,
                DebugType = 0
            });
            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugMessage = download.ToString(),
                DebugSource = this.GetType().Name,
                DebugSourceType = 1,
                DebugType = 1
            });
            try
            {
                if (UtilityMethods.GetFreeSpace(IrcSettings.fullfilepath) > (int.Parse(download.filesize) * 1024 * 1024))
                {

                    download.downloadIndex = DownloadQueue.Count - 1;

                    if (!DownloadQueue.Contains(download) || CurrentlyDownloading != download)
                    {


                        DownloadQueue.Add(download);
                        OnDebugEvent?.Invoke(this, new BaseDebugArgs()
                        {
                            DebugMessage = "Added download to queue: " + download.ToString(),
                            DebugSource = this.GetType().Name,
                            DebugSourceType = 1,
                            DebugType = 3
                        });

                        JsonSuccesReport succes = new JsonSuccesReport()
                        {
                            message = "Succesfully added download to download que."
                        };

                        return succes.ToJson();
                    }
                    else
                    {

                        OnDebugEvent?.Invoke(this, new BaseDebugArgs()
                        {
                            DebugMessage = "Could not add download: " + download.ToString() + ", already exist in queue or is already being downloaded ",
                            DebugSource = this.GetType().Name,
                            DebugSourceType = 1,
                            DebugType = 3
                        });
                        JsonError error = new JsonError()
                        {
                            type = "unsufficient_space_error",
                            errormessage = "Could not add download: " + download.ToString() + ", already exist in queue or is already being downloaded ",
                            errortype = "warning",
                            exception = "none"
                        };
                        return error.ToJson();
                    }

                }
                else
                {
                    OnDebugEvent?.Invoke(this, new BaseDebugArgs()
                    {
                        DebugMessage = "Could not add download with filesize: " + download.filesize + " due to insufficient space required: " + (UtilityMethods.GetFreeSpace(IrcSettings.fullfilepath) / 1024 / 1024).ToString(),
                        DebugSource = this.GetType().Name,
                        DebugSourceType = 1,
                        DebugType = 3
                    });

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
                OnDebugEvent?.Invoke(this, new BaseDebugArgs()
                {
                    DebugSource = this.GetType().Name,
                    DebugMessage = e.ToString(),
                    DebugSourceType = 1,
                    DebugType = 4
                });


                JsonError error = new JsonError()
                {
                    type = "add_download_error",
                    errormessage = "Could not add download to que.",
                    errortype = "exception",
                    exception = e.ToString()
                };
                return error.ToJson();
            }
        }

        public string RemoveDownload(string id = null, string filepath = null)
        {
            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugMessage = "RemoveDownload Called.",
                DebugSource = this.GetType().Name,
                DebugSourceType = 1,
                DebugType = 0
            });
            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugMessage = id,
                DebugSource = this.GetType().Name,
                DebugSourceType = 1,
                DebugType = 1
            });

            try {

                int index = -1;

                foreach (JsonDownloadInfo queuedDownload in DownloadQueue)
                {
                    if (id != null)
                    {
                        if (queuedDownload.id == id)
                        {
                            DownloadQueue.RemoveAt(index);
                            break;
                        }
                    }
                    else if (filepath != null)
                    {
                        if (Path.Combine(queuedDownload.fullfilepath, queuedDownload.filename) == filepath)
                        {
                            DownloadQueue.RemoveAt(index);
                            break;
                        }
                    }
                    else
                    {
                        JsonError error = new JsonError()
                        {
                            type = "remove_download_error",
                            errormessage = "Could not remove download from que, neither id or filepath is defined.",
                            errortype = "warning"
                        };
                        return error.ToJson();
                    }
                    index++;
                }

                if (IrcClientHandler.IsDownloading())
                {
                    IrcClientHandler.StopDownload();
                }
                JsonSuccesReport succes = new JsonSuccesReport()
                {
                    message = "Succesfully removed download from download que by download json."
                };

                return succes.ToJson();
            }
            catch (Exception e)
            {
                OnDebugEvent?.Invoke(this, new BaseDebugArgs()
                {
                    DebugSource = this.GetType().Name,
                    DebugMessage = e.ToString(),
                    DebugSourceType = 1,
                    DebugType = 4
                });

                JsonError error = new JsonError()
                {
                    type = "remove_download_error",
                    errormessage = "Could not remove download from que by download json.",
                    errortype = "exception"
                };
                return error.ToJson();
            }
        }


        public string StopQueue() {
            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugMessage = "StopQueue Called.",
                DebugSource = this.GetType().Name,
                DebugSourceType = 1,
                DebugType = 0
            });
            Stop = true;

            JsonSuccesReport succes = new JsonSuccesReport()
            {
                message = "Succesfully told queue to stop running."
            };

            return succes.ToJson();
        }

        public string GetCurrentlyDownloading()
        {
            return CurrentlyDownloading.ToJson();
        }

        private void OnIrcClientDownloadEvent(object sender, IrcClientDownloadEventArgs args)
        {

            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugSource = this.GetType().Name + " via " + sender.GetType().Name + " via " + sender.GetType().Name,
                DebugMessage = "OnIrcClientDownloadEvent called.",
                DebugSourceType = 2,
                DebugType = 0
            });

            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugSource = this.GetType().Name + " via " + sender.GetType().Name,
                DebugMessage = args.ToString(),
                DebugSourceType = 2,
                DebugType = 1
            });

            if (CurrentlyDownloading != null)
            {

                if (args.DownloadStatus == "PARSING" || args.DownloadStatus == "WAITING")
                {
                    OnDownloadUpdateEvent?.Invoke(this, new DownloadUpdateEventArgs()
                    {
                        id = CurrentlyDownloading.id,
                        animeid = CurrentlyDownloading.animeInfo.animeid,
                        animeTitle = CurrentlyDownloading.animeInfo.title,
                        animeCoverSmall = CurrentlyDownloading.animeInfo.cover_small,
                        animeCoverOriginal = CurrentlyDownloading.animeInfo.cover_original,
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
                        animeid = CurrentlyDownloading.animeInfo.animeid,
                        animeTitle = CurrentlyDownloading.animeInfo.title,
                        animeCoverSmall = CurrentlyDownloading.animeInfo.cover_small,
                        animeCoverOriginal = CurrentlyDownloading.animeInfo.cover_original,
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


            }
            else
            {
                OnDebugEvent?.Invoke(this, new BaseDebugArgs()
                {
                    DebugSource = this.GetType().Name + " via " + sender.GetType().Name,
                    DebugMessage = "Got download event, but no CurrrentlyDownloading object has been set!",
                    DebugSourceType = 2,
                    DebugType = 3
                });
            }

            if (args.DownloadStatus == "COMPLETED" || args.DownloadStatus == "FAILED" || args.DownloadStatus == "ABORTED")
            {
                IsDownloading = false;
                CurrentlyDownloading = new JsonDownloadInfo();
            }
            else
            {
                IsDownloading = true;
            }


        }

        private async Task DownloadQueueHandler()
        {
            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugMessage = "DownloadQueueHandler Called.",
                DebugSource = this.GetType().Name,
                DebugSourceType = 3,
                DebugType = 0
            });

            int retries = 0;
            while (!Stop) {

                Thread.Sleep(500);
                if (DownloadQueue.Count > 0)
                {
                    JsonDownloadInfo toDownload = DownloadQueue[0];
                    if (retries > 2)
                    {
                        IsDownloading = false;
                        DownloadQueue.RemoveAt(0);
                        retries = 0;
                        OnDebugEvent?.Invoke(this, new BaseDebugArgs()
                        {
                            DebugMessage = "Could not start download after 3 tries :(, removing download from queue. ",
                            DebugSource = this.GetType().Name,
                            DebugSourceType = 3,
                            DebugType = 3
                        });

                        OnDownloadUpdateEvent?.Invoke(this, new DownloadUpdateEventArgs()
                        {
                            id = CurrentlyDownloading.id,
                            animeid = CurrentlyDownloading.animeInfo.animeid,
                            animeTitle = CurrentlyDownloading.animeInfo.title,
                            animeCoverSmall = CurrentlyDownloading.animeInfo.cover_small,
                            animeCoverOriginal = CurrentlyDownloading.animeInfo.cover_original,
                            episodeNumber = CurrentlyDownloading.episodeNumber,
                            bot = CurrentlyDownloading.bot,
                            pack = CurrentlyDownloading.pack,
                            progress = "0",
                            speed = "0",
                            status = "FAILED",
                            filename = CurrentlyDownloading.filename,
                            filesize = CurrentlyDownloading.filesize,
                            fullfilepath= CurrentlyDownloading.fullfilepath,
                            downloadIndex = CurrentlyDownloading.downloadIndex
                        });

                        CurrentlyDownloading = new JsonDownloadInfo();
                    }
                    else
                    {
                        if (!IrcClientHandler.IsDownloading())
                        {
                            IsDownloading = false;
                            Thread.Sleep(500);


                            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
                            {
                                DebugMessage = "Send following download to irc server: " + toDownload.ToString(),
                                DebugSource = this.GetType().Name,
                                DebugSourceType = 1,
                                DebugType = 3
                            });

                            CurrentlyDownloading = toDownload;
                            IrcClientHandler.StartDownload(toDownload);
                            int timeOutCount = 0;
                            bool timedOut = true;

                            while (timeOutCount < 5)
                            {
                                if (IrcClientHandler.IsDownloading())
                                {
                                    timedOut = false;
                                    break;
                                }
                                Thread.Sleep(1000);
                                timeOutCount++;
                            }

                            if (!timedOut)
                            {

                                OnDebugEvent?.Invoke(this, new BaseDebugArgs()
                                {
                                    DebugMessage = "Download succesfully initiated, wait for irc download event to be sure.",
                                    DebugSource = this.GetType().Name,
                                    DebugSourceType = 3,
                                    DebugType = 2
                                });

                                IsDownloading = true;
                                DownloadQueue.RemoveAt(0);
                            }
                            else
                            {

                                CurrentlyDownloading = new JsonDownloadInfo();
                                OnDebugEvent?.Invoke(this, new BaseDebugArgs()
                                {
                                    DebugMessage = "Could not start download :(, retrying.  ",
                                    DebugSource = this.GetType().Name,
                                    DebugSourceType = 3,
                                    DebugType = 3
                                });
                                retries++;
                            }
                        }
                    }                    
                }
            }            
        }

        public void SetIrcSettings(IrcSettings settings)
        {
            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugMessage = "SetIrcSettings Called.",
                DebugSource = this.GetType().Name,
                DebugSourceType = 1,
                DebugType = 0
            });
            IrcSettings = settings;
        }

        public void SetLittleWeebSettings(LittleWeebSettings settings)
        {
            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugMessage = "SetLittleWeebSettings Called.",
                DebugSource = this.GetType().Name,
                DebugSourceType = 1,
                DebugType = 0
            });
        }
    }
}
