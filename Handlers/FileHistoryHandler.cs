using LittleWeebLibrary.EventArguments;
using LittleWeebLibrary.GlobalInterfaces;
using LittleWeebLibrary.Models;
using Newtonsoft.Json;
using System;
using System.IO;

namespace LittleWeebLibrary.Handlers
{
    public interface IFileHistoryHandler
    {
        void AddFileToFileHistory(JsonDownloadInfo downloadInfo);
        string RemoveFileFromFileHistory(string id = null, string filepath = null);
        JsonDownloadHistoryList GetCurrentFileHistory();
    }
    public class FileHistoryHandler : IFileHistoryHandler
    {
       

        private readonly string fileHistoryPath = "";
        private readonly string fileName = "";

        private readonly IDebugHandler DebugHandler;

        public FileHistoryHandler(IDebugHandler debugHandler)
        {

            debugHandler.TraceMessage("Constructor Called", DebugSource.CONSTRUCTOR, DebugType.ENTRY_EXIT);
            DebugHandler = debugHandler;

#if __ANDROID__
            fileHistoryPath = Path.Combine(Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, "LittleWeeb"), "FileHistory");
#else
            fileHistoryPath = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "LittleWeeb"), "FileHistory");
#endif
            fileName = "FileHistory.json";

            if (!Directory.Exists(fileHistoryPath))
            {
                DebugHandler.TraceMessage("File History directory does not exist, creating: " + fileHistoryPath, DebugSource.CONSTRUCTOR, DebugType.INFO);
                Directory.CreateDirectory(fileHistoryPath);
            }

            if (!File.Exists(Path.Combine(fileHistoryPath, fileName))) {

                DebugHandler.TraceMessage("File History file does not exist, creating: " + Path.Combine(fileHistoryPath, fileName), DebugSource.CONSTRUCTOR, DebugType.INFO);
                File.Create(Path.Combine(fileHistoryPath, fileName));
            }


        }

        public void AddFileToFileHistory(JsonDownloadInfo downloadInfo)
        {

            DebugHandler.TraceMessage("AddFileToFileHistory Called", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage(downloadInfo.ToString(), DebugSource.TASK, DebugType.PARAMETERS);
           

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

                    if (downloadHistoryObject.animeInfo.anime_id == downloadInfo.animeInfo.anime_id)
                    {
                        animeAlreadyExists = true;

                        int downloadIndex = 0;
                        foreach (JsonDownloadInfo info in downloadHistoryObject.downloadHistory)
                        {
                            if (info.id == downloadInfo.id || info.filename == downloadInfo.filename || (info.pack == downloadInfo.pack && info.bot == downloadInfo.bot))
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

            DebugHandler.TraceMessage("RemoveFileFromFileHistory Called", DebugSource.TASK, DebugType.ENTRY_EXIT);


            if (id != null)
            {
                DebugHandler.TraceMessage("ID: " + id , DebugSource.TASK, DebugType.PARAMETERS);
            }
            if (filepath != null)
            {
                DebugHandler.TraceMessage("Filepath: " + filepath, DebugSource.TASK, DebugType.PARAMETERS);
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

            DebugHandler.TraceMessage("GetCurrentFileHistory Called", DebugSource.TASK, DebugType.ENTRY_EXIT);
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
