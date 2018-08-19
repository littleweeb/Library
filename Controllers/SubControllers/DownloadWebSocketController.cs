using System;
using System.Collections.Generic;
using System.Text;
using LittleWeebLibrary.GlobalInterfaces;
using LittleWeebLibrary.EventArguments;
using LittleWeebLibrary.Settings;
using LittleWeebLibrary.Services;
using Newtonsoft.Json.Linq;
using LittleWeebLibrary.Handlers;
using LittleWeebLibrary.Models;
using Newtonsoft.Json;

namespace LittleWeebLibrary.Controllers
{
    public class DownloadWebSocketController : ISubWebSocketController, IDebugEvent
    {

        public event EventHandler<BaseDebugArgs> OnDebugEvent;

        private readonly IDownloadWebSocketService DownloadWebSocketService;
        private readonly IWebSocketHandler WebSocketHandler;
        private readonly IDirectoryWebSocketService DirectoryWebSocketService;

        public DownloadWebSocketController(IWebSocketHandler webSocketHandler, IDownloadWebSocketService downloadWebSocketService, IDirectoryWebSocketService directoryWebSocketService)
        {
            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugSource = this.GetType().Name,
                DebugMessage = "DownloadWebSocketController called.",
                DebugSourceType = 0,
                DebugType = 0
            });

            DownloadWebSocketService = downloadWebSocketService;
            WebSocketHandler = webSocketHandler;
            DirectoryWebSocketService = directoryWebSocketService;
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
                                case "add_download":
                                    DownloadWebSocketService.AddDownload(extra);
                                    break;
                                case "abort_download":
                                    DownloadWebSocketService.RemoveDownload(extra);
                                    break;
                                case "remove_download":
                                    DownloadWebSocketService.RemoveDownload(extra);
                                    break;
                            }
                        }
                        else
                        {
                            switch (action)
                            {
                                case "get_downloads":
                                    DownloadWebSocketService.GetCurrentFileHistory();
                                    break;
                                case "get_free_space":
                                    DirectoryWebSocketService.GetFreeSpace();
                                    break;
                                case "open_download_directory":
                                    DownloadWebSocketService.Openfullfilepath();
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
