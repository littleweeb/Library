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
        private readonly IVersionHandler VersionHandler;
        private readonly IKitsuHandler KitsuHandler;
        private readonly INiblHandler NiblHandler;
        private readonly IAnimeProfileHandler AnimeProfileHandler;
        private readonly IDebugHandler DebugHandler;
        private readonly IDataBaseHandler DataBaseHandler;
        private readonly IAnimeRuleHandler AnimeRuleHandler;

        //services
        private readonly IDirectoryWebSocketService DirectoryWebSocketService;
        private readonly IDownloadWebSocketService DownloadWebSocketService;
        private readonly IFileWebSocketService FileWebSocketService;
        private readonly IIrcWebSocketService IrcWebSocketService;
        private readonly ISettingsWebSocketService SettingsWebSocketService;
        private readonly IInfoApiWebSocketService InfoApiWebSocketService;
        private readonly IVersionWebSocketService VersionWebSocketService;
        private readonly IDataBaseWebSocketService DataBaseWebSocketService;

        //controllers
        private readonly ISubWebSocketController DirectoryWebSocketController;
        private readonly ISubWebSocketController DownloadWebSocketController;
        private readonly ISubWebSocketController FileWebSocketController;
        private readonly ISubWebSocketController IrcWebSocketController;
        private readonly ISubWebSocketController SettingsWebSocketController;
        private readonly ISubWebSocketController InfoApiWebSocketController;
        private readonly ISubWebSocketController VersionWebSocketController;
        private readonly ISubWebSocketController DataBaseWebSocketController;

        public StartUp()
        {
            //handlers

            DebugHandler =          new DebugHandler();
            SettingsHandler =       new SettingsHandler(DebugHandler);

            DebugHandler.SetSettings(SettingsHandler);



            DirectoryHandler =      new DirectoryHandler(DebugHandler);
            DataBaseHandler =       new DataBaseHandler(DebugHandler);
            FileHistoryHandler =    new FileHistoryHandler(DataBaseHandler, DebugHandler);
            FileHandler =           new FileHandler(DebugHandler);
            VersionHandler =        new VersionHandler(DebugHandler);
            WebSocketHandler =      new WebSocketHandler(SettingsHandler, DebugHandler);
            IrcClientHandler =      new IrcClientHandler(SettingsHandler, DebugHandler);
            DownloadHandler =       new DownloadHandler(IrcClientHandler, FileHistoryHandler, FileHandler, DebugHandler);
            KitsuHandler =          new KitsuHandler(DebugHandler);
            NiblHandler =           new NiblHandler(KitsuHandler, DebugHandler);
            AnimeRuleHandler =      new AnimeRuleHandler(DataBaseHandler, DebugHandler);
            AnimeProfileHandler =   new AnimeProfileHandler(KitsuHandler, NiblHandler, DataBaseHandler, AnimeRuleHandler, DebugHandler);

            //Services
            DirectoryWebSocketService = new DirectoryWebSocketService(WebSocketHandler, DirectoryHandler, DebugHandler);
            DownloadWebSocketService =  new DownloadWebSocketService(WebSocketHandler, DirectoryHandler, DownloadHandler, FileHandler, FileHistoryHandler, SettingsHandler, DebugHandler);
            FileWebSocketService =      new FileWebSocketService(WebSocketHandler, FileHandler, FileHistoryHandler, DownloadHandler, DebugHandler);
            IrcWebSocketService =       new IrcWebSocketService(WebSocketHandler, IrcClientHandler, SettingsHandler, DebugHandler);
            SettingsWebSocketService =  new SettingsWebSocketService(WebSocketHandler, DirectoryHandler, DebugHandler);
            InfoApiWebSocketService =   new InfoApiWebSocketService(WebSocketHandler, AnimeProfileHandler, NiblHandler, KitsuHandler, AnimeRuleHandler, DebugHandler);
            VersionWebSocketService =   new VersionWebSocketService(WebSocketHandler, VersionHandler, DebugHandler);
            DataBaseWebSocketService =  new DataBaseWebSocketService(WebSocketHandler, DataBaseHandler, DebugHandler);


            //Sub Controllers
            DirectoryWebSocketController =  new DirectoryWebSocketController(WebSocketHandler, DirectoryWebSocketService, DebugHandler);
            DownloadWebSocketController =   new DownloadWebSocketController(WebSocketHandler, DownloadWebSocketService, DirectoryWebSocketService, DebugHandler);
            FileWebSocketController =       new FileWebSocketController(WebSocketHandler, FileWebSocketService, DebugHandler);
            IrcWebSocketController =        new IrcWebSocketController(WebSocketHandler, IrcWebSocketService, DebugHandler);
            SettingsWebSocketController =   new SettingsWebSocketController(WebSocketHandler, SettingsWebSocketService, DebugHandler);
            InfoApiWebSocketController =    new InfoApiWebSocketController(WebSocketHandler, InfoApiWebSocketService, DebugHandler);
            VersionWebSocketController =    new VersionWebSocketController(WebSocketHandler, VersionWebSocketService, DebugHandler);
            DataBaseWebSocketController =   new DataBaseWebSocketController(WebSocketHandler, DataBaseWebSocketService, DebugHandler);

            IBaseWebSocketController baseWebSocketController = new BaseWebSocketController(WebSocketHandler, DebugHandler);
            //start debugh handler registering all the handlers, services and controllers as IDebugEvent interface.

            SettingsWebSocketService.SetSettingsClasses(
                SettingsHandler,
                IrcClientHandler,
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
                InfoApiWebSocketController,
                VersionWebSocketController,
                DataBaseWebSocketController
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
