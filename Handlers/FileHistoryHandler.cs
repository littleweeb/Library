using LittleWeebLibrary.EventArguments;
using LittleWeebLibrary.GlobalInterfaces;
using LittleWeebLibrary.Models;
using LittleWeebLibrary.Settings;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LittleWeebLibrary.Handlers
{
    public interface IFileHistoryHandler
    {
        void AddFileToFileHistory(JsonDownloadInfo downloadInfo);
        string RemoveFileFromFileHistory(string id = null, string filepath = null);
        JsonDownloadHistoryList GetCurrentFileHistory();
    }
    public class FileHistoryHandler : IFileHistoryHandler, IDebugEvent
    {
        public event EventHandler<BaseDebugArgs> OnDebugEvent;

        private readonly string fileHistoryPath = "";
        private readonly string fileName = "";

        public FileHistoryHandler()
        {
            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugMessage = "Constructor called.",
                DebugSource = this.GetType().Name,
                DebugSourceType = 0,
                DebugType = 0
            });

#if __ANDROID__
            fileHistoryPath = Path.Combine(Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, "LittleWeeb"), "FileHistory");
#else
            fileHistoryPath = Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LittleWeeb"), "FileHistory");
#endif
            fileName = "FileHistory.json";

            if (!Directory.Exists(fileHistoryPath))
            {
                Directory.CreateDirectory(fileHistoryPath);

            }

           

        }

        public void AddFileToFileHistory(JsonDownloadInfo downloadInfo)
        {
            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugMessage = "AddFileToFileHistory called.",
                DebugSource = this.GetType().Name,
                DebugSourceType = 1,
                DebugType = 0
            });

            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugMessage = downloadInfo.ToString(),
                DebugSource = this.GetType().Name,
                DebugSourceType = 1,
                DebugType = 1
            });

            if (!File.Exists(Path.Combine(fileHistoryPath, fileName)))
            {

                JsonDownloadHistory downloadHistoryObj = new JsonDownloadHistory()
                {
                    animeInfo = downloadInfo.animeInfo
                };

                downloadHistoryObj.downloadHistory.Add(downloadInfo);

                JsonDownloadHistoryList list = new JsonDownloadHistoryList();
                if (!list.downloadHistorylist.Contains(downloadHistoryObj))
                {
                    list.downloadHistorylist.Add(downloadHistoryObj);

                    using (var fileStream = File.Open(Path.Combine(fileHistoryPath, fileName), FileMode.CreateNew, FileAccess.ReadWrite, FileShare.ReadWrite))
                    {
                        using (var streamWriter = new StreamWriter(fileStream))
                        {
                            streamWriter.Write(list.ToJson());
                        }
                    }
                }
            }
            else
            {
                JsonDownloadHistoryList list = GetCurrentFileHistory();
              


                bool animeAlreadyExists = false;
                bool fileAlreadyExists = false;


                int listIndex = 0;
                foreach (JsonDownloadHistory downloadHistoryObject in list.downloadHistorylist)
                {

                    if (downloadHistoryObject.animeInfo.animeid == downloadInfo.animeInfo.animeid)
                    {
                        animeAlreadyExists = true;

                        int downloadIndex = 0;
                        foreach (JsonDownloadInfo info in downloadHistoryObject.downloadHistory)
                        {
                            if (info.id == downloadInfo.id)
                            {
                                list.downloadHistorylist[listIndex].downloadHistory[downloadIndex] = downloadInfo;
                                fileAlreadyExists = true;
                                break;
                            }
                            downloadIndex++;
                        }

                        if (!fileAlreadyExists)
                        {
                            list.downloadHistorylist[listIndex].downloadHistory.Add(downloadInfo);
                        }
                        break;
                    }
                    listIndex++;
                }

                if (!fileAlreadyExists && !animeAlreadyExists)
                {
                    JsonDownloadHistory downloadHistoryObj = new JsonDownloadHistory()
                    {
                        animeInfo = downloadInfo.animeInfo
                    };

                    downloadHistoryObj.downloadHistory.Add(downloadInfo);

                    list.downloadHistorylist.Add(downloadHistoryObj);
                }

                using (var fileStream = File.Open(Path.Combine(fileHistoryPath, fileName), FileMode.Truncate, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    using (var streamWriter = new StreamWriter(fileStream))
                    {
                        streamWriter.Write(list.ToJson());
                    }
                }
            }
        }

        public string RemoveFileFromFileHistory(string id = null, string filepath = null)
        {
            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugMessage = "RemoveFileFromFileHistory called.",
                DebugSource = this.GetType().Name,
                DebugSourceType = 1,
                DebugType = 0
            });

            if (id != null)
            {

                OnDebugEvent?.Invoke(this, new BaseDebugArgs()
                {
                    DebugMessage = id,
                    DebugSource = this.GetType().Name,
                    DebugSourceType = 1,
                    DebugType = 1
                });
            }
            if (filepath != null)
            {

                OnDebugEvent?.Invoke(this, new BaseDebugArgs()
                {
                    DebugMessage = filepath,
                    DebugSource = this.GetType().Name,
                    DebugSourceType = 1,
                    DebugType = 1
                });
            }

            if (File.Exists(Path.Combine(fileHistoryPath, fileName)))
            {

                string fileRemovedFromList = null;

                JsonDownloadHistoryList list = GetCurrentFileHistory();

                int indexList =0;
                int indexDownloadInfo = -1;
                bool downloadFound = false;

                foreach (JsonDownloadHistory history in list.downloadHistorylist)
                {
                    indexDownloadInfo = 0;
                    foreach (JsonDownloadInfo download in history.downloadHistory)
                    {
                        if (filepath != null)
                        {
                            if (Path.Combine(download.fullfilepath, download.filename) == filepath)
                            {
                                fileRemovedFromList = download.fullfilepath;
                                downloadFound = true;
                                break;
                            }
                        }
                        else if (id != null)
                        {

                            if (download.id == id)
                            {
                                fileRemovedFromList = download.fullfilepath;
                                downloadFound = true;
                                break;
                            }
                        }
                        else
                        {
                            downloadFound = false;
                            break;
                        }
                        indexDownloadInfo++;
                    }

                    if (downloadFound)
                    {
                        break;
                    }

                    indexList++;
                }

                if (downloadFound)
                {
                    list.downloadHistorylist[indexList].downloadHistory.RemoveAt(indexDownloadInfo);

                    if (list.downloadHistorylist[indexList].downloadHistory.Count == 0)
                    {
                        list.downloadHistorylist.RemoveAt(indexList);
                    }
                }
                using (var fileStream = File.Open(Path.Combine(fileHistoryPath, fileName), FileMode.Truncate, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    using (var streamWriter = new StreamWriter(fileStream))
                    {
                        streamWriter.Write(list.ToJson());
                    }
                }

                return fileRemovedFromList;
            }
            else
            {
                return null;
            }
        }
        

        public JsonDownloadHistoryList GetCurrentFileHistory()
        {
            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugMessage = "GetCurrentFileHistory called.",
                DebugSource = this.GetType().Name,
                DebugSourceType = 1, 
                DebugType = 0
            });

            string readContent = "";
            using (var fileStream = File.Open(Path.Combine(fileHistoryPath, fileName), FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                using (var streamReader = new StreamReader(fileStream))
                {
                    readContent = streamReader.ReadToEnd();
                }
            }

            return JsonConvert.DeserializeObject<JsonDownloadHistoryList>(readContent);
        }
    }
}
