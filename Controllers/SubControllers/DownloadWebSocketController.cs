using System;
using LittleWeebLibrary.GlobalInterfaces;
using LittleWeebLibrary.EventArguments;
using LittleWeebLibrary.Services;
using Newtonsoft.Json.Linq;
using LittleWeebLibrary.Handlers;
using LittleWeebLibrary.Models;
using Newtonsoft.Json;

namespace LittleWeebLibrary.Controllers
{
    public class DownloadWebSocketController : ISubWebSocketController
    {

       

        private readonly IDownloadWebSocketService DownloadWebSocketService;
        private readonly IWebSocketHandler WebSocketHandler;
        private readonly IDirectoryWebSocketService DirectoryWebSocketService;
        private readonly IDebugHandler DebugHandler;

        public DownloadWebSocketController(IWebSocketHandler webSocketHandler, IDownloadWebSocketService downloadWebSocketService, IDirectoryWebSocketService directoryWebSocketService, IDebugHandler debugHandler)
        {
            debugHandler.TraceMessage("Constructor Called.", DebugSource.CONSTRUCTOR, DebugType.ENTRY_EXIT);
            DebugHandler = debugHandler;
            DownloadWebSocketService = downloadWebSocketService;
            WebSocketHandler = webSocketHandler;
            DirectoryWebSocketService = directoryWebSocketService;
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
                        JObject extra = query.Value<JObject>("extra");

                        if (extra != null)
                        {
                            switch (action)
                            {
                                case "add_download":
                                    DownloadWebSocketService.AddDownload(extra);
                                    break;
                                case "add_downloads":
                                    DownloadWebSocketService.AddDownloads(extra);
                                    break;
                                case "abort_download":
                                    DownloadWebSocketService.AbortDownload(extra);
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
