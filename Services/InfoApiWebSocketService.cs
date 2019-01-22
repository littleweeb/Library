using LittleWeebLibrary.EventArguments;
using LittleWeebLibrary.GlobalInterfaces;
using LittleWeebLibrary.Handlers;
using LittleWeebLibrary.Models;
using LittleWeebLibrary.Settings;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WeebFileNameParserLibrary;

namespace LittleWeebLibrary.Services
{
    public interface IInfoApiWebSocketService
    {
        Task GetCurrentlyAiring();
        Task GetAnimeProfile(JObject query);
        Task GetFilesForAnime(JObject query);
    }
    public class InfoApiWebSocketService : IInfoApiWebSocketService
    {
       

        private readonly IAnimeProfileHandler AnimeProfileHandler;
        private readonly INiblHandler NiblHandler;
        private readonly IWebSocketHandler WebSocketHandler;
        private readonly IDebugHandler DebugHandler;
        private WeebFileNameParser WeebFileNameParser;

        public InfoApiWebSocketService(IWebSocketHandler webSocketHandler, IAnimeProfileHandler infoApiHandler, INiblHandler niblHandler, IDebugHandler debugHandler)
        {
            debugHandler.TraceMessage("Constructor Called.", DebugSource.CONSTRUCTOR, DebugType.ENTRY_EXIT);

            WebSocketHandler = webSocketHandler;
            AnimeProfileHandler = infoApiHandler;
            NiblHandler = niblHandler;
            DebugHandler = debugHandler;

            WeebFileNameParser = new WeebFileNameParser();
        }

        public async Task GetAnimeProfile(JObject query)
        {
            DebugHandler.TraceMessage("GetAnimeProfile Called.", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage("Query: " + query.ToString(), DebugSource.TASK, DebugType.PARAMETERS);

            string id = query.Value<string>("id");
            JsonKitsuAnimeInfo info = await AnimeProfileHandler.GetAnimeProfile(id);

            await WebSocketHandler.SendMessage(info.ToJson());

        }

        public async Task GetFilesForAnime(JObject query)
        {
            DebugHandler.TraceMessage("GetFilesForAnime Called.", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage("Query: " + query.ToString(), DebugSource.TASK, DebugType.PARAMETERS);

            try
            {
                JArray animeNames = query.Value<JArray>("titles");

                JObject returnresult = new JObject();

                foreach (string title in animeNames)
                {
                    Dictionary<string, string> parsed = WeebFileNameParser.ParseFullString(title);

                    string searchQuery = parsed["MainAnimeTitle"];

                    if (parsed["SubAnimeTitle"].Length > 0)
                    {
                        searchQuery += " " + parsed["SubAnimeTitle"];
                    }

                    JObject result = await NiblHandler.SearchNibl(searchQuery);
                    returnresult.Add(title, result);
                }


                await WebSocketHandler.SendMessage(returnresult.ToString());
            }
            catch (Exception e)
            {
                DebugHandler.TraceMessage("Failed to get files for requested anime(s): " + e.ToString(), DebugSource.TASK, DebugType.WARNING);

                JsonError jsonError = new JsonError
                {
                    errortype = "Exception",
                    errormessage = "failed to get files for requested anime(s)",
                    exception = e.ToString()
                };

                await WebSocketHandler.SendMessage(jsonError.ToString());

            }
            



        }

        public  async Task GetCurrentlyAiring()
        {
            DebugHandler.TraceMessage("GetCurrentlyAiring Called.", DebugSource.TASK, DebugType.ENTRY_EXIT);

            try
            {

                JsonCurrentlyAiring currentlyAiring = await AnimeProfileHandler.GetCurrentlyAiring();

                await WebSocketHandler.SendMessage(currentlyAiring.ToJson());

            }
            catch (Exception e)
            {

                DebugHandler.TraceMessage(e.ToString(), DebugSource.TASK, DebugType.ERROR);
                JsonError error = new JsonError()
                {
                    type = "get_currently_airing_error",
                    errormessage = "Could not get currently airing.",
                    errortype = "exception",
                    exception = e.ToString()
                };

                await WebSocketHandler.SendMessage(error.ToJson());
            }
        }

       
    }
}
