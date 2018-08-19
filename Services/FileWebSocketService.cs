using LittleWeebLibrary.EventArguments;
using LittleWeebLibrary.GlobalInterfaces;
using LittleWeebLibrary.Handlers;
using LittleWeebLibrary.Models;
using LittleWeebLibrary.Settings;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace LittleWeebLibrary.Services
{
    public interface IFileWebSocketService
    {
        void DeleteFile(JObject fileInfoJson);
        void OpenFile(JObject fileInfoJson);
    }
    public class FileWebSocketService : IFileWebSocketService, IDebugEvent
    {
        public event EventHandler<BaseDebugArgs> OnDebugEvent;


        private readonly IWebSocketHandler WebSocketHandler;
        private readonly IFileHandler FileHandler;
        private readonly IFileHistoryHandler FileHistoryHandler;
        private readonly IDownloadHandler DownloadHandler;

        public FileWebSocketService(IWebSocketHandler webSocketHandler, IFileHandler fileHandler, IFileHistoryHandler fileHistoryHandler, IDownloadHandler downloadHandler)
        {
            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugMessage = "Constructor called.",
                DebugSource = this.GetType().Name,
                DebugSourceType = 0,
                DebugType = 0
            });

            WebSocketHandler = webSocketHandler;
            FileHandler = fileHandler;
            FileHistoryHandler = fileHistoryHandler;
            DownloadHandler = downloadHandler;
        }


        public void DeleteFile(JObject fileInfoJson)
        {
            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugMessage = "DeleteFile called.",
                DebugSource = this.GetType().Name,
                DebugSourceType = 1,
                DebugType = 0
            });
            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugMessage = fileInfoJson.ToString(),
                DebugSource = this.GetType().Name,
                DebugSourceType = 1,
                DebugType = 1
            });

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
                WebSocketHandler.SendMessage(result);
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
                    type = "parse_file_to_delete_error",
                    errormessage = "Could not parse json containing file to delete information.",
                    errortype = "exception",
                    exception = e.ToString()
                };
            }
        }

        public async void OpenFile(JObject fileInfoJson)
        {
            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugMessage = "OpenFile called.",
                DebugSource = this.GetType().Name,
                DebugSourceType = 1,
                DebugType = 0
            });

            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugMessage = fileInfoJson.ToString(),
                DebugSource = this.GetType().Name,
                DebugSourceType = 1,
                DebugType = 1
            });

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
                    string result = await FileHandler.OpenFile(filePath);
                    await WebSocketHandler.SendMessage(result);
                }
                else
                {

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
                OnDebugEvent?.Invoke(this, new BaseDebugArgs()
                {
                    DebugSource = this.GetType().Name,
                    DebugMessage = e.ToString(),
                    DebugSourceType = 1,
                    DebugType = 4
                });

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
