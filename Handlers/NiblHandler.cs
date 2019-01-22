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
        Task<JObject> SearchNibl(string query, string season);
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

                foreach (JObject bot in result.Value<JArray>("content"))
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
                }

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

                JArray array = result.Value<JArray>("content");        

                try
                {
                    foreach (JObject pack in array.Children())
                    {
                        Dictionary<string, string> info = WeebFileNameParser.ParseFullString(pack.Value<string>("name"));
                        info.Add("BotName",  bots.Value<string>(pack.Value<string>("botId")));
                        info.Add("PackNumber", pack.Value<string>("number"));
                        info.Add("FullFileName", pack.Value<string>("name"));
                        listWithPacks.Add(info);
                    }
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


                    DebugHandler.TraceMessage(result.ToString(Formatting.Indented), DebugSource.TASK, DebugType.INFO);

                    JArray array = result.Value<JArray>("content");
                    foreach (JObject pack in array.Children())
                    {
                        Dictionary<string, string> info = WeebFileNameParser.ParseFullString(pack.Value<string>("name"));
                        info.Add("BotName", bots.Value<string>(pack.Value<string>("botId")));
                        info.Add("PackNumber", pack.Value<string>("number"));
                        info.Add("FullFileName", pack.Value<string>("name"));
                        listWithPacks.Add(info);
                    }
                }
                catch (Exception e)
                {
                    DebugHandler.TraceMessage(e.ToString(), DebugSource.TASK, DebugType.WARNING);
                }

                DebugHandler.TraceMessage("Search query: " + query + " Succeeded.", DebugSource.TASK, DebugType.INFO);
                return JObject.Parse("{\"packs\":" + JsonConvert.SerializeObject(listWithPacks) + " }");
            }
        }

        private async Task<string> Get(string url)
        {
            DebugHandler.TraceMessage("Get called", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage("URL: " + url, DebugSource.TASK, DebugType.PARAMETERS);
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync("https://api.nibl.co.uk:8080/nibl/" + url);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                else
                {
                    return "failed: " + response.StatusCode.ToString();
                }
            }

        }

        public Task<JObject> SearchNibl(string query, string season)
        {
            throw new NotImplementedException();
        }
    }
}
