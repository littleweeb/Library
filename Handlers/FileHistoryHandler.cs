using LittleWeebLibrary.EventArguments;
using LittleWeebLibrary.GlobalInterfaces;
using LittleWeebLibrary.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Threading.Tasks;

namespace LittleWeebLibrary.Handlers
{
    public interface IFileHistoryHandler
    {
        Task<bool> AddFileToFileHistory(JsonDownloadedInfo downloadInfo);
        Task<bool> RemoveFileFromFileHistory(string anime_id, string id = null, string filepath = null);
        Task<JsonDownloadedList> GetCurrentFileHistory();
    }
    public class FileHistoryHandler : IFileHistoryHandler
    {     


        private readonly IDebugHandler DebugHandler;
        private readonly IDataBaseHandler DataBaseHandler;

        public FileHistoryHandler( IDataBaseHandler dataBaseHandler, IDebugHandler debugHandler)
        {
            debugHandler.TraceMessage("Constructor Called", DebugSource.CONSTRUCTOR, DebugType.ENTRY_EXIT);
            DebugHandler = debugHandler;
            DataBaseHandler = dataBaseHandler;
        }

        public async Task<bool> AddFileToFileHistory(JsonDownloadedInfo downloadInfo)
        {

            DebugHandler.TraceMessage("AddFileToFileHistory Called", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage(downloadInfo.ToString(), DebugSource.TASK, DebugType.PARAMETERS);

            JObject current_anime = await DataBaseHandler.GetJObject("anime", "anime_id", downloadInfo.anime_id);
            JObject current_anime_download_list = await DataBaseHandler.GetJObject("downloads", "anime_id", downloadInfo.anime_id);

            bool toreturn = false;

            if (current_anime.Count > 0)
            {
                JArray currentlydownloaded = current_anime.Value<JArray>("anime_episodes_downloads");

                foreach (JObject downloaded in currentlydownloaded) {
                    if (downloaded.Value<string>("id") == downloadInfo.id)
                    {
                        toreturn = false;
                        break;
                    }
                    else
                    {
                        toreturn = true;
                    }
                }

                if (toreturn)
                {
                    currentlydownloaded.Add(downloadInfo);
                    current_anime["anime_episodes_downloads"] = currentlydownloaded;

                    await DataBaseHandler.StoreJObject("anime", current_anime, downloadInfo.anime_id);

                    if (current_anime_download_list.Count > 0)
                    {
                        JArray currentDownloaded = current_anime_download_list.Value<JArray>("downloadHistorylist");

                        foreach (JObject download in currentDownloaded)
                        {
                            if (download.Value<string>("id") == downloadInfo.id)
                            {
                                toreturn = false;
                                break;
                            }
                            else
                            {
                                toreturn = true;
                            }
                        }

                        if (toreturn)
                        {
                            currentDownloaded.Add(downloadInfo);
                            current_anime_download_list["downloadHistorylist"] = currentDownloaded;
                            await DataBaseHandler.StoreJObject("downloads", current_anime_download_list, downloadInfo.anime_id);
                        }
                    }
                    else
                    {
                        JsonDownloadedList downloadedList = new JsonDownloadedList()
                        {
                            anime_id = downloadInfo.anime_id,
                            anime_cover = current_anime.Value<JObject>("anime_info").Value<JArray>("data")[0]["attributes"].Value<JObject>("coverImage"),
                            anime_title = downloadInfo.anime_name
                        };

                        downloadedList.downloadHistorylist.Add(downloadInfo);

                        await DataBaseHandler.StoreJObject("downloads", downloadedList.ToJObject(), downloadInfo.anime_id);
                        toreturn = true;
                    }
                }
              
            }
                    

            return toreturn;
           
        }

        public async Task<bool> RemoveFileFromFileHistory(string anime_id, string id = null, string filepath = null)
        {

            DebugHandler.TraceMessage("RemoveFileFromFileHistory Called", DebugSource.TASK, DebugType.ENTRY_EXIT);
            if (id != null)
            {
                DebugHandler.TraceMessage("ID: " + id, DebugSource.TASK, DebugType.PARAMETERS);
            }
            if (filepath != null)
            {
                DebugHandler.TraceMessage("Filepath: " + filepath, DebugSource.TASK, DebugType.PARAMETERS);
            }

            JObject current_anime = await DataBaseHandler.GetJObject("anime", "anime_id", anime_id);
            JObject current_anime_download_list = await DataBaseHandler.GetJObject("downloads", "anime_id", anime_id);

            bool found = false;
            if (current_anime.Count > 0)
            {
                JArray currentlydownloaded = current_anime.Value<JArray>("anime_episodes_downloads");

                int indexToDelete = 0;
                foreach (JObject downloaded in currentlydownloaded)
                {
                    if (downloaded.Value<string>("id") == id)
                    {
                        found = true;
                        break;
                    }

                    if (downloaded.Value<string>("filepath") == id)
                    {
                        found = true;
                        break;
                    }
                    indexToDelete++;
                }

                if (found)
                {
                    currentlydownloaded.RemoveAt(indexToDelete);
                    current_anime["anime_episodes_downloads"] = currentlydownloaded;
                    await DataBaseHandler.UpdateJObject("anime", current_anime, "anime_id", anime_id);


                    if (current_anime_download_list.Count > 0)
                    {
                        JArray currentDownloaded = current_anime_download_list.Value<JArray>("downloadHistorylist");
                        found = false;
                        int index = 0;
                        foreach (JObject downloaded in currentDownloaded)
                        {
                            if (downloaded.Value<string>("id") == id)
                            {
                                found = true;
                                break;
                            }

                            if (downloaded.Value<string>("filepath") == id)
                            {
                                found = true;
                                break;
                            }
                            index++;
                        }

                        currentDownloaded.RemoveAt(index);

                        if (currentDownloaded.Count > 0)
                        {
                            current_anime_download_list["downloadHistorylist"] = currentDownloaded;
                            await DataBaseHandler.UpdateJObject("downloads", current_anime_download_list, anime_id);
                        }
                        else
                        {
                            await DataBaseHandler.RemoveJObject("downloads", anime_id);
                        }

                    }
                }
            }

            return false;
        }
        

        public async Task<JsonDownloadedList> GetCurrentFileHistory()
        {

            DebugHandler.TraceMessage("GetCurrentFileHistory Called", DebugSource.TASK, DebugType.ENTRY_EXIT);

            JArray listWithDownloads = await DataBaseHandler.GetCollection("downloads");

            JsonDownloadedList list = new JsonDownloadedList()
            {
                downloadHistorylist = listWithDownloads
            };

            return list;
        }
    }
}
