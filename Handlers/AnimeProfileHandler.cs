using LittleWeebLibrary.EventArguments;
using LittleWeebLibrary.GlobalInterfaces;
using LittleWeebLibrary.Models;
using LittleWeebLibrary.StaticClasses;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WeebFileNameParserLibrary;

/**
 * 
 *  THIS CLASS I SUPPOSED TO BE THE SERVICE! NOT A HANDLER!
 * 
 * */

namespace LittleWeebLibrary.Handlers
{
    public interface IAnimeProfileHandler
    {
        Task<JsonKitsuAnimeInfo> GetAnimeProfile(string id, int amountPerPage = 26, bool noCache = false);
        Task<JsonKitsuAnimeInfo> GetAnimeEpisodes(string id, int amountPerPage = 26, bool noCache = false, bool cached = false);
        Task<JsonCurrentlyAiring> GetCurrentlyAiring(bool nonFoundAnimes = false, int botId = 21, bool noCache = false, bool cached = false);
        // Task<JObject> GetAnime(string id, string episodePage, string bot, string resolution);

    }
    public class AnimeProfileHandler : IAnimeProfileHandler
    {
        private readonly IKitsuHandler KitsuHandler;
        private readonly INiblHandler NiblHandler;
        private readonly IDataBaseHandler DataBaseHandler;
        private readonly IAnimeRuleHandler AnimeRuleHandler;
        private readonly IDebugHandler DebugHandler;
        private readonly WeebFileNameParser WeebFileNameParser;

        public AnimeProfileHandler(IKitsuHandler kitsuHandler, INiblHandler niblHandler, IDataBaseHandler dataBaseHandler, IAnimeRuleHandler animeRuleHandler, IDebugHandler debugHandler)
        {
            debugHandler.TraceMessage("Constructor Called.", DebugSource.CONSTRUCTOR, DebugType.ENTRY_EXIT);
            KitsuHandler = kitsuHandler;
            NiblHandler = niblHandler;
            DataBaseHandler = dataBaseHandler;
            AnimeRuleHandler = animeRuleHandler;
            DebugHandler = debugHandler;
            WeebFileNameParser = new WeebFileNameParser();
        }


