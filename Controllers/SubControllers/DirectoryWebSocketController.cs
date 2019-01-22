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
    public class DirectoryWebSocketController : ISubWebSocketController
    {

       

        private readonly IWebSocketHandler WebSocketHandler;
        private readonly IDirectoryWebSocketService DirectoryWebSocketService;
        private readonly IDebugHandler DebugHandler;

        public DirectoryWebSocketController(IWebSocketHandler webSocketHandler, IDirectoryWebSocketService directoryWebSocketService, IDebugHandler debugHandler)
        {
            debugHandler.TraceMessage("Contructor Called.", DebugSource.CONSTRUCTOR, DebugType.ENTRY_EXIT);
            DebugHandler = debugHandler;
            DirectoryWebSocketService = directoryWebSocketService;
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
                        JObject extra = query.Value<JObject>("extra");

                        if (extra != null)
                        {
                            switch (action)
                            {
                                case "create_directory":
                                    DirectoryWebSocketService.CreateDirectory(extra);
                                    break;
                                case "delete_directory":
                                    DirectoryWebSocketService.DeleteDirectory(extra);
                                    break;
                                case "get_directories":
                                    DirectoryWebSocketService.GetDirectories(extra);
                                    break;
                            }
                        }
                        else
                        {
                            switch (action)
                            {
                                case "get_directories":
                                    DirectoryWebSocketService.GetDrives();
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
