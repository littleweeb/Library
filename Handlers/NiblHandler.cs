using LittleWeebLibrary.EventArguments;
using LittleWeebLibrary.GlobalInterfaces;
using LittleWeebLibrary.Models;
using LittleWeebLibrary.StaticClasses;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using WeebFileNameParserLibrary;

namespace LittleWeebLibrary.Handlers
{
    public interface INiblHandler
    {
        Task<JObject> GetBotList();
        Task<JObject> SearchNibl(string query);
        Task<JObject> SearchNibl(List<string> query, int episode = -1, string sort = "name", string order = "ASC");
        Task<int> GetLatestEpisode(string query);
        Task<JObject> GetLatestFiles(string botid);

    }
    public class NiblHandler :  INiblHandler
    {
        private WeebFileNameParser WeebFileNameParser;
        private readonly IKitsuHandler KitsuHandler;
        private readonly IDebugHandler DebugHandler;
        public NiblHandler(IKitsuHandler kitsuHandler, IDebugHandler debugHandler)
        {
            debugHandler.TraceMessage("Constructor Called", DebugSource.CONSTRUCTOR, DebugType.ENTRY_EXIT);
            KitsuHandler = kitsuHandler;
            WeebFileNameParser = new WeebFileNameParser();
            DebugHandler = debugHandler;
        }

        public async Task<JObject> GetBotList()
        {
            DebugHandler.TraceMessage("GetBotList called", DebugSource.TASK, DebugType.ENTRY_EXIT);

            string bots = await Get("bots");

            if (bots.Contains("failed:"))
            {
                return new JObject();
            }
            else
            {
                JObject result = JObject.Parse(bots);

                Dictionary<int, string> botVsId = new Dictionary<int, string>();


                JObject[] array = result.Value<JArray>("content").ToObject<JObject[]>();
                Parallel.ForEach(array, (bot) =>
                {
                    string name = bot.Value<string>("name");
                    int id = bot.Value<int>("id");

                    try
                    {
                        botVsId.Add(id, name);
                    }
                    catch
                    {
                        DebugHandler.TraceMessage("Could not add BOT: " + name + " with id : " + id + ", id already exists!", DebugSource.TASK, DebugType.WARNING);
                    }
                });

                return JObject.FromObject(botVsId);
            }
        }

