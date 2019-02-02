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

        private readonly string fileHistoryPath = "";
        private readonly string fileName = "";

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

            JObject current_anime = await DataBaseHandler.GetJObject("downloads", "anime_id", downloadInfo.anime_id);

            if (current_anime == null)
            {
                current_anime = await DataBaseHandler.GetJObject("anime", "anime_id", downloadInfo.anime_id);
            }

            if (current_anime != null)
            {
                JArray currentlydownloaded = current_anime.Value<JArray>("anime_episodes_downloads");

                foreach (JObject downloaded in currentlydownloaded) {
                    if (downloaded.Value<string>("id") == downloadInfo.id)
                    {
                        return false;
                    }
                }

                currentlydownloaded.Add(downloadInfo);
                current_anime["anime_episodes_downloads"] = currentlydownloaded;

                await DataBaseHandler.StoreJObject("downloads", current_anime);
            }

            return true;
           
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

            JObject current_anime = await DataBaseHandler.GetJObject("downloads", "anime_id", anime_id);

            if (current_anime != null)
            {
                JArray currentlydownloaded = current_anime.Value<JArray>("anime_episodes_downloads");

                int indexToDelete = 0;
                bool found = false;
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

                    if (currentlydownloaded.Count > 0)
                    {
                        await DataBaseHandler.UpdateJObject("downloads", current_anime, "anime_id", anime_id);
                    }
                    else
                    {
                        await DataBaseHandler.RemoveJObject("downloads", "anime_id", anime_id);
                    }

                    return true;
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
