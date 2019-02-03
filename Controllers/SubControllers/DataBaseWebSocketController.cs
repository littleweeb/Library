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
    public class DataBaseWebSocketController : ISubWebSocketController
    {       

        private readonly IWebSocketHandler WebSocketHandler;
        private readonly IDataBaseWebSocketService DataBaseWebSocketService;
        private readonly IDebugHandler DebugHandler;

        public DataBaseWebSocketController(IWebSocketHandler webSocketHandler, IDataBaseWebSocketService dataBaseWebSocketService, IDebugHandler debugHandler)
        {
            debugHandler.TraceMessage("Constructor Called.", DebugSource.CONSTRUCTOR, DebugType.ENTRY_EXIT);
            DebugHandler = debugHandler;
            DataBaseWebSocketService = dataBaseWebSocketService;
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
                                case "get_collection":
                                    DataBaseWebSocketService.GetCollection(extra);
                                    break;
                                case "get_document":
                                    DataBaseWebSocketService.GetDocument(extra);
                                    break;
                                case "store_document":
                                    DataBaseWebSocketService.StoreDocument(extra);
                                    break;
                                case "delete_document":
                                    DataBaseWebSocketService.DeleteDocument(extra);
                                    break;
                                case "update_document":
                                    DataBaseWebSocketService.UpdateDocument(extra);
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
