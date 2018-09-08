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
    public class DirectoryHandler :  IDirectoryHandler, IDebugEvent
    {
        public event EventHandler<BaseDebugArgs> OnDebugEvent;


        public string GetDrives()
        {

            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugMessage = "GetDrives Called.",
                DebugSource = this.GetType().Name,
                DebugSourceType = 1,
                DebugType = 0
            });

            try
            {
                JsonDirectories directories = new JsonDirectories();

#if __ANDROID__

#if DEBUG
#warning Compiling android code! 
#endif

                OnDebugEvent?.Invoke(this, new BaseDebugArgs()
                {
                    DebugMessage = "Running Android Code!.",
                    DebugSource = this.GetType().Name,
                    DebugSourceType = 1,
                    DebugType = 2
                });

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
                DriveInfo[] allDrives = DriveInfo.GetDrives();
                foreach (DriveInfo drive in allDrives)
                {
                    JsonDirectory directorywithpath = new JsonDirectory();
                    directorywithpath.dirname = drive.Name;
                    directorywithpath.path = drive.Name;
                    directories.directories.Add(directorywithpath);
                }
                return directories.ToJson();
#endif
            }
            catch (Exception e)
            {
                OnDebugEvent?.Invoke(this, new BaseDebugArgs()
                {
                    DebugMessage = e.ToString(),
                    DebugSource = this.GetType().Name,
                    DebugSourceType = 1,
                    DebugType = 4
                });

                JsonError error = new JsonError();
                error.type = "get_drives_error";
                error.errortype = "exception";
                error.errormessage = "Could not get drives, see log.";
                error.exception = e.ToString();

                return error.ToJson();
            }
            
        }

        public string GetDirectories(string path)
        {


            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugMessage = "GetDirectories Called.",
                DebugSource = this.GetType().Name,
                DebugSourceType = 1,
                DebugType = 0
            });

            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugMessage = path,
                DebugSource = this.GetType().Name,
                DebugSourceType = 1,
                DebugType = 1
            });
            
            try
            {
                string[] dirs = Directory.GetDirectories(path);
                
                JsonDirectories tosendover = new JsonDirectories();
                List<JsonDirectory> directorieswithpath = new List<JsonDirectory>();
                foreach (string directory in dirs)
                {
                    JsonDirectory directorywithpath = new JsonDirectory();
                    directorywithpath.dirname = directory.Replace(Path.GetDirectoryName(directory) + Path.DirectorySeparatorChar, "");
                    directorywithpath.path = directory;
                    tosendover.directories.Add(directorywithpath);
                }
                return tosendover.ToJson();


            }
            catch (Exception e)
            {
                OnDebugEvent?.Invoke(this, new BaseDebugArgs()
                {
                    DebugMessage = e.ToString(),
                    DebugSource = this.GetType().Name,
                    DebugSourceType = 1,
                    DebugType = 4
                });

                JsonError error = new JsonError();
                error.type = "get_drives_error";
                error.errortype = "exception";
                error.errormessage = "Could not get drives, see log.";
                error.exception = e.ToString();

                return error.ToJson();
            }
        }

        public string DeleteDirectory(string path)
        {
            try
            {
                DirectoryInfo info = new DirectoryInfo(path);
                int amountOfFiles = info.GetFiles().Length;
                if (amountOfFiles == 0)
                {
                    if (Directory.Exists(path))
                    {

                        Directory.Delete(path);

                        JsonSuccesReport report = new JsonSuccesReport()
                        {
                            message = "Succesfully deleted folder with path: " + path
                        };

                        return report.ToJson();

                    }
                    else
                    {

                        JsonSuccesReport report = new JsonSuccesReport()
                        {
                            message = "Directory with path : " + path + " already removed."
                        };

                        return report.ToJson();

                    }
                }
                else
                {
                    OnDebugEvent?.Invoke(this, new BaseDebugArgs()
                    {
                        DebugMessage = "Could not delete directory: " + path + " because there are still files and/or other directories inside!",
                        DebugSource = this.GetType().Name,
                        DebugSourceType = 1,
                        DebugType = 3
                    });

                    JsonError error = new JsonError();
                    error.type = "delete_directory_warning";
                    error.errortype = "warning";
                    error.errormessage = "Could not delete directory: " + path + " because there are still files and/or other directories inside!";

                    return error.ToJson();
                }
            }
            catch (Exception e)
            {
                OnDebugEvent?.Invoke(this, new BaseDebugArgs()
                {
                    DebugMessage = e.ToString(),
                    DebugSource = this.GetType().Name,
                    DebugSourceType = 1,
                    DebugType = 4
                });

                JsonError error = new JsonError();
                error.type = "delete_directory_error";
                error.errortype = "exception";
                error.errormessage = "Could not get drives, see log.";

                return error.ToJson();
            }
        }

        public string CreateDirectory(string path, string name)
        {

            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugMessage = "CreateDirectory Called.",
                DebugSource = this.GetType().Name,
                DebugSourceType = 1,
                DebugType = 0
            });

            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugMessage = path,
                DebugSource = this.GetType().Name,
                DebugSourceType = 1,
                DebugType = 1
            });

            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugMessage = name,
                DebugSource = this.GetType().Name,
                DebugSourceType = 1,
                DebugType = 1
            });

            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);

                    return GetDirectories(path);
                }
                else
                {
                    JsonError err = new JsonError();
                    err.type = "creating_directory_path_already_exists";

                    OnDebugEvent?.Invoke(this, new BaseDebugArgs()
                    {
                        DebugMessage = "Directory already exists!",
                        DebugSource = this.GetType().Name,
                        DebugSourceType = 1,
                        DebugType = 3
                    });

                    JsonError error = new JsonError();
                    error.type = "directory_already_exists";
                    error.errortype = "warning";
                    error.errormessage = "Directory already exist.";

                    return error.ToJson();
                }
            }
            catch (Exception e)
            {
                OnDebugEvent?.Invoke(this, new BaseDebugArgs()
                {
                    DebugMessage = e.ToString(),
                    DebugSource = this.GetType().Name,
                    DebugSourceType = 1,
                    DebugType = 4
                });

                JsonError error = new JsonError();
                error.type = "create_directory_error";
                error.errortype = "exception";
                error.errormessage = "Could not  create directory, see log.";

                return error.ToJson();

            }
        }

        
        public string OpenDirectory(string directoryPath)
        {
            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugMessage = "OpenDirectory Called.",
                DebugSource = this.GetType().Name,
                DebugSourceType = 1,
                DebugType = 0
            });

            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugMessage = directoryPath,
                DebugSource = this.GetType().Name,
                DebugSourceType = 1,
                DebugType = 1
            });

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
                JsonSuccesReport report = new JsonSuccesReport()
                {
                    message = "Succesfully opened folder with path: " + directoryPath
                };

                return report.ToJson();
            }
            catch (Exception e)
            {

                OnDebugEvent?.Invoke(this, new BaseDebugArgs()
                {
                    DebugMessage = e.ToString(),
                    DebugSource = this.GetType().Name,
                    DebugSourceType = 1,
                    DebugType = 4
                });


                JsonError err = new JsonError();
                err.type = "open_directory_failed";
                err.errormessage = "Could not open directory.";
                err.errortype = "exception";

                return err.ToJson();

            }
        }

        public string GetFreeSpace(string path)
        {
            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugMessage = "GetFreeSpace Called.",
                DebugSource = this.GetType().Name,
                DebugSourceType = 1,
                DebugType = 0
            });

            JsonFreeSpace space = new JsonFreeSpace();
            space.freespacebytes = UtilityMethods.GetFreeSpace(path);
            space.freespacekbytes = space.freespacebytes / 1024;
            space.freespacembytes = space.freespacekbytes / 1024;

            return space.ToJson();
        }
    }
}
