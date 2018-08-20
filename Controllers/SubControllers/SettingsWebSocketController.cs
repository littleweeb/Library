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
    public class SettingsWebSocketController : ISubWebSocketController, IDebugEvent
    {

        public event EventHandler<BaseDebugArgs> OnDebugEvent;

        private readonly ISettingsWebSocketService SettingsWebSocketService;
        private readonly IWebSocketHandler WebSocketHandler;

        public SettingsWebSocketController(IWebSocketHandler webSocketHandler, ISettingsWebSocketService settingsWebSocketService)
        {
            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugSource = this.GetType().Name,
                DebugMessage = "SettingsWebSocketController called.",
                DebugSourceType = 0,
                DebugType = 0
            });

            SettingsWebSocketService = settingsWebSocketService;
            WebSocketHandler = webSocketHandler;
        }


        public void OnWebSocketEvent(WebSocketEventArgs args)
        {
            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugSource = this.GetType().Name,
                DebugMessage = "Method OnWebSocketEvent called.",
                DebugSourceType = 1,
                DebugType = 0
            });

            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugSource = this.GetType().Name,
                DebugMessage = args.ToString(),
                DebugSourceType = 1,
                DebugType = 1
            });


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
                    OnDebugEvent?.Invoke(this, new BaseDebugArgs()
                    {
                        DebugSource = this.GetType().Name,
                        DebugMessage = e.ToString(),
                        DebugSourceType = 1,
                        DebugType = 4
                    });

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
                OnDebugEvent?.Invoke(this, new BaseDebugArgs()
                {
                    DebugSource = this.GetType().Name,
                    DebugMessage = e.ToString(),
                    DebugSourceType = 1,
                    DebugType = 4
                });

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
