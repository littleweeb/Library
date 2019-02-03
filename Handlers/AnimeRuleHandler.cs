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
        Task<bool> AddRules(JObject customRules, string anime_id);

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

        public async Task<bool> AddRules(JObject customRules, string anime_id)
        {
            DebugHandler.TraceMessage("AddRules Called.", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage("For anime: " + anime_id, DebugSource.TASK, DebugType.PARAMETERS);

            JObject anime = await DataBaseHandler.GetJObject("anime", anime_id);

            JObject currentRules = anime.Value<JObject>("anime_rules");

            currentRules.Merge(customRules);

            anime["anime_rules"] = currentRules;

            return await DataBaseHandler.UpdateJObject("anime", anime, anime_id);

        }

        public async Task<JObject> FilterAnimeRules(JObject niblData, string animeId)
        {
            DebugHandler.TraceMessage("FilterAnimeRules Called.", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage("For anime: " + animeId, DebugSource.TASK, DebugType.PARAMETERS);

            JObject anime = await DataBaseHandler.GetJObject("anime", animeId);

            JObject localRules = anime.Value<JObject>("anime_rules");

            JArray must_contain = new JArray();
            JArray cannot_contain = new JArray();

            if (GlobalRules.ContainsKey(animeId)){
                JObject globalRules = GlobalRules.Value<JObject>(animeId);
                must_contain.Merge(globalRules.Value<JArray>("must_contain"));
                cannot_contain.Merge(globalRules.Value<JArray>("cannot_contain"));


                DebugHandler.TraceMessage("Found global rules for anime : " + animeId, DebugSource.TASK, DebugType.INFO);
                DebugHandler.TraceMessage("Must contain : " + must_contain.ToString(), DebugSource.TASK, DebugType.INFO);
                DebugHandler.TraceMessage("Cannot contain : " + cannot_contain.ToString(), DebugSource.TASK, DebugType.INFO);
            }

            if (localRules.Count > 0)
            {
                must_contain.Merge(localRules.Value<JArray>("must_contain"));
                cannot_contain.Merge(localRules.Value<JArray>("cannot_contain"));

                DebugHandler.TraceMessage("Found local rules for anime : " + animeId, DebugSource.TASK, DebugType.INFO);
                DebugHandler.TraceMessage("Must contain : " + must_contain.ToString(), DebugSource.TASK, DebugType.INFO);
                DebugHandler.TraceMessage("Cannot contain : " + cannot_contain.ToString(), DebugSource.TASK, DebugType.INFO);
            }

            if (must_contain.Count > 0 || cannot_contain.Count > 0)
            {
                JArray listWithPacks = niblData.Value<JArray>("packs");


                //DebugHandler.TraceMessage("Start iterating files from nibl with rules: " + listWithPacks.Count.ToString(), DebugSource.TASK, DebugType.INFO);

                JArray newListWithPacks = new JArray();

                foreach (JObject pack in listWithPacks)
                {
                    if (pack.ContainsKey("FullFileName"))
                    {
                        string episodeFileName = pack.Value<string>("FullFileName");

                        bool must_contain_check = true;
                        bool cannot_contain_check = true;


                        if (must_contain.Count > 0)
                        {
                            foreach (string mustcontain in must_contain)
                            {
                                if (mustcontain != string.Empty)
                                {
                                    if (episodeFileName.ToUpper().Contains(mustcontain.ToUpper()))
                                    {
                                        must_contain_check = true;
                                        break;
                                    }
                                    else
                                    {
                                        must_contain_check = false;
                                    }
                                }
                            }
                        }
                        if (cannot_contain.Count > 0)
                        {
                            foreach (string cannotcontain in cannot_contain)
                            {
                                if (cannotcontain != string.Empty)
                                {
                                    if (episodeFileName.ToUpper().Contains(cannotcontain.ToUpper()))
                                    {
                                        cannot_contain_check = false;
                                        break;
                                    }
                                }
                            }
                        }

                        if (must_contain_check && cannot_contain_check)
                        {
                           // DebugHandler.TraceMessage("File with filename " + episodeFileName + " passes rule check!", DebugSource.TASK, DebugType.INFO);
                            newListWithPacks.Add(pack);
                        }
                    }
                }
                niblData["packs"] = newListWithPacks;

                DebugHandler.TraceMessage("Finished parsing rules, new list with files size: " + newListWithPacks.Count.ToString(), DebugSource.TASK, DebugType.INFO);
                return niblData;
            }
            else
            {
                DebugHandler.TraceMessage("No rules found for anime with id: " + animeId + ", returning unparsed list.", DebugSource.TASK, DebugType.INFO);
                return niblData;
            }
        }

        private async void Init()
        {
            DebugHandler.TraceMessage("Init called", DebugSource.TASK, DebugType.ENTRY_EXIT);
            string currentRules = await Get("littleweebrules.json");
            DebugHandler.TraceMessage("Result: " + currentRules, DebugSource.TASK, DebugType.INFO);
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
