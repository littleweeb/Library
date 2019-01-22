using LittleWeebLibrary.EventArguments;
using LittleWeebLibrary.GlobalInterfaces;
using LittleWeebLibrary.Models;
using LittleWeebLibrary.Settings;
using LittleWeebLibrary.StaticClasses;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

#if __ANDROID__
using Android.Content;
#endif

namespace LittleWeebLibrary.Handlers
{
    public interface IFileHandler
    {
        string OpenFile(string filePath, string fileName = null);
        string DeleteFile(string filePath, string fileName = null);
    }
    public class FileHandler : IFileHandler, ISettingsInterface
    {
       

        private readonly IDebugHandler DebugHandler;
        private LittleWeebSettings LittleWeebSettings;

        public FileHandler(IDebugHandler debugHandler)
        {
            debugHandler.TraceMessage("Constructor called", DebugSource.CONSTRUCTOR, DebugType.ENTRY_EXIT);
            DebugHandler = debugHandler;
        }

        public string OpenFile(string filePath, string fileName = null)
        {

            DebugHandler.TraceMessage("OpenFile called", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage("FilePath: " + filePath, DebugSource.TASK, DebugType.PARAMETERS);

            string fullFilePath = filePath;

            if (fileName != null)
            {



                DebugHandler.TraceMessage("FileName: " + fileName, DebugSource.TASK, DebugType.PARAMETERS);

                fullFilePath = Path.Combine(filePath, fileName);

                DebugHandler.TraceMessage("Full Filepath: " + fullFilePath, DebugSource.TASK, DebugType.INFO);
            }

            try
            {                
                for (int i = 0; i < 20; i++)
                {
                    if (File.Exists(fullFilePath))
                    {
#if __ANDROID__
                        Android.Net.Uri uri = Android.Net.Uri.Parse(fullFilePath);
                        Intent intent = new Intent(Intent.ActionView);
                        intent.SetDataAndType(uri, "video/*");
                        intent.SetFlags(ActivityFlags.ClearWhenTaskReset | ActivityFlags.NewTask);
                        Android.App.Application.Context.StartActivity(intent);
#else

                        if (UtilityMethods.CheckOperatingSystems() == UtilityMethods.OperatingSystems.OsX)
                        {
                            Process.Start("open", fullFilePath);
                        }
                        else
                        {
                            var p = new Process
                            {
                                StartInfo = new ProcessStartInfo(fullFilePath)
                                {
                                    UseShellExecute = true
                                }
                            };
                            p.Start();
                        }                       
#endif
                        JsonSuccess report = new JsonSuccess()
                        {
                            message = "Succesfully opened file with path: " + fullFilePath
                        };
                        return report.ToJson();
                    }
                    Thread.Sleep(200);
                }

                JsonError jsonError = new JsonError
                {
                    type = "open_file_failed",
                    errormessage = "Could not open file but didn't throw exception.",
                    errortype = "warning",
                    exception = "none"
                };

                DebugHandler.TraceMessage("Could not open file but didn't throw exception, file: " + fullFilePath, DebugSource.TASK, DebugType.WARNING);

                return jsonError.ToJson();

            }
            catch (Exception e)
            {
                DebugHandler.TraceMessage("Could not open file. " + fullFilePath, DebugSource.TASK, DebugType.WARNING);
                DebugHandler.TraceMessage(e.ToString(), DebugSource.TASK, DebugType.WARNING);


                JsonError jsonError = new JsonError
                {
                    type = "open_file_failed",
                    errormessage = "Could not open file.",
                    errortype = "exception",
                    exception = e.ToString()
                };
                return jsonError.ToJson();
            }
        }
       

        public string DeleteFile(string filePath, string fileName = null)
        {

            DebugHandler.TraceMessage("DeleteFile called", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage("FilePath: " + filePath, DebugSource.TASK, DebugType.PARAMETERS);

            string fullFilePath = filePath;

            if (fileName != null)
            {


                DebugHandler.TraceMessage("FileName: " + fileName, DebugSource.TASK, DebugType.PARAMETERS);
                fullFilePath = Path.Combine(filePath, fileName);

                DebugHandler.TraceMessage("Full File Path: " + fullFilePath, DebugSource.TASK, DebugType.INFO);
            }

            try
            {
                if (File.Exists(fullFilePath))
                {
                    File.Delete(fullFilePath);

                    string[] filePaths = Directory.GetFiles(Path.GetDirectoryName(filePath));
                    if (filePaths.Length == 0)
                    {
                        Directory.Delete(filePath);

                        DebugHandler.TraceMessage("Succesfully deleted file: " + fullFilePath + " & empty directory: " + filePath, DebugSource.TASK, DebugType.INFO);

                        JsonSuccess report = new JsonSuccess()
                        {
                            message = "Succesfully deleted file with path: " + fullFilePath + " and directory: " + filePath
                        };

                        return report.ToJson();
                    }
                    else
                    {
                        DebugHandler.TraceMessage("Succesfully deleted file: " + fullFilePath , DebugSource.TASK, DebugType.INFO);

                        JsonSuccess report = new JsonSuccess()
                        {
                            message = "Succesfully deleted file with path: " + fullFilePath
                        };

                        return report.ToJson();
                    }
                }
                else
                {
                    DebugHandler.TraceMessage("File: " + fullFilePath + " does not exist, possibly already deleted.", DebugSource.TASK, DebugType.INFO);
                    JsonSuccess report = new JsonSuccess()
                    {
                        message = "File with filepath: " + fullFilePath + " already removed."
                    };

                    return report.ToJson();
                }

            }
            catch (Exception e)
            {


                DebugHandler.TraceMessage("Failed to delete file: " + fullFilePath, DebugSource.TASK, DebugType.WARNING);
                DebugHandler.TraceMessage(e.ToString(), DebugSource.TASK, DebugType.WARNING);

                JsonError jsonError = new JsonError
                {
                    type = "delete_file_failed",
                    errormessage = "Could not delete file.",
                    errortype = "exception",
                    exception = e.ToString()
                };

                return jsonError.ToJson();
            }
        }

        public void SetIrcSettings(IrcSettings settings)
        {
        }

        public void SetLittleWeebSettings(LittleWeebSettings settings)
        {
            DebugHandler.TraceMessage("SetLittleWeebSettings called", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage(settings.ToString(), DebugSource.TASK, DebugType.PARAMETERS);
            LittleWeebSettings = settings;
        }
    }
}
