
using LittleWeebLibrary.EventArguments;
using LittleWeebLibrary.GlobalInterfaces;
using LittleWeebLibrary.Models;
using LittleWeebLibrary.StaticClasses;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace LittleWeebLibrary.Handlers
{
    public interface IVersionHandler
    {
        JsonVersionInfo GetLocalVersion();
        Task<JsonVersionInfo> GetLatestVersionAndroid(bool release);
        Task<JsonVersionInfo> GetLatestVersionDesktop(bool release);
    }

    public class VersionHandler : IVersionHandler, IDebugEvent
    {
        public event EventHandler<BaseDebugArgs> OnDebugEvent;

        public VersionHandler()
        {

        }

        public async Task<JsonVersionInfo> GetLatestVersionDesktop(bool release)
        {

            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugMessage = "GetLatestVersionDesktop called.",
                DebugSource = this.GetType().Name,
                DebugSourceType = 1,
                DebugType = 0
            });


            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugMessage = "Is release mode: " + release.ToString(),
                DebugSource = this.GetType().Name,
                DebugSourceType = 1,
                DebugType = 1
            });


            JsonVersionInfo versionInfo = new JsonVersionInfo();
            try
            {
                HttpClient client = new HttpClient();

                var request = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/repos/littleweeb/Desktop/releases");
                request.Headers.Add("Accept", "application/vnd.github.v3+json");
                request.Headers.Add("User-Agent", "LittleWeeb");

                var response = await client.SendAsync(request);


                if (response.IsSuccessStatusCode)
                {
                    OnDebugEvent?.Invoke(this, new BaseDebugArgs()
                    {
                        DebugMessage = "Succesfully retreived releases!",
                        DebugSource = this.GetType().Name,
                        DebugSourceType = 1,
                        DebugType = 2
                    });

                    string json = await response.Content.ReadAsStringAsync(); 

                    OnDebugEvent?.Invoke(this, new BaseDebugArgs()
                    {
                        DebugMessage = "Releases: " + json,
                        DebugSource = this.GetType().Name,
                        DebugSourceType = 1,
                        DebugType = 2
                    });

                    JArray jsonResult = JArray.Parse(json);



                    foreach (JObject latestRelease in jsonResult.Children<JObject>())
                    {
                        string mode = "develop";
                        if (release)
                        {
                            mode = "master";
                        }
                        if (latestRelease.Value<string>("target_commitish") == mode)
                        {

                            string latestTag = latestRelease.Value<string>("tag_name");

                            string tagShouldContain = "win";
                            if (UtilityMethods.CheckOperatingSystems() == UtilityMethods.OperatingSystems.Linux)
                            {
                                tagShouldContain = "linux";
                            }
                            else if (UtilityMethods.CheckOperatingSystems() == UtilityMethods.OperatingSystems.OsX)
                            {
                                tagShouldContain = "mac";
                            }

                            if (latestTag.Contains(tagShouldContain))
                            {
                                string latestVersion = latestTag.Split('_')[0];
                                string releaseUrl = latestRelease.Value<string>("html_url");
                                string date = latestRelease.Value<string>("published_at");

                                JArray assets = latestRelease.Value<JArray>("assets");

                                JObject latestAsset = assets.Last.ToObject<JObject>();

                                string latestDownload = latestAsset.Value<string>("browser_download_url");
                                string latestFileName = latestAsset.Value<string>("name");

                                versionInfo.newbuild = latestFileName.Split('.')[latestFileName.Split('.').Length - 2];
                                versionInfo.newversion = latestVersion;
                                versionInfo.release_url = releaseUrl;
                                versionInfo.direct_download_url = latestDownload;
                                versionInfo.file_name = latestFileName;
                                versionInfo.date = date;
                                versionInfo.release_version = mode;
                                return versionInfo;
                            }                           
                          
                        }
                    }

                    OnDebugEvent?.Invoke(this, new BaseDebugArgs()
                    {
                        DebugMessage = "No matching releases found!",
                        DebugSource = this.GetType().Name,
                        DebugSourceType = 1,
                        DebugType = 3
                    });
                    return versionInfo;
                }
                else
                {
                    OnDebugEvent?.Invoke(this, new BaseDebugArgs()
                    {
                        DebugMessage = "Failed retreiving releases!",
                        DebugSource = this.GetType().Name,
                        DebugSourceType = 1,
                        DebugType = 3
                    });
                    return versionInfo;
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

                return versionInfo;
            }
        }


        public async Task<JsonVersionInfo> GetLatestVersionAndroid(bool release)
        {
            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugMessage = "GetLatestVersionAndroid called.",
                DebugSource = this.GetType().Name,
                DebugSourceType = 1,
                DebugType = 0
            });

            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugMessage =  "Is release mode: " + release.ToString(),
                DebugSource = this.GetType().Name,
                DebugSourceType = 1,
                DebugType = 1
            });

            JsonVersionInfo versionInfo = new JsonVersionInfo();
            try
            {
                HttpClient client = new HttpClient();

                var request = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/repos/littleweeb/Android/releases");
                request.Headers.Add("Accept", "application/vnd.github.v3+json");
                request.Headers.Add("User-Agent", "LittleWeeb");


                var response = await client.SendAsync(request);


                if (response.IsSuccessStatusCode)
                {
                    OnDebugEvent?.Invoke(this, new BaseDebugArgs()
                    {
                        DebugMessage = "Succesfully retreived releases!",
                        DebugSource = this.GetType().Name,
                        DebugSourceType = 1,
                        DebugType = 2
                    });

                    string json = await response.Content.ReadAsStringAsync();

                    OnDebugEvent?.Invoke(this, new BaseDebugArgs()
                    {
                        DebugMessage = "Releases: " + json,
                        DebugSource = this.GetType().Name,
                        DebugSourceType = 1,
                        DebugType = 2
                    });

                    JArray jsonResult = JArray.Parse(json);



                    foreach (JObject latestRelease in jsonResult.Children<JObject>())
                    {
                        string mode = "develop";
                        if (release)
                        {
                            mode = "master";
                        }
                        if (latestRelease.Value<string>("target_commitish") == mode)
                        {
                            string latestTag = latestRelease.Value<string>("tag_name");
                            string latestVersion = latestTag.Split('_')[0];
                            string releaseUrl = latestRelease.Value<string>("html_url");
                            string date = latestRelease.Value<string>("published_at");

                            JArray assets = latestRelease.Value<JArray>("assets");

                            JObject latestAsset = assets.Last.ToObject<JObject>();

                            string latestDownload = latestAsset.Value<string>("browser_download_url");
                            string latestFileName = latestAsset.Value<string>("name");

                            versionInfo.newbuild = latestFileName.Split('.')[latestFileName.Split('.').Length - 2];
                            versionInfo.newversion = latestVersion;
                            versionInfo.release_url = releaseUrl;
                            versionInfo.direct_download_url = latestDownload;
                            versionInfo.file_name = latestFileName;
                            versionInfo.date = date;
                            versionInfo.release_version = mode;
                            return versionInfo;
                        }
                    }


                    OnDebugEvent?.Invoke(this, new BaseDebugArgs()
                    {
                        DebugMessage = "No matching releases found!",
                        DebugSource = this.GetType().Name,
                        DebugSourceType = 1,
                        DebugType = 3
                    });
                    return versionInfo;
                }
                else
                {
                    OnDebugEvent?.Invoke(this, new BaseDebugArgs()
                    {
                        DebugMessage = "Failed retreiving releases!",
                        DebugSource = this.GetType().Name,
                        DebugSourceType = 1,
                        DebugType = 3
                    });
                    return versionInfo;
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

                return versionInfo;
            }
        }

        public JsonVersionInfo GetLocalVersion()
        {
            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugMessage = "GetLocalVersion called.",
                DebugSource = this.GetType().Name,
                DebugSourceType = 1,
                DebugType = 0
            });
            JsonVersionInfo info = new JsonVersionInfo();

            try
            {
                string versionPath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "version.json");

                string contents = File.ReadAllText(versionPath);

                JObject currentVersionJson = JObject.Parse(contents);

                info.currentbuild = currentVersionJson.Value<string>("build");
                info.currentversion = currentVersionJson.Value<string>("version");
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
            }
           
            return info;
        }
    }
}
