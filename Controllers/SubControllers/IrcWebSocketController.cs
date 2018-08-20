using LittleWeebLibrary.GlobalInterfaces;
using LittleWeebLibrary.EventArguments;
using Newtonsoft.Json.Linq;
using System;
using LittleWeebLibrary.Services;
using LittleWeebLibrary.Handlers;
using LittleWeebLibrary.Models;
using Newtonsoft.Json;

namespace LittleWeebLibrary.Controllers
{
    public class IrcWebSocketController : ISubWebSocketController, IDebugEvent
    {

        public event EventHandler<BaseDebugArgs> OnDebugEvent;

        private readonly IIrcWebSocketService IrcWebSocketService;
        private readonly IWebSocketHandler WebSocketHandler;

        public IrcWebSocketController(IWebSocketHandler webSocketHandler, IIrcWebSocketService ircWebSocketService)
        {
            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugSource = this.GetType().Name,
                DebugMessage = "IrcWebSocketController called.",
                DebugSourceType = 0,
                DebugType = 0
            });

            IrcWebSocketService = ircWebSocketService;
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
                                case "connect_irc":
                                    IrcWebSocketService.Connect(extra);
                                    break;
                                case "sendmessage_irc":
                                    IrcWebSocketService.SendMessage(extra);
                                    break;
                            }
                        }
                        else
                        {
                            switch (action)
                            {
                                case "get_irc_data":
                                    IrcWebSocketService.GetCurrentIrcSettings();
                                    break;
                                case "connect_irc":
                                    IrcWebSocketService.Connect();
                                    break;
                                case "disconnect_irc":
                                    IrcWebSocketService.Disconnect();
                                    break;
                                case "enablechat_irc":
                                    IrcWebSocketService.EnableSendMessage();
                                    break;
                                case "disablechat_irc":
                                    IrcWebSocketService.DisableSendMessage();
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


