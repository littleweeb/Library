using LittleWeebLibrary.EventArguments;
using LittleWeebLibrary.GlobalInterfaces;
using LittleWeebLibrary.Models;
using LittleWeebLibrary.StaticClasses;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace LittleWeebLibrary.Handlers
{
    public interface IDirectoryHandler
    {
        string GetDrives();
        string GetDirectories(string path);
        string DeleteDirectory(string path);
        string CreateDirectory(string path, string name);
        string OpenDirectory(string directoryPath);
        string GetFreeSpace(string directoryPath);

    }
    public class DirectoryHandler :  IDirectoryHandler
    {
       

        private readonly IDebugHandler DebugHandler;

        public DirectoryHandler(IDebugHandler debugHandler)
        {
            debugHandler.TraceMessage("Constructor Called", DebugSource.CONSTRUCTOR, DebugType.ENTRY_EXIT);
            DebugHandler = debugHandler;
        }

        public string GetDrives()
        {

            DebugHandler.TraceMessage("GetDrives Called", DebugSource.TASK, DebugType.ENTRY_EXIT);
            try
            {
                JsonDirectories directories = new JsonDirectories();

#if __ANDROID__

#if DEBUG
#warning Compiling android code! 
#endif
                
                DebugHandler.TraceMessage("Running Android Code!", DebugSource.TASK, DebugType.INFO);
                

                string onlyAvailablePath = "/storage/";
                if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.M) {
                    string[] dirs = Directory.GetDirectories("/storage/");

                    foreach (string dir in dirs)
                    {
                        if (dir.Contains("-"))
                        {
                            onlyAvailablePath = Path.Combine(dir, "/Android/data/LittleWeeb.LittleWeeb/files");
                            break;
                        }
                    }
                }
              

                JsonDirectory directory = new JsonDirectory();
                directory.path = onlyAvailablePath;
                directory.dirname = "External Storage if Present.";

                directories.directories.Add(directory);            

                directory = new JsonDirectory();
                directory.path = Android.OS.Environment.RootDirectory.AbsolutePath;
                directory.dirname = "Internal Root Directory";

                directories.directories.Add(directory); 


                return directories.ToJson();
#else

                DebugHandler.TraceMessage("Running Windows Code!", DebugSource.TASK, DebugType.INFO);
                DriveInfo[] allDrives = DriveInfo.GetDrives();
                foreach (DriveInfo drive in allDrives)
                {
                    JsonDirectory directorywithpath = new JsonDirectory
                    {
                        dirname = drive.Name,
                        path = drive.Name
                    };
                    directories.directories.Add(directorywithpath);
                }
                return directories.ToJson();
#endif
            }
            catch (Exception e)
            {

                DebugHandler.TraceMessage("Failed to get drives: " + e.ToString(), DebugSource.TASK, DebugType.WARNING);

                JsonError error = new JsonError
                {
                    type = "get_drives_error",
                    errortype = "exception",
                    errormessage = "Could not get drives, see log.",
                    exception = e.ToString()
                };

                return error.ToJson();
            }
            
        }

        public string GetDirectories(string path)
        {


            DebugHandler.TraceMessage("GetDirectories Called", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage("Directory Path: " + path, DebugSource.TASK, DebugType.PARAMETERS);
           
            
            try
            {
                string[] dirs = Directory.GetDirectories(path);
                
                JsonDirectories tosendover = new JsonDirectories();
                List<JsonDirectory> directorieswithpath = new List<JsonDirectory>();
                foreach (string directory in dirs)
                {
                    JsonDirectory directorywithpath = new JsonDirectory
                    {
                        dirname = directory.Replace(Path.GetDirectoryName(directory) + Path.DirectorySeparatorChar, ""),
                        path = directory
                    };
                    tosendover.directories.Add(directorywithpath);
                }
                return tosendover.ToJson();


            }
            catch (Exception e)
            {
                DebugHandler.TraceMessage("Failed getting directories: " + e.ToString(), DebugSource.TASK, DebugType.WARNING);

                JsonError jsonError = new JsonError
                {
                    type = "get_drives_error",
                    errortype = "exception",
                    errormessage = "Could not get drives, see log.",
                    exception = e.ToString()
                };

                return jsonError.ToJson();
            }
        }

        public string DeleteDirectory(string path)
        {

            DebugHandler.TraceMessage("DeleteDirectory Called", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage("Directory Path: " + path, DebugSource.TASK, DebugType.PARAMETERS);
            try
            {
                DirectoryInfo info = new DirectoryInfo(path);
                int amountOfFiles = info.GetFiles().Length;
                if (amountOfFiles == 0)
                {
                    if (Directory.Exists(path))
                    {

                        Directory.Delete(path);

                        JsonSuccess report = new JsonSuccess()
                        {
                            message = "Succesfully deleted folder with path: " + path
                        };

                        return report.ToJson();

                    }
                    else
                    {

                        JsonSuccess report = new JsonSuccess()
                        {
                            message = "Directory with path : " + path + " already removed."
                        };

                        return report.ToJson();

                    }
                }
                else
                {
                    DebugHandler.TraceMessage("Could not delete directory: " + path + " because there are still files and/or other directories inside!", DebugSource.TASK, DebugType.WARNING);


                    JsonError jsonError = new JsonError
                    {
                        type = "delete_directory_warning",
                        errortype = "warning",
                        errormessage = "Could not delete directory: " + path + " because there are still files and/or other directories inside!"
                    };

                    return jsonError.ToJson();
                }
            }
            catch (Exception e)
            {

                DebugHandler.TraceMessage("Could not delete directory: " + e.ToString(), DebugSource.TASK, DebugType.WARNING);

                JsonError jsonError = new JsonError
                {
                    type = "delete_directory_error",
                    errortype = "exception",
                    errormessage = "Could not get drives, see log."
                };

                return jsonError.ToJson();
            }
        }

        public string CreateDirectory(string path, string name)
        {

            DebugHandler.TraceMessage("CreateDirectory Called", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage("Directory Path: " + path + ", Directory name: " + name, DebugSource.TASK, DebugType.PARAMETERS);
        

            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);

                    return GetDirectories(path);
                }
                else
                {
                    DebugHandler.TraceMessage("Could not create directory: Directory already exists!", DebugSource.TASK, DebugType.WARNING);

                    JsonError jsonError = new JsonError
                    {
                        type = "directory_already_exists",
                        errortype = "warning",
                        errormessage = "Directory already exist."
                    };

                    return jsonError.ToJson();
                }
            }
            catch (Exception e)
            {
                DebugHandler.TraceMessage("Could not create directory: " + e.ToString(), DebugSource.TASK, DebugType.WARNING);

                JsonError jsonError = new JsonError
                {
                    type = "create_directory_error",
                    errortype = "exception",
                    errormessage = "Could not  create directory, see log."
                };

                return jsonError.ToJson();

            }
        }

        
        public string OpenDirectory(string directoryPath)
        {


            DebugHandler.TraceMessage("OpenDirectory Called", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage("Directory Path: " + directoryPath, DebugSource.TASK, DebugType.PARAMETERS);

            try
            {

#if __ANDROID__
                Android.Net.Uri uri = Android.Net.Uri.Parse(directoryPath);
                Android.Content.Intent intent = new Android.Content.Intent(Android.Content.Intent.ActionView);
                intent.SetDataAndType(uri, "*/*");
                intent.SetFlags(Android.Content.ActivityFlags.ClearWhenTaskReset | Android.Content.ActivityFlags.NewTask);
                Android.App.Application.Context.StartActivity(Android.Content.Intent.CreateChooser(intent, "Choose File Explorer"));
#else
                if (UtilityMethods.CheckOperatingSystems() == UtilityMethods.OperatingSystems.Linux)
                {
                    Process.Start("xdg-open", directoryPath);
                }
                else if (UtilityMethods.CheckOperatingSystems() == UtilityMethods.OperatingSystems.OsX)
                {
                    Process.Start("open", directoryPath);
                }
                else
                {
                    Process.Start("explorer.exe", directoryPath);
                }
#endif
                JsonSuccess report = new JsonSuccess()
                {
                    message = "Succesfully opened folder with path: " + directoryPath
                };

                return report.ToJson();
            }
            catch (Exception e)
            {

                DebugHandler.TraceMessage("Could not open directory: " + e.ToString(), DebugSource.TASK, DebugType.WARNING);

                JsonError jsonError = new JsonError
                {
                    type = "open_directory_failed",
                    errormessage = "Could not open directory.",
                    errortype = "exception"
                };

                return jsonError.ToJson();

            }
        }

        public string GetFreeSpace(string path)
        {


            DebugHandler.TraceMessage("GetFreeSpace Called", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage("Directory Path: " + path, DebugSource.TASK, DebugType.PARAMETERS);

            JsonFreeSpace space = new JsonFreeSpace
            {
                freespacebytes = UtilityMethods.GetFreeSpaceKbits(path) * 128,
            };
            space.freespacekbytes = space.freespacebytes * 128 / 1024;
            space.freespacembytes = space.freespacekbytes * 128 / 1024;

            return space.ToJson();
        }
    }
}
