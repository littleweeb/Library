using LittleWeebLibrary.EventArguments;
using LittleWeebLibrary.GlobalInterfaces;
using LittleWeebLibrary.Handlers;
using LittleWeebLibrary.Models;
using LittleWeebLibrary.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace LittleWeebLibrary.Controllers.SubControllers
{
    public class InfoApiWebSocketController : ISubWebSocketController
    {

       

        private readonly IWebSocketHandler WebSocketHandler;
        private readonly IInfoApiWebSocketService InfoApiWebSocketService;
        private readonly IDebugHandler DebugHandler;

        public InfoApiWebSocketController(IWebSocketHandler webSocketHandler, IInfoApiWebSocketService infoApiWebSocketService, IDebugHandler debugHandler)
        {
            debugHandler.TraceMessage("Constructor Called.", DebugSource.CONSTRUCTOR, DebugType.ENTRY_EXIT);
            DebugHandler = debugHandler;
            InfoApiWebSocketService = infoApiWebSocketService;
            WebSocketHandler = webSocketHandler;
        }


        public void OnWebSocketEvent(WebSocketEventArgs args)
        {

            DebugHandler.TraceMessage("OnWebSocketEvent called", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage(args.ToString(), DebugSource.TASK, DebugType.PARAMETERS);


            try
            {
                try
                {
                    JObject query = JObject.Parse(args.Message);
                    string action = query.Value<string>("action");

                    if (action != null)
                    {
                        JObject extra = query.Value<JObject>("extra");

                        if (extra != null)
                        {
                            switch (action)
                            {
                                case "search_nibl":
                                    InfoApiWebSocketService.GetFilesForAnime(extra);
                                    break;
                                case "get_anime_profile":
                                    InfoApiWebSocketService.GetAnimeProfile(extra);
                                    break;
                                case "get_anime_episodes":
                                    InfoApiWebSocketService.GetAnimeEpisodes(extra);
                                    break;
                                case "get_currently_airing":
                                    InfoApiWebSocketService.GetCurrentlyAiring(extra);
                                    break;
                                case "search_kitsu":
                                    InfoApiWebSocketService.SearchKitsu(extra);
                                    break;
                            }
                        }
                        else
                        {
                            switch (action)
                            {
                                case "get_currently_airing":
                                    InfoApiWebSocketService.GetCurrentlyAiring();
                                    break;                               
                            }
                        }
                    }
                }
                catch (JsonReaderException e)
                {
                    DebugHandler.TraceMessage(e.ToString(), DebugSource.TASK, DebugType.WARNING);

                    JsonError error = new JsonError()
                    {
                        type = "command_error",
                        errormessage = "Error happend during parsing of command.",
                        errortype = "exception",
                        exception = e.ToString()
                    };
                    WebSocketHandler.SendMessage(error.ToJson());
                }
               
            }
            catch (Exception e)
            {

                DebugHandler.TraceMessage(e.ToString(), DebugSource.TASK, DebugType.ERROR);

                JsonError error = new JsonError()
                {
                    type = "command_error",
                    errormessage = "Error happend during execution of command.",
                    errortype = "exception",
                    exception = e.ToString()
                };
                WebSocketHandler.SendMessage(error.ToJson());
            }
        }
    }
}