        //TODO: remove repeating anime titles, create title parse for episodes 
        public async Task<JObject> GetLatestFiles(string botid)
        {
            DebugHandler.TraceMessage("GetLatestFiles called", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage("Bot ID: " + botid, DebugSource.TASK, DebugType.PARAMETERS);

            JObject bots = await GetBotList();

            string filesresult = await Get("packs/" + botid + "/page?episodeNumber=0&page=0&size=500&sort=lastModified&direction=DESC");
                                          
            if (filesresult.Contains("failed:"))
            {
                return new JObject();
            }
            else
            {
                var result = (JObject)JsonConvert.DeserializeObject(filesresult);

                List<Dictionary<string, string>> listWithPacks = new List<Dictionary<string, string>>();

                JObject[] array = result.Value<JArray>("content").ToObject<JObject[]>();

                try
                {
                    Parallel.ForEach(array, (pack) =>
                    {
                        Dictionary<string, string> info = WeebFileNameParser.ParseFullString(pack.Value<string>("name"));
                        info.Add("BotName", bots.Value<string>(pack.Value<string>("botId")));
                        info.Add("PackNumber", pack.Value<string>("number"));
                        info.Add("FullFileName", pack.Value<string>("name"));
                        info.Add("FileSize", (long.Parse(pack.Value<long>("sizekbits").ToString())/1024).ToString());
                        listWithPacks.Add(info);
                    });
                }
                catch(Exception e)
                {
                    DebugHandler.TraceMessage(e.ToString(), DebugSource.TASK, DebugType.WARNING);
                }

                return JObject.Parse("{\"bot\":\"" + bots.Value<string>(botid) + "\",\"packs\":" + JsonConvert.SerializeObject(listWithPacks) + " }");
            }
        }

        public async Task<JObject> SearchNibl(string query)
        {

            DebugHandler.TraceMessage("SearchNibl called", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage("Search query: " + query, DebugSource.TASK, DebugType.PARAMETERS);

            string searchresult = await Get("search?query=" + query);

            if (searchresult.Contains("failed:"))
            {

                DebugHandler.TraceMessage("Search query: " + query + " FAILED.", DebugSource.TASK, DebugType.WARNING);
                return new JObject();
            }
            else
            {

                DebugHandler.TraceMessage("Search query: " + query + " SUCCEEDED.", DebugSource.TASK, DebugType.INFO);
                List<Dictionary<string, string>> listWithPacks = new List<Dictionary<string, string>>();

                try
                {
                    JObject bots = await GetBotList();
                    JObject result = JObject.Parse(searchresult);
                    JObject[] array = result.Value<JArray>("content").ToObject<JObject[]>();
                    Parallel.ForEach(array, (pack) =>
                    {
                        Dictionary<string, string> info = WeebFileNameParser.ParseFullString(pack.Value<string>("name"));
                        info.Add("BotName", bots.Value<string>(pack.Value<string>("botId")));
                        info.Add("PackNumber", pack.Value<string>("number"));
                        info.Add("FullFileName", pack.Value<string>("name"));
                        info.Add("FileSize", (long.Parse(pack.Value<long>("sizekbits").ToString()) / 1024).ToString());
                        listWithPacks.Add(info);
                    });
                }
                catch (Exception e)
                {
                    DebugHandler.TraceMessage(e.ToString(), DebugSource.TASK, DebugType.WARNING);
                }

                DebugHandler.TraceMessage("Search query: " + query + " Succeeded.", DebugSource.TASK, DebugType.INFO);
                return JObject.Parse("{\"packs\":" + JsonConvert.SerializeObject(listWithPacks) + " }");
            }
        }

        public async Task<JObject> SearchNibl(List<string> query,  int episode = -1, string sort = "name", string order = "ASC")
        {
            List<string> urls = new List<string>();
            foreach(string searchquery in query)
            {
                if (episode > -1)
                {
                    urls.Add("search/page?query=" + searchquery + "&size=4000&sort=" + sort + "&direction=" + order + "&episodeNumber=" + episode.ToString());
                }
                else
                {
                    urls.Add("search/page?query=" + searchquery + "&size=4000&sort=" + sort + "&direction=" + order);
                }
            }

            List<Task<string>> tasks = new List<Task<string>>();
            foreach (string url in urls)
            {
                tasks.Add(Get(url));
            }

            await Task.WhenAll(tasks.ToArray());

            JObject result = new JObject();

          
            foreach (Task<string> task in tasks)
            {
                result.Merge(JObject.Parse(task.Result));
                task.Dispose();
            }

            JObject bots = await GetBotList();
            JObject[] array = result.Value<JArray>("content").ToObject<JObject[]>();

            List<Dictionary<string, string>> listWithPacks = new List<Dictionary<string, string>>();


            Parallel.ForEach(array, (pack) =>
            {
                Dictionary<string, string> info = WeebFileNameParser.ParseFullString(pack.Value<string>("name"));
                info.Add("BotName", bots.Value<string>(pack.Value<string>("botId")));
                info.Add("PackNumber", pack.Value<string>("number"));
                info.Add("FullFileName", pack.Value<string>("name"));
                info.Add("FileSize", (long.Parse(pack.Value<long>("sizekbits").ToString()) / 1024).ToString());
                listWithPacks.Add(info);
            });

            DebugHandler.TraceMessage("Search query: " + query + " Succeeded. Total Results: " + array.Count(), DebugSource.TASK, DebugType.INFO);
            return JObject.Parse("{\"packs\":" + JsonConvert.SerializeObject(listWithPacks) + " }");

        }

       
        public async Task<int> GetLatestEpisode(string query)
        {
            string searchresult = await Get("search/page?query=" + query + "&episodeNumber=0&page=0&size=100&sort=episodeNumber&direction=DESC");


            int season = -1;

            Dictionary<string, string> parsedquery = WeebFileNameParser.ParseFullString(query);

            if (parsedquery.ContainsKey("Season"))
            {
                season = int.Parse(parsedquery["Season"]);
            }

            if (searchresult.Contains("failed:"))
            {

                DebugHandler.TraceMessage("Search query: " + query + " FAILED.", DebugSource.TASK, DebugType.WARNING);
                return -1;
            }
            else
            {

                DebugHandler.TraceMessage("Search query: " + query + " SUCCEEDED.", DebugSource.TASK, DebugType.INFO);

                try
                {
                    JObject result = JObject.Parse(searchresult);
                    JObject[] array = result.Value<JArray>("content").ToObject<JObject[]>();

                    int largest = 0;
                    Parallel.ForEach(array, (pack) =>
                    {
                        Dictionary<string, string> info = WeebFileNameParser.ParseFullString(pack.Value<string>("name"));

                        if (season != -1)
                        {
                            if (info.ContainsKey("Season"))
                            {
                                if (int.Parse(info["Season"]) == season)
                                {
                                    if (pack.Value<int>("episodeNumber") > largest)
                                    {
                                        largest = pack.Value<int>("episodeNumber");
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (pack.Value<int>("episodeNumber") > largest)
                            {
                                largest = pack.Value<int>("episodeNumber");
                            }
                        }
                    });

                    int episodeNumber = largest;
                    return episodeNumber;
                    
                }
                catch (Exception e)
                {
                    DebugHandler.TraceMessage(e.ToString(), DebugSource.TASK, DebugType.WARNING);
                    return -1;
                }

            }
        }

        private async Task<string> Get(string url)
        {
            DebugHandler.TraceMessage("Get called", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage("URL: " + url, DebugSource.TASK, DebugType.PARAMETERS);
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync("https://api.nibl.co.uk:8080/nibl/" + url).ConfigureAwait(false);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                }
                else
                {
                    return "failed: " + response.StatusCode.ToString();
                }
            }

        }

    }
}
