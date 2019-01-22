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
    public class IrcWebSocketController : ISubWebSocketController
    {

       

        private readonly IIrcWebSocketService IrcWebSocketService;
        private readonly IWebSocketHandler WebSocketHandler;
        private readonly IDebugHandler DebugHandler;

        public IrcWebSocketController(IWebSocketHandler webSocketHandler, IIrcWebSocketService ircWebSocketService, IDebugHandler debugHandler)
        {
            debugHandler.TraceMessage("Constructor Called.", DebugSource.CONSTRUCTOR, DebugType.ENTRY_EXIT);
            DebugHandler = debugHandler;
            IrcWebSocketService = ircWebSocketService;
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


