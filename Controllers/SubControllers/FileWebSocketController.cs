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

namespace LittleWeebLibrary.Controllers
{
    public class FileWebSocketController : ISubWebSocketController, IDebugEvent
    {

        public event EventHandler<BaseDebugArgs> OnDebugEvent;

        private readonly IFileWebSocketService FileWebSocketService;
        private readonly IWebSocketHandler WebSocketHandler;

        public FileWebSocketController(IWebSocketHandler webSocketHandler, IFileWebSocketService fileWebSocketService)
        {
            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugSource = this.GetType().Name,
                DebugMessage = "FileWebSocketController called.",
                DebugSourceType = 0,
                DebugType = 0
            });

            FileWebSocketService = fileWebSocketService;
            WebSocketHandler = webSocketHandler;
        }

      

        public void OnWebSocketEvent(WebSocketEventArgs args)
        {
            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugSource = this.GetType().Name,
                DebugMessage = "OnWebSocketEvent called.",
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
                                case "delete_file":
                                    FileWebSocketService.DeleteFile(extra);
                                    break;
                                case "open_file":
                                    FileWebSocketService.OpenFile(extra);
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
