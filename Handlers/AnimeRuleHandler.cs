using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace LittleWeebLibrary.Handlers
{
    public interface IAnimeRuleHandler
    {
        Task<JObject> FilterAnimeRules(JObject niblData, string animeId);
        Task AddRules(JObject rules, JObject customRules);

    }
    public class AnimeRuleHandler : IAnimeRuleHandler
    {
        private readonly IDataBaseHandler DataBaseHandler;
        private readonly IDebugHandler DebugHandler;
        private JObject GlobalRules;

        public AnimeRuleHandler(IDataBaseHandler dataBaseHandler, IDebugHandler debugHandler)
        {
            debugHandler.TraceMessage("Constructor Called", DebugSource.CONSTRUCTOR, DebugType.ENTRY_EXIT);
            DebugHandler = debugHandler;
            DataBaseHandler = dataBaseHandler;
            Init();

        }

        public Task AddRules(JObject rules, JObject customRules)
        {
            throw new NotImplementedException();
        }

        public async Task<JObject> FilterAnimeRules(JObject niblData, string animeId)
        {
            throw new NotImplementedException();
        }

        private async void Init()
        {
            string currentRules = await Get("littleweebrules.json");
            GlobalRules = JObject.Parse(currentRules);
        }

        private async Task<string> Get(string url)
        {
            DebugHandler.TraceMessage("Get called", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage("URL: " + url, DebugSource.TASK, DebugType.PARAMETERS);
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync("https://raw.githubusercontent.com/littleweeb/LittleWeebRules/master/" + url);

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
    }
}
