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
    public class SettingsWebSocketController : ISubWebSocketController
    {

       

        private readonly ISettingsWebSocketService SettingsWebSocketService;
        private readonly IWebSocketHandler WebSocketHandler;
        private readonly IDebugHandler DebugHandler;

        public SettingsWebSocketController(IWebSocketHandler webSocketHandler, ISettingsWebSocketService settingsWebSocketService, IDebugHandler debugHandler)
        {
            debugHandler.TraceMessage("Constructor Called.", DebugSource.CONSTRUCTOR, DebugType.ENTRY_EXIT);
            DebugHandler = debugHandler;
            SettingsWebSocketService = settingsWebSocketService;
            WebSocketHandler = webSocketHandler;
        }


        public void OnWebSocketEvent(WebSocketEventArgs args)
        {
            DebugHandler.TraceMessage("OnWebSocketEvent Called", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage(args.ToString(), DebugSource.TASK, DebugType.PARAMETERS);


            try
            {
                try{
                    JObject query = JObject.Parse(args.Message);
                    string action = query.Value<string>("action");

                    if (action != null)
                    {
                        JObject extra = query.Value<JObject>("extra");

                        if (extra != null)
                        {
                            switch (action)
                            {
                                case "set_littleweeb_settings":
                                    SettingsWebSocketService.SetLittleWeebSettings(extra);
                                    break;
                                case "set_irc_settings":
                                    SettingsWebSocketService.SetIrcSettings(extra);
                                    break;
                                case "set_download_directory":
                                    SettingsWebSocketService.Setfullfilepath(extra);
                                    break;
                            }
                        }
                        else
                        {
                            switch (action)
                            {

                                case "get_littleweeb_settings":
                                    SettingsWebSocketService.GetCurrentLittleWeebSettings();
                                    break;
                                case "get_irc_settings":
                                    SettingsWebSocketService.GetCurrentIrcSettings();
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
