using LittleWeebLibrary.EventArguments;
using LittleWeebLibrary.GlobalInterfaces;
using LittleWeebLibrary.Models;
using LittleWeebLibrary.Settings;
using LittleWeebLibrary.StaticClasses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace LittleWeebLibrary.Handlers
{
    public interface IDownloadHandler
    {
        event EventHandler<DownloadUpdateEventArgs> OnDownloadUpdateEvent;
        string AddDownload(JsonDownloadInfo download);
        string AddDownloads(List<JsonDownloadInfo> download);
        string AbortDownload(string id = null, string filePath = null);
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
        private bool DownloadProcesOnGoing = false;
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
                if (download.filesize.Contains(".")){
                    download.filesize = ((int)(double.Parse(download.filesize, System.Globalization.CultureInfo.InvariantCulture) * 1024)).ToString();
                }
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

                int index = 0;

                foreach (JsonDownloadInfo queuedDownload in DownloadQueue)
                {
                    if (id != null)
                    {
                        if (queuedDownload.id == id)
                        {
                            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
                            {
                                DebugMessage = "Removed download at index: " + index.ToString() + " using id: " + id,
                                DebugSource = this.GetType().Name,
                                DebugSourceType = 1,
                                DebugType = 3
                            });

                            DownloadQueue.RemoveAt(index);
                            break;
                        }
                    }
                    else if (filepath != null)
                    {
                        if (Path.Combine(queuedDownload.fullfilepath, queuedDownload.filename) == filepath)
                        {

                            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
                            {
                                DebugMessage = "Removed download at index: " + index.ToString() + " using filepath: " + filepath,
                                DebugSource = this.GetType().Name,
                                DebugSourceType = 1,
                                DebugType = 3
                            });

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


                if (args.DownloadStatus.Contains("COMPLETED"))
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


                    if (CurrentlyDownloading.id != string.Empty)
                    {
                        RemoveDownload(CurrentlyDownloading.id);

                        CurrentlyDownloading = new JsonDownloadInfo();
                    }


                    DownloadProcesOnGoing = false;
                } else if (args.DownloadStatus.Contains("FAILED") || args.DownloadStatus.Contains("ABORTED"))
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
                OnDebugEvent?.Invoke(this, new BaseDebugArgs()
                {
                    DebugSource = this.GetType().Name + " via " + sender.GetType().Name,
                    DebugMessage = "Got download event, but no CurrrentlyDownloading object has been set!",
                    DebugSourceType = 2,
                    DebugType = 3
                });
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

            await Task.Run(() =>
            {
                while (!Stop)
                {

                    if (DownloadQueue.Count > 0 && !DownloadProcesOnGoing)
                    {
                        DownloadProcesOnGoing = true;
                        CurrentlyDownloading = DownloadQueue[0];

                        OnDebugEvent?.Invoke(this, new BaseDebugArgs()
                        {
                            DebugMessage = "Requesting the following download: " + CurrentlyDownloading.ToJson(), 
                            DebugSource = this.GetType().Name,
                            DebugSourceType = 1,
                            DebugType = 3
                        });

                        IrcClientHandler.StartDownload(CurrentlyDownloading);                      

                    }

                    Thread.Sleep(500);
                }
            });

            await Task.Run(() =>
            {
                while (!Stop)
                {

                    if (DownloadProcesOnGoing)
                    {

                        OnDebugEvent?.Invoke(this, new BaseDebugArgs()
                        {
                            DebugMessage = "Currently Busy Downloading: " + CurrentlyDownloading.ToJson(),
                            DebugSource = this.GetType().Name,
                            DebugSourceType = 1,
                            DebugType = 3
                        });

                    }
                    else
                    {
                        OnDebugEvent?.Invoke(this, new BaseDebugArgs()
                        {
                            DebugMessage = "Currently Waiting For Download to start with queue length: " + DownloadQueue.Count.ToString(),
                            DebugSource = this.GetType().Name,
                            DebugSourceType = 1,
                            DebugType = 3
                        });


                        Thread.Sleep(10000);
                    }

                    Thread.Sleep(1000);
                }
            });

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

        public string AbortDownload(string id = null, string filePath = null)
        {
            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugMessage = "AbortDownload Called.",
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

            try
            {

                if (IrcClientHandler.IsDownloading() && DownloadProcesOnGoing)
                {
                    if (CurrentlyDownloading.id == id)
                    {
                        IrcClientHandler.StopDownload();
                    }
                }

                JsonSuccesReport succes = new JsonSuccesReport()
                {
                    message = "Succesfully aborted download from download que by download json."
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

        public string AddDownloads(List<JsonDownloadInfo> download)
        {
            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugMessage = "AddDownloads Called.",
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

            try{

                long totalSizeNeeded = 0;
                foreach (JsonDownloadInfo downloadinfo in download)
                {
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

                    JsonSuccesReport succes = new JsonSuccesReport()
                    {
                        message = "Succesfully added " + download.Count + " downloads to download queue."
                    };

                    return succes.ToJson();
                }
                else
                {
                    OnDebugEvent?.Invoke(this, new BaseDebugArgs()
                    {
                        DebugMessage = "Could not add downloads with filesize: " + totalSizeNeeded + " due to insufficient space required: " + (UtilityMethods.GetFreeSpace(IrcSettings.fullfilepath) / 1024 / 1024).ToString(),
                        DebugSource = this.GetType().Name,
                        DebugSourceType = 1,
                        DebugType = 3
                    });

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
    }
}
