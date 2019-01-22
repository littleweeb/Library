using LittleWeebLibrary.EventArguments;
using LittleWeebLibrary.GlobalInterfaces;
using LittleWeebLibrary.Handlers;
using LittleWeebLibrary.Models;
using LittleWeebLibrary.Settings;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace LittleWeebLibrary.Services
{
    public interface IDirectoryWebSocketService
    {
        Task CreateDirectory(JObject directoryJson);
        Task DeleteDirectory(JObject directoryJson);
        Task GetDirectories(JObject directoryJson);
        Task GetFreeSpace();
        Task GetDrives();
    }
    public class DirectoryWebSocketService : IDirectoryWebSocketService, ISettingsInterface
    {
       

        private readonly IDirectoryHandler DirectoryHandler;
        private readonly IWebSocketHandler WebSocketHandler;
        private readonly IDebugHandler DebugHandler;

        private LittleWeebSettings LittleWeebSettings;
        private IrcSettings IrcSettings;

        public DirectoryWebSocketService(IWebSocketHandler webSocketHandler, IDirectoryHandler directoryHandler, IDebugHandler debugHandler)
        {
            
            debugHandler.TraceMessage("Constructor Called.", DebugSource.CONSTRUCTOR, DebugType.ENTRY_EXIT);
            DebugHandler = debugHandler;
            WebSocketHandler = webSocketHandler;
            DirectoryHandler = directoryHandler;
        }

        public async Task CreateDirectory(JObject directoryJson)
        {

            DebugHandler.TraceMessage("CreateDirectory called.", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage(directoryJson.ToString(), DebugSource.TASK, DebugType.PARAMETERS);
            try
            {

                string filePath = directoryJson.Value<string>("path");
                if (filePath.Contains("//"))
                {
                    filePath.Replace("//", "/");
                }

                if (filePath.Contains("\\\\"))
                {
                    filePath.Replace("\\\\", "\\");
                }
                string result = DirectoryHandler.CreateDirectory(filePath, "");
                await WebSocketHandler.SendMessage(result);
            }
            catch (Exception e)
            {
                DebugHandler.TraceMessage(e.ToString(), DebugSource.TASK, DebugType.WARNING);

                JsonError error = new JsonError()
                {
                    type = "create_directory_error",
                    errormessage = "Could not create directory.",
                    errortype = "exception",
                    exception = e.ToString()
                };

                await WebSocketHandler.SendMessage(error.ToJson());
            }
        }

        public async Task DeleteDirectory(JObject directoryJson)
        {
            DebugHandler.TraceMessage("DeleteDirectory called.", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage(directoryJson.ToString(), DebugSource.TASK, DebugType.PARAMETERS);
            try
            {
                string filePath = directoryJson.Value<string>("path");
                if (filePath.Contains("//"))
                {
                    filePath.Replace("//", "/");
                }

                if (filePath.Contains("\\\\"))
                {
                    filePath.Replace("\\\\", "\\");
                }
                string result = DirectoryHandler.DeleteDirectory(filePath);
                await WebSocketHandler.SendMessage(result);
            } 
            catch (Exception e)
            {
                DebugHandler.TraceMessage(e.ToString(), DebugSource.TASK, DebugType.WARNING);
              
                JsonError error = new JsonError()
                {
                    type = "create_directory_error",
                    errormessage = "Could not create directory.",
                    errortype = "exception",
                    exception = e.ToString()
                };

                await WebSocketHandler.SendMessage(error.ToJson());
            }
        }

        public async Task GetDrives()
        {
            DebugHandler.TraceMessage("GetDrives called.", DebugSource.TASK, DebugType.ENTRY_EXIT);
            string result = DirectoryHandler.GetDrives();
            await WebSocketHandler.SendMessage(result);
        }

        public async Task GetDirectories(JObject directoryJson)
        {
            DebugHandler.TraceMessage("GetDirectories called.", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage(directoryJson.ToString(), DebugSource.TASK, DebugType.PARAMETERS);

            try
            {

                string filePath = directoryJson.Value<string>("path");
                if (filePath.Contains("//"))
                {
                    filePath.Replace("//", "/");
                }

                if (filePath.Contains("\\\\"))
                {
                    filePath.Replace("\\\\", "\\");
                }
                string result = DirectoryHandler.GetDirectories(filePath);
                await WebSocketHandler.SendMessage(result);
            }
            catch (Exception e)
            {
                DebugHandler.TraceMessage(e.ToString(), DebugSource.TASK, DebugType.WARNING);

                JsonError error = new JsonError()
                {
                    type = "create_directory_error",
                    errormessage = "Could not create directory.",
                    errortype = "exception",
                    exception = e.ToString(),
                };

                await WebSocketHandler.SendMessage(error.ToJson());
            }
        }

        public async Task GetFreeSpace()
        {
            DebugHandler.TraceMessage("GetFreeSpace called.", DebugSource.TASK, DebugType.ENTRY_EXIT);
            string result = DirectoryHandler.GetFreeSpace(IrcSettings.fullfilepath);
            await WebSocketHandler.SendMessage(result);
        }

        public void SetIrcSettings(IrcSettings settings)
        {
            DebugHandler.TraceMessage("SetIrcSettings called.", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage(settings.ToString(), DebugSource.TASK, DebugType.PARAMETERS);
            IrcSettings = settings;
        }

        public void SetLittleWeebSettings(LittleWeebSettings settings)
        {
            DebugHandler.TraceMessage("SetIrcSettings called.", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage(settings.ToString(), DebugSource.TASK, DebugType.PARAMETERS);
            LittleWeebSettings = settings;
        }
    }
}
