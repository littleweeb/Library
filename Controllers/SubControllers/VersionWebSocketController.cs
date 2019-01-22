using LittleWeebLibrary.EventArguments;
using LittleWeebLibrary.GlobalInterfaces;
using LittleWeebLibrary.Handlers;
using LittleWeebLibrary.Models;
using LittleWeebLibrary.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace LittleWeebLibrary.Controllers.SubControllers
{
    public class VersionWebSocketController : ISubWebSocketController
    {
       

        private readonly IVersionWebSocketService VersionWebSocketService;
        private readonly IWebSocketHandler WebSocketHandler;
        private readonly IDebugHandler DebugHandler;

        public VersionWebSocketController(IWebSocketHandler webSocketHandler, IVersionWebSocketService versionWebSocketService, IDebugHandler debugHandler)
        {

            debugHandler.TraceMessage("Constructor Called.", DebugSource.CONSTRUCTOR, DebugType.ENTRY_EXIT);
            DebugHandler = debugHandler;
            VersionWebSocketService = versionWebSocketService;
            WebSocketHandler = webSocketHandler;
        }

        public void OnWebSocketEvent(WebSocketEventArgs args)
        {

            DebugHandler.TraceMessage("OnWebSocketEvent Called", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage(args.ToString(), DebugSource.TASK, DebugType.PARAMETERS);

            try
            {
                try
                {
                    JObject query = JObject.Parse(args.Message);
                    string action = query.Value<string>("action");

                    if (action != null)
                    {

                        switch (action)
                        {

                            case "check_version":
                                VersionWebSocketService.CheckVersion();
                                break;
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
