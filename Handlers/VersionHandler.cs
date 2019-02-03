
using Android.Content.Res;
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

    public class VersionHandler : IVersionHandler
    {
       

        private readonly IDebugHandler DebugHandler;

        public VersionHandler(IDebugHandler debugHandler)
        {
            debugHandler.TraceMessage("Constructor Called.", DebugSource.CONSTRUCTOR, DebugType.ENTRY_EXIT);
            DebugHandler = debugHandler;
        }

        public async Task<JsonVersionInfo> GetLatestVersionDesktop(bool release)
        {
            DebugHandler.TraceMessage("GetLatestVersionDesktop Called.", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage("Is release mode: " + release.ToString(), DebugSource.TASK, DebugType.PARAMETERS);


            JsonVersionInfo versionInfo = new JsonVersionInfo();
            try
            {
                HttpClient client = new HttpClient();

                var request = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/repos/littleweeb/Desktop/releases?access_token=018003ade567151524c10210f0bd97b05b6ec96b");
                request.Headers.Add("Accept", "application/vnd.github.v3+json");
                request.Headers.Add("User-Agent", "LittleWeeb");

                var response = await client.SendAsync(request);


                if (response.IsSuccessStatusCode)
                {
                    DebugHandler.TraceMessage("Succesfully retrieved releases!", DebugSource.TASK, DebugType.INFO);

                    string json = await response.Content.ReadAsStringAsync(); 

                    DebugHandler.TraceMessage("Releases: " + json, DebugSource.TASK, DebugType.INFO);
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

                    DebugHandler.TraceMessage("No matching releases found!", DebugSource.TASK, DebugType.INFO);
                    return versionInfo;
                }
                else
                {
                    DebugHandler.TraceMessage("Failed retreiving releases!", DebugSource.TASK, DebugType.WARNING);
                    return versionInfo;
                }
            }
            catch (Exception e)
            {
                DebugHandler.TraceMessage(e.ToString(), DebugSource.TASK, DebugType.WARNING);

                return versionInfo;
            }
        }


        public async Task<JsonVersionInfo> GetLatestVersionAndroid(bool release)
        {

            DebugHandler.TraceMessage("GetLatestVersionAndroid Called.", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage("Is release mode: " + release.ToString(), DebugSource.TASK, DebugType.PARAMETERS);


            JsonVersionInfo versionInfo = new JsonVersionInfo();
            try
            {
                HttpClient client = new HttpClient();

                var request = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/repos/littleweeb/Android/releases?access_token=018003ade567151524c10210f0bd97b05b6ec96b");
                request.Headers.Add("Accept", "application/vnd.github.v3+json");
                request.Headers.Add("User-Agent", "LittleWeeb");


                var response = await client.SendAsync(request);


                if (response.IsSuccessStatusCode)
                {
                    DebugHandler.TraceMessage("Succesfully retrieved releases!", DebugSource.TASK, DebugType.INFO);

                    string json = await response.Content.ReadAsStringAsync();



                    DebugHandler.TraceMessage("Release: " + json, DebugSource.TASK, DebugType.INFO);
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


                    DebugHandler.TraceMessage("No matching releases found!", DebugSource.TASK, DebugType.INFO);
                    return versionInfo;
                }
                else
                {
                    DebugHandler.TraceMessage("Failed retreiving releases!", DebugSource.TASK, DebugType.WARNING);
                    return versionInfo;
                }
            }
            catch (Exception e)
            {

                DebugHandler.TraceMessage(e.ToString(), DebugSource.TASK, DebugType.WARNING);

                return versionInfo;
            }
        }

        public JsonVersionInfo GetLocalVersion()
        {
            DebugHandler.TraceMessage("GetLocalVersion Called.", DebugSource.TASK, DebugType.ENTRY_EXIT);
            JsonVersionInfo info = new JsonVersionInfo();

            try
            {


                string contents = "";
#if __ANDROID__               
                var stream = Android.App.Application.Context.Assets.Open("version.json");

                StreamReader sr = new StreamReader(stream);
                contents = sr.ReadToEnd();
                sr.Close();
#else
                string versionPath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "version.json");
                contents = File.ReadAllText(versionPath);
#endif


                JObject currentVersionJson = JObject.Parse(contents);

                info.currentbuild = currentVersionJson.Value<string>("build");
                info.currentversion = currentVersionJson.Value<string>("version");
            }
            catch (Exception e)
            {

                DebugHandler.TraceMessage(e.ToString(), DebugSource.TASK, DebugType.WARNING);
            }
           
            return info;
        }
    }
}
