using LittleWeebLibrary.EventArguments;
using LittleWeebLibrary.GlobalInterfaces;
using LittleWeebLibrary.Handlers;
using LittleWeebLibrary.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace LittleWeebLibrary.Services
{
    public interface IFileWebSocketService
    {
        Task DeleteFile(JObject fileInfoJson);
        Task OpenFile(JObject fileInfoJson);
    }

    public class FileWebSocketService : IFileWebSocketService
    {
       

        private readonly IWebSocketHandler WebSocketHandler;
        private readonly IFileHandler FileHandler;
        private readonly IFileHistoryHandler FileHistoryHandler;
        private readonly IDownloadHandler DownloadHandler;
        private readonly IDebugHandler DebugHandler;

        public FileWebSocketService(IWebSocketHandler webSocketHandler, IFileHandler fileHandler, IFileHistoryHandler fileHistoryHandler, IDownloadHandler downloadHandler, IDebugHandler debugHandler)
        {
           
            debugHandler.TraceMessage("Constructor called.", DebugSource.CONSTRUCTOR, DebugType.ENTRY_EXIT);

            WebSocketHandler = webSocketHandler;
            FileHandler = fileHandler;
            FileHistoryHandler = fileHistoryHandler;
            DownloadHandler = downloadHandler;
            DebugHandler = debugHandler;
        }


        public async Task DeleteFile(JObject fileInfoJson)
        {
            DebugHandler.TraceMessage("DeleteFile called.", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage(fileInfoJson.ToString(), DebugSource.TASK, DebugType.PARAMETERS);

            try
            {
                string filePath = fileInfoJson.Value<string>("path");

                if (filePath.Contains("//"))
                {
                    filePath.Replace("//", "/");
                }

                if (filePath.Contains("\\\\"))
                {
                    filePath.Replace("\\\\", "\\");
                }

                DownloadHandler.RemoveDownload(filePath);
                FileHistoryHandler.RemoveFileFromFileHistory(filePath);
                string result = FileHandler.DeleteFile(filePath);
                await WebSocketHandler.SendMessage(result);
            }
            catch (Exception e)
            {

                DebugHandler.TraceMessage(e.ToString(), DebugSource.TASK, DebugType.WARNING);
                JsonError error = new JsonError()
                {
                    type = "parse_file_to_delete_error",
                    errormessage = "Could not parse json containing file to delete information.",
                    errortype = "exception",
                    exception = e.ToString()
                };
                await WebSocketHandler.SendMessage(error.ToJson());
            }
        }

        public async Task OpenFile(JObject fileInfoJson)
        {
            DebugHandler.TraceMessage("OpenFile called.", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage(fileInfoJson.ToString(), DebugSource.TASK, DebugType.PARAMETERS);            

            try
            {
                string filePath = fileInfoJson.Value<string>("path");

                if (filePath.Contains("//"))
                {
                    filePath.Replace("//", "/");
                }

                if (filePath.Contains("\\\\"))
                {
                    filePath.Replace("\\\\", "\\");
                }

                if (filePath != null)
                {
                    string result = FileHandler.OpenFile(filePath);
                    await WebSocketHandler.SendMessage(result);
                }
                else
                {

                    DebugHandler.TraceMessage("Request does not contain path parameter to open file.", DebugSource.TASK, DebugType.WARNING);
                    JsonError error = new JsonError()
                    {
                        type = "parse_file_to_open_error",
                        errormessage = "Request does not contain path parameter to open file.",
                        errortype = "warning",
                        exception = "none"
                    };

                    await WebSocketHandler.SendMessage(error.ToJson());
                }
            }
            catch (Exception e)
            {
                DebugHandler.TraceMessage(e.ToString(), DebugSource.TASK, DebugType.WARNING);
                JsonError error = new JsonError()
                {
                    type = "parse_file_to_open_error",
                    errormessage = "Could not parse json containing file to open information.",
                    errortype = "exception",
                    exception = e.ToString()
                };

                await WebSocketHandler.SendMessage(error.ToJson());
            }        
        }
    }
}
