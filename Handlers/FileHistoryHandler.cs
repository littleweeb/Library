using LittleWeebLibrary.EventArguments;
using LittleWeebLibrary.GlobalInterfaces;
using LittleWeebLibrary.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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

        public FileHistoryHandler(IDataBaseHandler dataBaseHandler, IDebugHandler debugHandler)
        {
            debugHandler.TraceMessage("Constructor Called", DebugSource.CONSTRUCTOR, DebugType.ENTRY_EXIT);
            DebugHandler = debugHandler;
            DataBaseHandler = dataBaseHandler;

            Init();
        }

        public async void Init()
        {
            JObject downloaded_anime = await DataBaseHandler.GetJObject("downloads", "list_with_downloads");

            if (downloaded_anime.Count == 0)
            {
                downloaded_anime = new JObject();
                downloaded_anime["anime_with_downloads"] = new JArray();
                await DataBaseHandler.StoreJObject("downloads", downloaded_anime, "list_with_downloads");
            }
        }

        public async Task<bool> AddFileToFileHistory(JsonDownloadedInfo downloadInfo)
        {

            DebugHandler.TraceMessage("AddFileToFileHistory Called", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage(downloadInfo.ToString(), DebugSource.TASK, DebugType.PARAMETERS);

            JObject current_anime = await DataBaseHandler.GetJObject("anime", "anime_id", downloadInfo.anime_id);
            //JObject current_anime_download_list = await DataBaseHandler.GetJObject("downloads", "anime_id", downloadInfo.anime_id);
            JObject downloaded_anime = await DataBaseHandler.GetJObject("downloads", "list_with_downloads");
            JArray current_anime_download_list = downloaded_anime.Value<JArray>("anime_with_downloads");

            if (current_anime_download_list == null)
            {
                current_anime_download_list = new JArray();
            }

            bool toreturn = true;

            if (current_anime.Count > 0)
            {
                DebugHandler.TraceMessage("Found Anime with id: " + downloadInfo.anime_id, DebugSource.TASK, DebugType.INFO);

                JArray currentlydownloaded = current_anime.Value<JArray>("anime_downloads");

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
                    currentlydownloaded.Add(downloadInfo.ToJObject());
                    current_anime["anime_downloads"] = currentlydownloaded;

                    await DataBaseHandler.UpdateJObject("anime", current_anime, downloadInfo.anime_id);


                    if (!current_anime_download_list.Contains(downloadInfo.anime_id))
                    {
                        current_anime_download_list.Add(downloadInfo.anime_id);
                    }

                    downloaded_anime["anime_with_downloads"] = current_anime_download_list;

                    await DataBaseHandler.UpdateJObject("downloads", downloaded_anime, "list_with_downloads");
                   
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
            //JObject current_anime_download_list = await DataBaseHandler.GetJObject("downloads", "anime_id", downloadInfo.anime_id);
            JObject downloaded_anime = await DataBaseHandler.GetJObject("downloads",  "list_with_downloads");
            JArray current_anime_download_list = downloaded_anime.Value<JArray>("anime_with_downloads");

            bool found = false;
            if (current_anime.Count > 0)
            {
                JArray currentlydownloaded = current_anime.Value<JArray>("anime_downloads");

                int indexToDelete = 0;
                foreach (JObject downloaded in currentlydownloaded)
                {
                    if (downloaded.Value<string>("id") == id || downloaded.Value<string>("filepath") == filepath)
                    {
                        found = true;
                        break;
                    }
                    indexToDelete++;
                }

                if (found)
                {
                    currentlydownloaded.RemoveAt(indexToDelete);
                    current_anime["anime_downloads"] = currentlydownloaded;
                    await DataBaseHandler.UpdateJObject("anime", current_anime, "anime_id", anime_id);


                    if (current_anime_download_list.Count == 0)
                    {
                        if (current_anime_download_list.Contains(anime_id))
                        {
                            current_anime_download_list.Remove(anime_id);

                            downloaded_anime["anime_with_downloads"] = current_anime_download_list;

                            await DataBaseHandler.UpdateJObject("downloads", downloaded_anime, "list_with_downloads");
                        }
                    }
                }
            }

            return false;
        }
        

        public async Task<JsonDownloadedList> GetCurrentFileHistory()
        {

            DebugHandler.TraceMessage("GetCurrentFileHistory Called", DebugSource.TASK, DebugType.ENTRY_EXIT);

            JObject downloaded_animes = await DataBaseHandler.GetJObject("downloads", "list_with_downloads");
            JArray current_anime_download_list = downloaded_animes.Value<JArray>("anime_with_downloads");

            List<JObject> downloaded = new List<JObject>();

            foreach (string id in current_anime_download_list)
            {
                JObject downloaded_anime = await DataBaseHandler.GetJObject("anime", "anime_id", id);
                downloaded.Add(downloaded_anime);
            }


            JsonDownloadedList list = new JsonDownloadedList()
            {
                downloaded_anime = downloaded
            };

            return list;
        }
    }
}
