using LittleWeebLibrary.Controllers;
using LittleWeebLibrary.Controllers.SubControllers;
using LittleWeebLibrary.GlobalInterfaces;
using LittleWeebLibrary.Handlers;
using LittleWeebLibrary.Services;
using System.Collections.Generic;
using System.Diagnostics;


namespace LittleWeebLibrary
{
    public class StartUp
    {

        //handlers
        private readonly ISettingsHandler SettingsHandler;
        private readonly IWebSocketHandler WebSocketHandler;
        private readonly IFileHistoryHandler FileHistoryHandler;
        private readonly IFileHandler FileHandler;
        private readonly IDirectoryHandler DirectoryHandler;
        private readonly IIrcClientHandler IrcClientHandler;
        private readonly IDownloadHandler DownloadHandler;
        private readonly IDebugHandler DebugHandler;
        private readonly IVersionHandler VersionHandler;

        //services
        private readonly IDirectoryWebSocketService DirectoryWebSocketService;
        private readonly IDownloadWebSocketService DownloadWebSocketService;
        private readonly IFileWebSocketService FileWebSocketService;
        private readonly IIrcWebSocketService IrcWebSocketService;
        private readonly ISettingsWebSocketService SettingsWebSocketService;
        private readonly IVersionWebSocketService VersionWebSocketService;

        //controllers
        private readonly ISubWebSocketController DirectoryWebSocketController;
        private readonly ISubWebSocketController DownloadWebSocketController;
        private readonly ISubWebSocketController FileWebSocketController;
        private readonly ISubWebSocketController IrcWebSocketController;
        private readonly ISubWebSocketController SettingsWebSocketController;
        private readonly ISubWebSocketController VersionWebSocketController;

        public StartUp()
        {
            //handlers

            DirectoryHandler =      new DirectoryHandler();
            SettingsHandler =       new SettingsHandler();
            FileHistoryHandler =    new FileHistoryHandler();
            FileHandler =           new FileHandler();
            VersionHandler =        new VersionHandler();
            WebSocketHandler =      new WebSocketHandler(SettingsHandler);
            IrcClientHandler =      new IrcClientHandler(SettingsHandler);
            DownloadHandler =       new DownloadHandler(IrcClientHandler);
            DebugHandler =          new DebugHandler(SettingsHandler);

            //Services
            DirectoryWebSocketService = new DirectoryWebSocketService(WebSocketHandler, DirectoryHandler);
            DownloadWebSocketService =  new DownloadWebSocketService(WebSocketHandler, DirectoryHandler, DownloadHandler, FileHandler, FileHistoryHandler, SettingsHandler);
            FileWebSocketService =      new FileWebSocketService(WebSocketHandler, FileHandler, FileHistoryHandler, DownloadHandler);
            IrcWebSocketService =       new IrcWebSocketService(WebSocketHandler, IrcClientHandler, SettingsHandler);
            SettingsWebSocketService =  new SettingsWebSocketService(WebSocketHandler, DirectoryHandler);
            VersionWebSocketService =   new VersionWebSocketService(WebSocketHandler, VersionHandler);


            //Controllers
            DirectoryWebSocketController =  new DirectoryWebSocketController(WebSocketHandler, DirectoryWebSocketService);
            DownloadWebSocketController =   new DownloadWebSocketController(WebSocketHandler, DownloadWebSocketService, DirectoryWebSocketService);
            FileWebSocketController =       new FileWebSocketController(WebSocketHandler, FileWebSocketService);
            IrcWebSocketController =        new IrcWebSocketController(WebSocketHandler, IrcWebSocketService);
            SettingsWebSocketController =   new SettingsWebSocketController(WebSocketHandler, SettingsWebSocketService);
            VersionWebSocketController =    new VersionWebSocketController(WebSocketHandler, VersionWebSocketService);

            IBaseWebSocketController baseWebSocketController = new BaseWebSocketController(WebSocketHandler);
            //start debugh handler registering all the handlers, services and controllers as IDebugEvent interface.

            SettingsWebSocketService.SetSettingsClasses(
                WebSocketHandler,
                SettingsHandler,
                IrcClientHandler,
                DebugHandler,
                FileHandler,
                DownloadHandler,
                DirectoryWebSocketService,
                IrcWebSocketService
            );

            baseWebSocketController.SetSubControllers(new List<ISubWebSocketController>()
            {
                DirectoryWebSocketController,
                DownloadWebSocketController,
                FileWebSocketController,
                IrcWebSocketController,
                SettingsWebSocketController,
                VersionWebSocketController
            });

            DebugHandler.SetDebugEvents(new List<IDebugEvent>()
            {
                SettingsHandler as IDebugEvent,
                WebSocketHandler as IDebugEvent,
                FileHistoryHandler as IDebugEvent,
                FileHandler as IDebugEvent,
                DirectoryHandler as IDebugEvent,
                IrcClientHandler as IDebugEvent,
                DownloadHandler as IDebugEvent,
                VersionHandler as IDebugEvent,
                DirectoryWebSocketService as IDebugEvent,
                DownloadWebSocketService as IDebugEvent,
                FileWebSocketService as IDebugEvent,
                IrcWebSocketService as IDebugEvent,
                SettingsWebSocketService as IDebugEvent,
                DirectoryWebSocketController as IDebugEvent,
                DownloadWebSocketController as IDebugEvent,
                FileWebSocketController as IDebugEvent,
                IrcWebSocketController as IDebugEvent,
                SettingsWebSocketController as IDebugEvent,
                VersionWebSocketService as IDebugEvent,
                baseWebSocketController as IDebugEvent,

            });
            

        }

        public void Start()
        {
            WebSocketHandler.StartServer();
        }

        public void Stop()
        {
            IrcClientHandler.StopDownload();
            IrcClientHandler.StopConnection();
            DownloadHandler.StopQueue();
            WebSocketHandler.StopServer();
        }
    }
}