        public async Task<JsonKitsuAnimeInfo> GetAnimeProfile(string id, int amountPerPage = 26, bool noCache = false)
        {
            DebugHandler.TraceMessage("GetAnimeProfile called", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage("Anime ID: " + id, DebugSource.TASK, DebugType.PARAMETERS);


            JsonKitsuAnimeInfo result = new JsonKitsuAnimeInfo();
           
            JObject db_result = await DataBaseHandler.GetJObject("anime", id);

            if (db_result.Count == 0 || noCache)
            {
                result = await KitsuHandler.GetFullAnime(id, amountPerPage);
                result = await AnimeLatestEpisode(result);

                await DataBaseHandler.StoreJObject("anime", result.ToJObject(), id);
                result.anime_stored = false;
                return result;
            }
            else
            {
                result = JsonConvert.DeserializeObject<JsonKitsuAnimeInfo>(db_result.ToString());
                if ((result.anime_info["attributes"].Value<string>("status") == "current" && long.Parse(result.updated) > (UtilityMethods.GetEpoch() - (30*60*1000))) || noCache)
                {
                    result = await AnimeLatestEpisode(result);
                    result.anime_stored = true;
                }


                if (result.anime_episodes_per_page != amountPerPage)
                {
                    DebugHandler.TraceMessage("RE-ARRANGING EPISODES PER PAGE BEFORE PARSING", DebugSource.TASK, DebugType.INFO);
                    JArray AllEpisodes = new JArray();
                    foreach (JArray current_page in result.anime_episodes)
                    {
                        foreach (JObject episode in current_page)
                        {
                            AllEpisodes.Add(episode);
                        }
                    }

                    JArray page = new JArray();
                    JArray pages = new JArray();
                    int episodeCount = 0;
                    foreach (JObject episode in AllEpisodes)
                    {
                        if (episodeCount >= amountPerPage)
                        {
                            pages.Add(page);
                            page = new JArray();
                            episodeCount = 0;
                        }

                        page.Add(episode);
                        episodeCount++;
                    }

                    result.anime_episodes_per_page = amountPerPage;
                    result.anime_episodes = pages;
                    await DataBaseHandler.UpdateJObject("anime", result.ToJObject(), id, true);
                }


            }


            return result;
        }



        public async Task<JsonKitsuAnimeInfo> GetAnimeEpisodes(string id, int amountPerPage = 26, bool noCache = false, bool cached = false)
        {
            JsonKitsuAnimeInfo result = null;
            JObject db_result = await DataBaseHandler.GetJObject("anime", id);
            
            if (db_result.Count == 0)
            {
                result = await GetAnimeProfile(id, amountPerPage);
            }
            else
            {
                result = JsonConvert.DeserializeObject<JsonKitsuAnimeInfo>(db_result.ToString());
            }

            DebugHandler.TraceMessage("Does anime contains files: " + result.anime_episodes[0].Value<JObject>(0).ContainsKey("files").ToString(), DebugSource.TASK, DebugType.INFO);
            DebugHandler.TraceMessage("OR Is anime current: " + result.anime_info["attributes"].Value<string>("status"), DebugSource.TASK, DebugType.INFO);
            DebugHandler.TraceMessage("AND Previous time updated:  " + long.Parse(result.updated).ToString(), DebugSource.TASK, DebugType.INFO);
            DebugHandler.TraceMessage("AND Current time minus 30 minutes:  " + (UtilityMethods.GetEpoch() - (30*60*1000)).ToString(), DebugSource.TASK, DebugType.INFO);
            DebugHandler.TraceMessage("ANDCurrent time:  " + UtilityMethods.GetEpoch().ToString(), DebugSource.TASK, DebugType.INFO);
            DebugHandler.TraceMessage("OR refresh cache:  " + noCache.ToString(), DebugSource.TASK, DebugType.INFO);



            if (result.anime_episodes_per_page != amountPerPage) 
            {

                DebugHandler.TraceMessage("RE-ARRANGING EPISODES PER PAGE BEFORE PARSING", DebugSource.TASK, DebugType.INFO);
                JArray AllEpisodes = new JArray();
                foreach (JArray current_page in result.anime_episodes)
                {
                    foreach (JObject episode in current_page)
                    {
                        AllEpisodes.Add(episode);
                    }
                }

                JArray page = new JArray();
                JArray pages = new JArray();
                int episodeCount = 0;
                foreach (JObject episode in AllEpisodes)
                {
                    if (episodeCount >= amountPerPage)
                    {
                        pages.Add(page);
                        page = new JArray();
                        episodeCount = 0;
                    }

                    page.Add(episode);
                    episodeCount++;
                }

                result.anime_episodes = pages;
                result.anime_episodes_per_page = amountPerPage;

            }

            if (cached && result.anime_episodes[0].Value<JObject>(0).ContainsKey("files"))
            {
                return result;
            }


            if (!result.anime_episodes[0].Value<JObject>(0).ContainsKey("files") || (result.anime_info["attributes"].Value<string>("status") == "current" && long.Parse(result.updated) > (UtilityMethods.GetEpoch() - (30*60*1000))) || noCache) {
                if (result != null)
                {
                    DebugHandler.TraceMessage("START GATHERING EPISODE FILES PER EPISODE", DebugSource.TASK, DebugType.INFO);

                    


                    List<string> animeTitles = new List<string>();
                    int seasonNumber = 1;
                    int previousEpisodeEnd = 0;
                    JObject niblResults = new JObject();

                    var loadDataTasks = new Task[]
                    { 
                        //parse all titles from the anime.
                        Task.Run(async () => animeTitles= await ParseTitles(result)),
                        //get season number from title or from kitsu
                        Task.Run(async () => seasonNumber = await GetSeasonFromAnime(result))
                    };

                    try
                    {
                        await Task.WhenAll(loadDataTasks);
                    }
                    catch (Exception ex)
                    {
                        DebugHandler.TraceMessage("FAILED RUNNING DATA GATHERING TASK (ParseTitles & GetSeasonFromAnime) ASYNC: " + ex.ToString(), DebugSource.TASK, DebugType.ERROR);
                        // handle exception
                    }

                    loadDataTasks = new Task[]
                    {
                    //get total amount of episodes from previous seasons (excluding current season).
                    Task.Run(async () => previousEpisodeEnd = await GetPreviousSeasonsTotalEpisodes(result, seasonNumber)),
                
                    //search nibl
                    Task.Run(async () => niblResults =  await NiblHandler.SearchNibl(animeTitles))
                    };
                    try
                    {
                        await Task.WhenAll(loadDataTasks);
                    }
                    catch (Exception ex)
                    {
                        DebugHandler.TraceMessage("FAILED RUNNING DATA GATHERING TASK (GetPreviousSeasonsTotalEpisodes & SearchNibl) ASYNC: " + ex.ToString(), DebugSource.TASK, DebugType.ERROR);
                        // handle exception
                    }


                    if (previousEpisodeEnd == -1)
                    {
                        seasonNumber = -1;
                    }

                    //check rules
                    niblResults = await AnimeRuleHandler.FilterAnimeRules(niblResults, id);


                    DebugHandler.TraceMessage("NIBL RESULTS AFTER RULES: " + niblResults.Value<JArray>("packs").Count.ToString(), DebugSource.TASK, DebugType.ERROR);

                    //parse nibl search results
                    Dictionary<string, Dictionary<int, List<JObject>>> parsedNiblResults = await ParseNiblSearchResults(niblResults, previousEpisodeEnd, seasonNumber);


                    //combine parsed nibl results with kistu episodes.


                    loadDataTasks = new Task[]
                    {
                        //compare results per episode from kitsu
                        Task.Run(async () => result.anime_episodes = await CombineAnimeEpisodesParsedNiblResults(result.anime_episodes, parsedNiblResults)),
                
                        //search nibl
                        Task.Run(async () => result.anime_bot_sources.Merge(await BotListPerResults(parsedNiblResults)))
                    };
                    try
                    {
                        await Task.WhenAll(loadDataTasks);
                    }
                    catch (Exception ex)
                    {
                        DebugHandler.TraceMessage("FAILED RUNNING DATA GATHERING TASK (CombineAnimeEpisodesParsedNiblResults & BotListPerResults) ASYNC: " + ex.ToString(), DebugSource.TASK, DebugType.ERROR);
                        // handle exception
                    }
                    result.anime_stored = false;

                    await DataBaseHandler.UpdateJObject("anime", result.ToJObject(), id, true);
                }
            }          

            return result;
        }

        public async Task<JsonCurrentlyAiring> GetCurrentlyAiring(bool nonFoundAnimes = false, int botId = 21, bool noCache = false, bool cached = false)
        {

            JObject db_result = await DataBaseHandler.GetJObject("airing", "currently_airing");

            bool newEntry = false;

            if (db_result.Count > 0)
            {
                if (long.Parse(db_result.Value<string>("updated")) > (UtilityMethods.GetEpoch() - (30*60*1000)) && !noCache || cached)
                {
                    JsonCurrentlyAiring toreturn = JsonConvert.DeserializeObject<JsonCurrentlyAiring>(db_result.ToString());
                    return toreturn;
                }
            } else {
                newEntry = true;
            }

            DebugHandler.TraceMessage("GetCurrentlyAiring called", DebugSource.TASK, DebugType.ENTRY_EXIT);

            JObject result = await NiblHandler.GetLatestFiles(botId.ToString());         


            Dictionary<int, JObject> latestAiringKitsuUnordered = new Dictionary<int, JObject>();
            List<JObject> latestAiringKitsu = new List<JObject>();
            List<JObject> nonLatestAiringKitsu = new List<JObject>();


            JArray array = result.Value<JArray>("packs");
            List<string> animetitles = new List<string>();


            List<Task<Dictionary<int, JObject>>> parralel = new List<Task<Dictionary<int, JObject>>>();

            int i = 0;

            Dictionary<int, int> TaskIds = new Dictionary<int, int>();

            foreach (JObject pack in array) {
                if (pack.ContainsKey("AccurateMainAnimeTitle"))
                {
                    string title = pack.Value<string>("AccurateMainAnimeTitle");
                    if (!animetitles.Contains(title))
                    {
                        Dictionary<int, JObject> di = await CurrentlyAiringAsync(i, title);

                        Task<Dictionary<int, JObject>> newT = Task.Factory.StartNew(async delegate
                        {
                            return await CurrentlyAiringAsync(i, title);
                        }).Unwrap();


                        TaskIds.Add(newT.Id, i);
                        parralel.Add(newT);
                        i++;
                        animetitles.Add(title);
                    }
                }
            }


            try
            {
                await Task.WhenAll(parralel.ToArray());

                foreach(Task<Dictionary<int, JObject>> t in parralel){
                    Dictionary<int, JObject> r = t.Result;
                    int index = TaskIds[t.Id];
                    KeyValuePair<int, JObject> kp = r.First();
                    latestAiringKitsuUnordered.Add(index, kp.Value);
                }
            }
            catch (Exception ex)
            {
                DebugHandler.TraceMessage("FAILED RUNNING DATA GATHERING TASK (CURRENTLY AIRING) ASYNC: " + ex.ToString(), DebugSource.TASK, DebugType.ERROR);
                // handle exception
            }


            for (int a = 0; a < latestAiringKitsuUnordered.Count; a++)
            {
                try
                {
                    if (latestAiringKitsuUnordered[a].Count > 0)
                    {
                        latestAiringKitsu.Add(latestAiringKitsuUnordered[a]);
                    }
                }
                catch (Exception e)
                {
                    DebugHandler.TraceMessage("FAILED TO ADD AIRING ANIME ORDERD AT INDEX: " + a.ToString() + ", ERROR: " + e.ToString(), DebugSource.TASK, DebugType.WARNING);
                }
            }


            JObject resultCurrentlyAiringKitsu = new JObject();

            if (nonFoundAnimes)
            {
                resultCurrentlyAiringKitsu["airing_all"] = await KitsuHandler.GetCurrentlyAiring();
            }

            resultCurrentlyAiringKitsu["airing_ordered"] = JArray.FromObject(latestAiringKitsu);



            JsonCurrentlyAiring airing = new JsonCurrentlyAiring
            {
                result = resultCurrentlyAiringKitsu
            };

            if (newEntry) {
                await DataBaseHandler.StoreJObject("airing", JObject.FromObject(airing), "currently_airing");
            } else
            {
                await DataBaseHandler.UpdateJObject("airing", JObject.FromObject(airing), "currently_airing", true); 
            }

            return airing;

        }


        private async Task<int> GetSeasonFromAnime(JsonKitsuAnimeInfo info)
        {

            DebugHandler.TraceMessage("GetSeasonFromAnime called", DebugSource.TASK, DebugType.ENTRY_EXIT);
            // DebugHandler.TraceMessage("Anime Info: ", DebugSource.TASK, DebugType.PARAMETERS);
            // DebugHandler.TraceMessage(info.ToJson(), DebugSource.TASK, DebugType.PARAMETERS);

            int seasonNumber = 1;

            if (info.anime_episodes.Count > 0)
            {
                if (info.anime_episodes[0][0].Value<JObject>("attributes").ContainsKey("seasonNumber"))
                {
                    seasonNumber = info.anime_episodes[0][0]["attributes"].Value<int>("seasonNumber");
                }

                await Task.Run(() =>
                {
                    Dictionary<string, string> parsedTitle = WeebFileNameParser.ParseFullString(info.anime_info["attributes"].Value<string>("canonicalTitle"));

                    if (parsedTitle.ContainsKey("Season"))
                    {
                        int parsedSeasonNumberFromTitle = -1;
                        if (int.TryParse(parsedTitle["Season"], out parsedSeasonNumberFromTitle))
                        {
                            if (parsedSeasonNumberFromTitle > seasonNumber)
                            {
                                seasonNumber = parsedSeasonNumberFromTitle;
                            }
                        }
                    }
                });
            }
            

            return seasonNumber;
        }

        private async Task<int> GetPreviousSeasonsTotalEpisodes(JsonKitsuAnimeInfo info, int seasonNumber)
        {

            DebugHandler.TraceMessage("GetPreviousSeasonsTotalEpisodes called", DebugSource.TASK, DebugType.ENTRY_EXIT);
            //DebugHandler.TraceMessage("Anime Info: ", DebugSource.TASK, DebugType.PARAMETERS);
           // DebugHandler.TraceMessage(info.ToJson(), DebugSource.TASK, DebugType.PARAMETERS);
            DebugHandler.TraceMessage("SeasonNumber: " + seasonNumber, DebugSource.TASK, DebugType.PARAMETERS);

            int tempSeasonNumber = seasonNumber;

            int previousEpisodeEnd = 0;

            JArray tempRelated = info.anime_relations;

            bool has_prequal = false;
            await Task.Run(async () =>
            {
                while (tempSeasonNumber > 1)
                {
                    foreach (JObject relatedAnime in tempRelated)
                    {
                        if (relatedAnime.Value<string>("role") == "prequel")
                        {

                            previousEpisodeEnd += relatedAnime["attributes"].Value<int>("episodeCount");

                            if ((tempSeasonNumber - 1) > 1)
                            {
                                tempRelated = await KitsuHandler.GetRelations(relatedAnime.Value<string>("id"));
                            }
                            has_prequal = true;
                            break;
                        }
                    }
                    tempSeasonNumber--;
                }
            });

            if (!has_prequal)
            {

                DebugHandler.TraceMessage("HAS NO PREQUALS SO IGNORE SEASON NUMBER", DebugSource.TASK, DebugType.PARAMETERS);
                previousEpisodeEnd = -1;
            }
            else
            {
                DebugHandler.TraceMessage("HAS PREQUAL, PREQUAL STOP EPISODES: " + previousEpisodeEnd, DebugSource.TASK, DebugType.PARAMETERS);

            }

            return previousEpisodeEnd;
        }

        private async Task<List<string>> ParseTitles(JsonKitsuAnimeInfo info)
        {
            DebugHandler.TraceMessage("ParseTitles called", DebugSource.TASK, DebugType.ENTRY_EXIT);
  //          DebugHandler.TraceMessage("Anime Info: ", DebugSource.TASK, DebugType.PARAMETERS);
//            DebugHandler.TraceMessage(info.ToJson(), DebugSource.TASK, DebugType.PARAMETERS);

            List<string> animeTitles = new List<string>();

            JObject titles = info.anime_info["attributes"].Value<JObject>("titles");

            await Task.Run(() =>
            {
                foreach (KeyValuePair<string, JToken> title in titles)
                {

                    if (!animeTitles.Contains(title.Value.ToString()))
                    {
                        Dictionary<string, string> parsed = WeebFileNameParser.ParseFullString(title.Value.ToString());
                        parsed["MainAnimeTitle"] = UtilityMethods.RemoveSpecialCharacters(parsed["MainAnimeTitle"]).Trim();
                        parsed["SubAnimeTitle"] = UtilityMethods.RemoveSpecialCharacters(parsed["SubAnimeTitle"]).Trim();

                        DebugHandler.TraceMessage("1: Removing special chars from title: " + title.Value.ToString().ToLower(), DebugSource.TASK, DebugType.INFO);
                        string titleparsed = UtilityMethods.RemoveSpecialCharacters(title.Value.ToString().ToLower()).Trim();

                        if (titleparsed.Length > 2 && title.Key != "ja_jp")                        {


                            bool parsedSeason = false;
                            if (parsed.ContainsKey("Season"))
                            {
                                if (int.Parse(parsed["Season"]) > 1)
                                {
                                    if (!animeTitles.Contains(parsed["MainAnimeTitle"] + " " + UtilityMethods.IntToRoman(int.Parse(parsed["Season"]))))
                                    {
                                        DebugHandler.TraceMessage("Added Title: " + parsed["MainAnimeTitle"] + " " + UtilityMethods.IntToRoman(int.Parse(parsed["Season"])), DebugSource.TASK, DebugType.INFO);
                                        animeTitles.Add(parsed["MainAnimeTitle"] + " " + UtilityMethods.IntToRoman(int.Parse(parsed["Season"])));
                                    }

                                    if (!animeTitles.Contains(parsed["MainAnimeTitle"] + " s" + parsed["Season"]))
                                    {
                                        DebugHandler.TraceMessage("Added Title: " + parsed["MainAnimeTitle"] + " s" + parsed["Season"], DebugSource.TASK, DebugType.INFO);
                                        animeTitles.Add(parsed["MainAnimeTitle"] + " s" + parsed["Season"]);
                                    }

                                    if (!animeTitles.Contains(parsed["MainAnimeTitle"] + " season " + parsed["Season"]))
                                    {
                                        DebugHandler.TraceMessage("Added Title: " + parsed["MainAnimeTitle"] + " season " + parsed["Season"], DebugSource.TASK, DebugType.INFO);
                                        animeTitles.Add(parsed["MainAnimeTitle"] + " season " + parsed["Season"]);
                                    }
                                    parsedSeason = true;
                                }
                            }

                            if (!parsedSeason)
                            {
                                if (parsed.ContainsKey("SubAnimeTitle"))
                                {
                                    titleparsed = parsed["MainAnimeTitle"] + parsed["SubAnimeTitle"];

                                    if (!animeTitles.Contains(parsed["MainAnimeTitle"]))
                                    {
                                        DebugHandler.TraceMessage("Added Title: " + parsed["MainAnimeTitle"], DebugSource.TASK, DebugType.ENTRY_EXIT);
                                        animeTitles.Add(parsed["MainAnimeTitle"]);
                                    }

                                    if (!animeTitles.Contains(titleparsed) && parsed["SubAnimeTitle"].Length > 2) // last part of anime title must be longer than 2 chars to be significant.
                                    {
                                        DebugHandler.TraceMessage("Added Title: " + titleparsed, DebugSource.TASK, DebugType.ENTRY_EXIT);
                                        animeTitles.Add(titleparsed);
                                    }

                                }
                            }
                            else
                            {

                            }
                            animeTitles.Add(title.Value.ToString());

                        }
                    }
                   
                }

                if (info.anime_info["attributes"].Value<JArray>("abbreviatedTitles") != null)
                {
                    foreach (string title in info.anime_info["attributes"].Value<JArray>("abbreviatedTitles"))
                    {
                        if (!animeTitles.Contains(title))
                        {

                            Dictionary<string, string> parsed = WeebFileNameParser.ParseFullString(title);

                            DebugHandler.TraceMessage("2: Removing special chars from title: " + title.ToLower(), DebugSource.TASK, DebugType.INFO);
                            string titleparsed = UtilityMethods.RemoveSpecialCharacters(title.ToLower()).Trim();

                            parsed["MainAnimeTitle"] = UtilityMethods.RemoveSpecialCharacters(parsed["MainAnimeTitle"]).Trim();
                            parsed["SubAnimeTitle"] = UtilityMethods.RemoveSpecialCharacters(parsed["SubAnimeTitle"]).Trim();

                            if (titleparsed.Length > 2)
                            {

                                bool parsedSeason = false;

                                if (parsed.ContainsKey("Season"))
                                {
                                    if (int.Parse(parsed["Season"]) > 1)
                                    {
                                        if (!animeTitles.Contains(parsed["MainAnimeTitle"] + " " + UtilityMethods.IntToRoman(int.Parse(parsed["Season"]))))
                                        {
                                            DebugHandler.TraceMessage("Added Title: " + parsed["MainAnimeTitle"] + " " + UtilityMethods.IntToRoman(int.Parse(parsed["Season"])), DebugSource.TASK, DebugType.INFO);
                                            animeTitles.Add(parsed["MainAnimeTitle"] + " " + UtilityMethods.IntToRoman(int.Parse(parsed["Season"])));
                                        }

                                        if (!animeTitles.Contains(parsed["MainAnimeTitle"] + " s" + parsed["Season"]))
                                        {
                                            DebugHandler.TraceMessage("Added Title: " + parsed["MainAnimeTitle"] + " s" + parsed["Season"], DebugSource.TASK, DebugType.ENTRY_EXIT);
                                            animeTitles.Add(parsed["MainAnimeTitle"] + " s" + parsed["Season"]);
                                        }

                                        if (!animeTitles.Contains(parsed["MainAnimeTitle"] + " season " + parsed["Season"]))
                                        {
                                            DebugHandler.TraceMessage("Added Title: " + parsed["MainAnimeTitle"] + " season " + parsed["Season"], DebugSource.TASK, DebugType.ENTRY_EXIT);
                                            animeTitles.Add(parsed["MainAnimeTitle"] + " season " + parsed["Season"]);
                                        }
                                        parsedSeason = true;
                                    }
                                }


                                if (!parsedSeason)
                                {
                                    if (parsed.ContainsKey("SubAnimeTitle"))
                                    {
                                        titleparsed = parsed["MainAnimeTitle"] + parsed["SubAnimeTitle"];


                                        if (!animeTitles.Contains(parsed["MainAnimeTitle"]))
                                        {
                                            DebugHandler.TraceMessage("Added Title: " + parsed["MainAnimeTitle"], DebugSource.TASK, DebugType.ENTRY_EXIT);
                                            animeTitles.Add(parsed["MainAnimeTitle"]);
                                        }

                                        if (!animeTitles.Contains(titleparsed) && parsed["SubAnimeTitle"].Length > 2) // last part of anime title must be longer than 2 chars to be significant.
                                        {
                                            DebugHandler.TraceMessage("Added Title: " + titleparsed, DebugSource.TASK, DebugType.ENTRY_EXIT);
                                            animeTitles.Add(titleparsed);
                                        }
                                    }

                                }

                                animeTitles.Add(title);
                            }

                        }
                       
                    }
                }
            });          

            return animeTitles;
        }

       
        private async Task<Dictionary<string, Dictionary<int, List<JObject>>>> ParseNiblSearchResults(JObject niblResults, int previousEpisodeEnd, int seasonNumber)
        {

            DebugHandler.TraceMessage("ParseNiblSearchResults called", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage("Anime Info: ", DebugSource.TASK, DebugType.PARAMETERS);
           // DebugHandler.TraceMessage(niblResults.ToString(Newtonsoft.Json.Formatting.None), DebugSource.TASK, DebugType.PARAMETERS);
            DebugHandler.TraceMessage("Anime Previous Episodes: " + previousEpisodeEnd, DebugSource.TASK, DebugType.PARAMETERS);
            DebugHandler.TraceMessage("Anime SeasonNumber: " + seasonNumber, DebugSource.TASK, DebugType.PARAMETERS);

            Dictionary<string, Dictionary<int, List<JObject>>> resolutions = new Dictionary<string, Dictionary<int, List<JObject>>>()
            {
                { "UNKNOWN", new Dictionary<int, List<JObject>>()},
                { "480P", new Dictionary<int, List<JObject>>()},
                { "720P", new Dictionary<int, List<JObject>>()},
                { "1080P", new Dictionary<int, List<JObject>>()}
            };

            await Task.Run(() => {
                foreach (JObject pack in niblResults.Value<JArray>("packs"))
                {
                    string resolution = "UNKNOWN";

                    bool correctSeason = false;
                    int seasonFromFile = -1;
                    if (pack.ContainsKey("Season"))
                    {

                        if (int.TryParse(pack.Value<string>("Season"), out seasonFromFile))
                        {
                            if (seasonFromFile == seasonNumber)
                            {
                                correctSeason = true;
                            }
                        }
                    }

                    if (correctSeason || seasonNumber < 0)
                    {
                        if (pack.ContainsKey("Video_Resolution"))
                        {
                            resolution = pack.Value<string>("Video_Resolution");
                        }

                        int episodeValue = -1;
                        if (!resolutions.ContainsKey(resolution))
                        {
                            if (pack.ContainsKey("Episode"))
                            {
                                if (int.TryParse(pack.Value<string>("Episode"), out episodeValue))
                                {
                                    resolutions.Add(resolution, new Dictionary<int, List<JObject>>() { { episodeValue, new List<JObject>() { pack } } });
                                }
                                else
                                {
                                    resolutions.Add(resolution, new Dictionary<int, List<JObject>>() { { episodeValue, new List<JObject>() { pack } } });
                                }
                            }
                            else
                            {
                                resolutions.Add(resolution, new Dictionary<int, List<JObject>>() { { episodeValue, new List<JObject>() { pack } } });
                            }
                        }
                        else
                        {
                            if (pack.ContainsKey("Episode"))
                            {
                                if (int.TryParse(pack.Value<string>("Episode"), out episodeValue))
                                {
                                    if (resolutions[resolution].ContainsKey(episodeValue))
                                    {
                                        resolutions[resolution][episodeValue].Add(pack);
                                    }
                                    else
                                    {
                                        resolutions[resolution].Add(episodeValue, new List<JObject>() { pack });
                                    }
                                }
                                else
                                {
                                    if (resolutions[resolution].ContainsKey(episodeValue))
                                    {
                                        resolutions[resolution][episodeValue].Add(pack);
                                    }
                                    else
                                    {
                                        resolutions[resolution].Add(episodeValue, new List<JObject>() { pack });
                                    }
                                }
                            }
                            else
                            {
                                if (resolutions[resolution].ContainsKey(episodeValue))
                                {
                                    resolutions[resolution][episodeValue].Add(pack);
                                }
                                else
                                {
                                    resolutions[resolution].Add(episodeValue, new List<JObject>() { pack });
                                }
                            }
                        }
                    }
                    else // not correct season
                    {
                        if (previousEpisodeEnd > 0)
                        {
                            if (pack.ContainsKey("Video_Resolution"))
                            {
                                resolution = pack.Value<string>("Video_Resolution");
                            }

                            int episodeValue = -1;
                            if (!resolutions.ContainsKey(resolution))
                            {
                                if (pack.ContainsKey("Episode"))
                                {
                                    if (int.TryParse(pack.Value<string>("Episode"), out episodeValue))
                                    {
                                        if (episodeValue > previousEpisodeEnd)
                                        {
                                            resolutions.Add(resolution, new Dictionary<int, List<JObject>>() { { (episodeValue - previousEpisodeEnd), new List<JObject>() { pack } } });
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (pack.ContainsKey("Episode"))
                                {
                                    if (int.TryParse(pack.Value<string>("Episode"), out episodeValue))
                                    {
                                        if (episodeValue > previousEpisodeEnd)
                                        {
                                            if (resolutions[resolution].ContainsKey(episodeValue - previousEpisodeEnd))
                                            {
                                                resolutions[resolution][(episodeValue - previousEpisodeEnd)].Add(pack);
                                            }
                                            else
                                            {
                                                resolutions[resolution].Add((episodeValue - previousEpisodeEnd), new List<JObject>() { pack });
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            });

            return resolutions;

        }

        private async Task<JArray> BotListPerResults(Dictionary<string, Dictionary<int, List<JObject>>> parsed)
        {

            List<string> listWithBots = new List<string>();
            await Task.Run(() =>
            {
                foreach (KeyValuePair<string, Dictionary<int, List<JObject>>> resolution in parsed)
                {
                    foreach (KeyValuePair<int, List<JObject>> episode in resolution.Value)
                    {
                        foreach (JObject pack in episode.Value)
                        {
                            if (!listWithBots.Contains(pack.Value<string>("BotName")))
                            {
                                listWithBots.Add(pack.Value<string>("BotName"));
                            }
                        }
                    }
                }
            });           
            return JArray.FromObject(listWithBots);
        }

        private async Task<JArray> CombineAnimeEpisodesParsedNiblResults(JArray result, Dictionary<string, Dictionary<int, List<JObject>>> parsedNiblResult)
        {
            DebugHandler.TraceMessage("CombineAnimeEpisodesParsedNiblResults called", DebugSource.TASK, DebugType.ENTRY_EXIT);
           // DebugHandler.TraceMessage("Anime Info: ", DebugSource.TASK, DebugType.PARAMETERS);
           // DebugHandler.TraceMessage(result.ToJson(), DebugSource.TASK, DebugType.PARAMETERS);


            JArray new_anime_episodes = result;

            List<int> indexToRemove = new List<int>();

            await Task.Run(() =>
            {
                int pageindex = 0;
                foreach (JArray page in new_anime_episodes)
                {
                    int index = 0;
                    foreach (JObject episode in page)
                    {
                        int episodeNumber = episode["attributes"].Value<int>("number");

                        JObject fileList = new JObject();

                        bool hasvalues = false;
                        foreach (KeyValuePair<string, Dictionary<int, List<JObject>>> resolution in parsedNiblResult)
                        {
                            if (resolution.Value.ContainsKey(episodeNumber))
                            {
                                List<JObject> files = resolution.Value[episodeNumber];

                                JArray filesArray = JArray.FromObject(files);

                                fileList[resolution.Key] = filesArray;

                                if (filesArray.Count > 0)
                                {
                                    hasvalues = true;
                                }
                            }

                        }

                        if (hasvalues)
                        {
                            new_anime_episodes[pageindex][index]["files"] = fileList;
                        }
                        else
                        {
                            new_anime_episodes[pageindex][index]["files"] = false;
                        }

                        index++;
                    }

                    pageindex++;
                }
                
            });

            return new_anime_episodes;          
        }

        private async Task<JsonKitsuAnimeInfo> AnimeLatestEpisode(JsonKitsuAnimeInfo result)
        {
            DebugHandler.TraceMessage("LatestEpisode called", DebugSource.TASK, DebugType.ENTRY_EXIT);
           
            if (result.anime_info.Value<JObject>("attributes").ContainsKey("episodeCount"))
            {
                if (result.anime_info["attributes"]["episodeCount"].Type != JTokenType.Null)
                {
                    result.anime_total_episodes = result.anime_info["attributes"].Value<int>("episodeCount");
                    result.anime_total_episode_pages = (int)Math.Ceiling(((double)((result.anime_total_episodes) / (double)20))) - 1;
                }
            }

            DebugHandler.TraceMessage("Airing: " + result.anime_id + " = " + result.anime_info["attributes"].Value<string>("status"), DebugSource.TASK, DebugType.INFO);

            if (result.anime_info["attributes"].Value<string>("status") == "current")
            {
                List<string> animeTitles = await ParseTitles(result);
                int seasonNumber = -1;
                int previousEpisodeEnd = -1;
                JObject niblResults = new JObject();

                try
                {
                    seasonNumber = await GetSeasonFromAnime(result);
                    DebugHandler.TraceMessage("FINISHED GET SEASING NUMBER: " + seasonNumber.ToString(), DebugSource.TASK, DebugType.INFO);
                }
                catch (Exception ex)
                {
                    DebugHandler.TraceMessage("FAILED RUNNING DATA GATHERING TASK (GetSeasonFromAnime) ASYNC: " + ex.ToString(), DebugSource.TASK, DebugType.ERROR);
                    // handle exception
                }

                previousEpisodeEnd = await GetPreviousSeasonsTotalEpisodes(result, seasonNumber);

                int currentLatestEpisode = 0;
                JObject resultNibl = new JObject();

                foreach (string title in animeTitles)
                {
                    int episodeNumber = await NiblHandler.GetLatestEpisode(title);
                    if (episodeNumber > currentLatestEpisode)
                    {
                        currentLatestEpisode = episodeNumber;
                    }
                }
                DebugHandler.TraceMessage("FINISHED GETTING LATEST EPISODE NUMBER FROM NIBL " + currentLatestEpisode.ToString(), DebugSource.TASK, DebugType.INFO);


                resultNibl = await NiblHandler.SearchNibl(animeTitles, currentLatestEpisode);
                DebugHandler.TraceMessage("Nibl result latest episode: " + resultNibl.ToString(), DebugSource.TASK, DebugType.INFO);

                DebugHandler.TraceMessage("Nibl result latest episode done: " + resultNibl.ToString(), DebugSource.TASK, DebugType.INFO);
                Dictionary<string, Dictionary<int, List<JObject>>> parsedNibl =  await ParseNiblSearchResults(resultNibl, previousEpisodeEnd, seasonNumber);

                JObject files = new JObject
                {
                    ["files"] = JObject.FromObject(parsedNibl)
                };

                JObject latestKitsuEpisode = await KitsuHandler.GetEpisode(result.anime_id, currentLatestEpisode);

                latestKitsuEpisode.Merge(files);
                result.anime_latest_episode = latestKitsuEpisode;

                result.anime_total_episodes = currentLatestEpisode;
                result.anime_total_episode_pages = (int)Math.Ceiling(((double)((currentLatestEpisode) / (double)20) )) - 1;

            }

            return result;
        }

        private async Task<Dictionary<int, JObject>> CurrentlyAiringAsync(int index, string title){

            Dictionary<int, JObject> latestAiringKitsuUnordered = new Dictionary<int, JObject>();
            DebugHandler.TraceMessage("Searching anime at index: " + index + ", anime title: " + title, DebugSource.TASK, DebugType.INFO);

            JsonKistuSearchResult searchresult = await KitsuHandler.SearchAnime(title, new Dictionary<string, string>() { { "status", "current" }, { "subtype", "TV" } }, 0, 1);
            try
            {
                DebugHandler.TraceMessage("Adding anime at index: " + index + ", anime title: " + searchresult.result.Value<JObject>(0)["attributes"].Value<string>("canonicalTitle"), DebugSource.TASK, DebugType.INFO);

                latestAiringKitsuUnordered.Add(index, searchresult.result.Value<JObject>(0));
            }
            catch (Exception e)
            {
                latestAiringKitsuUnordered.Add(index, new JObject());

                DebugHandler.TraceMessage(e.ToString(), DebugSource.TASK, DebugType.ERROR);
            }

            return latestAiringKitsuUnordered;
        }

    }
}
