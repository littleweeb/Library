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
        Task<JsonKitsuAnimeInfo> GetAnimeProfile(string id);
        Task<JsonKitsuAnimeInfo> GetAnimeEpisodes(string id, int page, int pages = 1);
        Task<JsonCurrentlyAiring> GetCurrentlyAiring(double likeness = 0.5, bool nonFoundAnimes = false, int botId = 21);
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


        public async Task<JsonKitsuAnimeInfo> GetAnimeProfile(string id)
        {
            DebugHandler.TraceMessage("GetAnimeProfile called", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage("Anime ID: " + id, DebugSource.TASK, DebugType.PARAMETERS);


            JsonKitsuAnimeInfo result = new JsonKitsuAnimeInfo();
           
            JObject db_result = await DataBaseHandler.GetJObject("anime", id);
            if (db_result.Count == 0)
            {
                result = await KitsuHandler.GetFullAnime(id);
                result = await AnimeLatestEpisode(result);

                await DataBaseHandler.StoreJObject("anime", result.ToJObject(), id);
                return result;
            }

            result = JsonConvert.DeserializeObject<JsonKitsuAnimeInfo>(db_result.ToString());

            return result;
        }



        public async Task<JsonKitsuAnimeInfo> GetAnimeEpisodes(string id, int page, int pages = 1)
        {
            JsonKitsuAnimeInfo result = null;
            JObject db_result = await DataBaseHandler.GetJObject("anime", id);
            
            if (db_result.Count == 0)
            {
                result = await GetAnimeProfile(id);
            }
            else
            {
                result = JsonConvert.DeserializeObject<JsonKitsuAnimeInfo>(db_result.ToString());
            }

                                 
            if (result != null) {
                JArray kitsuEpisodes = await KitsuHandler.GetEpisodes(id, page, pages);
                result.anime_episodes = kitsuEpisodes;

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
                    Task.Run(async () => niblResults = await SearchNibl(animeTitles, page + 1, pages))
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

                //parse nibl search results
                Dictionary<string, Dictionary<int, List<JObject>>> parsedNiblResults = await ParseNiblSearchResults(niblResults, previousEpisodeEnd, seasonNumber);

                
                //combine parsed nibl results with kistu episodes.


                   loadDataTasks = new Task[]
                   {
                        //compare results per episode from kitsu
                        Task.Run(async () =>  result.anime_episodes = await CombineAnimeEpisodesParsedNiblResults(result, parsedNiblResults)),
                
                        //search nibl
                        Task.Run(async () => result.anime_bot_sources = await BotListPerResults(parsedNiblResults))
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
            }

            return result;
        }

        public async Task<JsonCurrentlyAiring> GetCurrentlyAiring(double likeness = 0.5, bool nonFoundAnimes = false, int botId = 21)
        {

         
            JArray latestAiring = await KitsuHandler.GetCurrentlyAiring();

            Dictionary<int, List<string>> airingAnimeTitles = new Dictionary<int, List<string>>();
            int i = 0;

            foreach (JObject anime in latestAiring)
            {
                List<string> titlelist = new List<string>();

                JObject attributes = anime.Value<JObject>("attributes");
                JObject titles = attributes.Value<JObject>("titles");
                if (attributes["abbreviatedTitles"] != null)
                {
                    try
                    {
                        JArray abbreviatedTitles = attributes.Value<JArray>("abbreviatedTitles");
                        string[] abbreviatedTitlesStrings = abbreviatedTitles.ToObject<string[]>();


                        foreach (string title in abbreviatedTitlesStrings)
                        {
                            titlelist.Add(title);
                        }

                    }
                    catch
                    {

                    }
                }

                if (titles.Value<string>("en") != null)
                {
                    titlelist.Add(titles.Value<string>("en"));
                }

                if (titles.Value<string>("en_jp") != null)
                {
                    titlelist.Add(titles.Value<string>("en_jp"));
                }

                if (titles.Value<string>("en_us") != null)
                {
                    titlelist.Add(titles.Value<string>("en_us"));
                }

                airingAnimeTitles.Add(i, titlelist);

                i++;
            }


            JObject listWithAiringAnime = JObject.FromObject(airingAnimeTitles);

            List<JObject> latestAiringKitsu = new List<JObject>();
            List<JObject> nonLatestAiringKitsu = new List<JObject>();

            JObject result = await NiblHandler.GetLatestFiles(botId.ToString());

            JArray array = result.Value<JArray>("packs");
            List<string> animeIdsAdded = new List<string>();

            foreach (JObject pack in array) {
                if (pack["Video_Resolution"] != null) {
                    if (pack.Value<string>("Video_Resolution") == "720P")
                    {

                        string title_compare = pack.Value<string>("MainAnimeTitle").ToLower();

                        Dictionary<int, List<double>> comparisonValues = new Dictionary<int, List<double>>();
                        foreach (KeyValuePair<int, List<string>> keyValuePair in airingAnimeTitles)
                        {
                            List<double> comparisonValue = new List<double>();
                            int x = 0;
                            foreach (string title in keyValuePair.Value)
                            {
                                string title_lower = title.ToLower();
                                double comparison_value = UtilityMethods.CalculateSimilarity(title_lower, title_compare);


                                comparisonValue.Add(comparison_value);
                                x++;
                            }
                            comparisonValues.Add(keyValuePair.Key, comparisonValue);
                          
                        }

                        double max = 0;
                        int maxKey = -1;
                        foreach (KeyValuePair<int, List<double>> resultComparison in comparisonValues)
                        {


                            foreach (double resultComparison2 in resultComparison.Value)
                            {
                                if (resultComparison2 > max)
                                {
                                    max = resultComparison2;
                                    maxKey = resultComparison.Key;
                                }
                            }
                              
                        }

                        JObject currentlyairing = latestAiring[maxKey].ToObject<JObject>();
                        if (!animeIdsAdded.Contains(currentlyairing.Value<string>("id")) && max > likeness)
                        {

                            currentlyairing.Add("latest_episode", pack);
                            latestAiringKitsu.Add(currentlyairing);

                            animeIdsAdded.Add(currentlyairing.Value<string>("id"));
                        }
                    }

                }
                else
                {
                    string title_compare = pack.Value<string>("MainAnimeTitle").ToLower();

                    Dictionary<int, List<double>> comparisonValues = new Dictionary<int, List<double>>();
                    foreach (KeyValuePair<int, List<string>> keyValuePair in airingAnimeTitles)
                    {
                        List<double> comparisonValue = new List<double>();
                        int x = 0;
                        foreach (string title in keyValuePair.Value)
                        {
                            string title_lower = title.ToLower();
                            double comparison_value = UtilityMethods.CalculateSimilarity(title_lower, title_compare);

                            //Console.WriteLine("Compare Handler, comparing: " + title_compare + ", " + title_lower + ", result = " + comparison_value);

                            comparisonValue.Add(comparison_value);
                            x++;
                        }
                        comparisonValues.Add(keyValuePair.Key, comparisonValue);

                    }

                    double max = 0;
                    int maxKey = -1;
                    foreach (KeyValuePair<int, List<double>> resultComparison in comparisonValues)
                    {


                        foreach (double resultComparison2 in resultComparison.Value)
                        {
                            if (resultComparison2 > max)
                            {
                                max = resultComparison2;
                                maxKey = resultComparison.Key;
                            }
                        }

                    }

                    JObject currentlyairing = latestAiring[maxKey].ToObject<JObject>();
                    if (!animeIdsAdded.Contains(currentlyairing.Value<string>("id")) && max > likeness)
                    {

                        currentlyairing.Add("latest_episode", pack);
                        latestAiringKitsu.Add(currentlyairing);

                        animeIdsAdded.Add(currentlyairing.Value<string>("id"));
                    }
                }
            }
            JObject resultCurrentlyAiringKitsu = null;
            if (nonFoundAnimes)
            {

                foreach (JObject anime in latestAiring)
                {
                    if (!animeIdsAdded.Contains(anime.Value<string>("id")))
                    {
                        nonLatestAiringKitsu.Add(anime);
                    }
                }
                resultCurrentlyAiringKitsu = new JObject { ["airing_ordered"] = JToken.FromObject(latestAiringKitsu), ["airing_non_ordered"] = JToken.FromObject(nonLatestAiringKitsu) };
            }
            else
            {
                resultCurrentlyAiringKitsu = new JObject { ["airing_ordered"] = JToken.FromObject(latestAiringKitsu) };
            }

         

            JsonCurrentlyAiring airing = new JsonCurrentlyAiring
            {
                result = resultCurrentlyAiringKitsu
            };

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
                seasonNumber = info.anime_episodes[0]["attributes"].Value<int>("seasonNumber");

                await Task.Run(() =>
                {
                    Dictionary<string, string> parsedTitle = WeebFileNameParser.ParseFullString(info.anime_info["data"][0]["attributes"].Value<string>("canonicalTitle"));

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
                DebugHandler.TraceMessage("HAS PREQUAL", DebugSource.TASK, DebugType.PARAMETERS);

            }

            return previousEpisodeEnd;
        }

        private async Task<List<string>> ParseTitles(JsonKitsuAnimeInfo info)
        {
            DebugHandler.TraceMessage("ParseTitles called", DebugSource.TASK, DebugType.ENTRY_EXIT);
  //          DebugHandler.TraceMessage("Anime Info: ", DebugSource.TASK, DebugType.PARAMETERS);
//            DebugHandler.TraceMessage(info.ToJson(), DebugSource.TASK, DebugType.PARAMETERS);

            List<string> animeTitles = new List<string>();

            JObject titles = info.anime_info["data"][0]["attributes"].Value<JObject>("titles");

            await Task.Run(() =>
            {
                foreach (KeyValuePair<string, JToken> title in titles)
                {
                    if (!animeTitles.Contains(title.Value.ToString().ToLower()))
                    {
                        animeTitles.Add(title.Value.ToString().ToLower());
                    }
                }

                if (info.anime_info["data"][0]["attributes"].Value<JArray>("abbreviatedTitles") != null)
                {
                    foreach (string title in info.anime_info["data"][0]["attributes"].Value<JArray>("abbreviatedTitles"))
                    {
                        if (!animeTitles.Contains(title.ToLower()))
                        {
                            animeTitles.Add(title.ToLower());
                        }
                    }
                }
            });          

            return animeTitles;
        }

        private async Task<JObject> SearchNibl(List<string> titles, int page, int pages)
        {

            DebugHandler.TraceMessage("SearchNibl called", DebugSource.TASK, DebugType.ENTRY_EXIT);

            JObject niblResults = new JObject();
            await Task.Run(async () =>
            {
                foreach (string title in titles)
                {
                    Dictionary<string, string> parsed = WeebFileNameParser.ParseFullString(title);

                    string searchQuery = parsed["MainAnimeTitle"];

                    if (parsed["SubAnimeTitle"].Length > 0)
                    {
                        searchQuery += " " + parsed["SubAnimeTitle"];
                    }

                    JObject nibl_result = await NiblHandler.SearchNibl(searchQuery, new int[] { ((page - 1) * 20), (page * pages * 20) }, 0, pages * 50);

                    if (nibl_result.ContainsKey("packs"))
                    {
                        if (nibl_result.Value<JArray>("packs").Count > 0)
                        {
                            if (niblResults.ContainsKey("packs"))
                            {

                                JArray old = niblResults.Value<JArray>("packs");
                                JArray newarray = nibl_result.Value<JArray>("packs");
                                old.Merge(newarray);
                                niblResults["packs"] = old;

                            }
                            else
                            {
                                niblResults = nibl_result;
                            }
                        }
                        else
                        {
                            DebugHandler.TraceMessage("NO RESULTS FROM NIBL FOR ANIME: " + title, DebugSource.TASK, DebugType.WARNING);
                        }
                    }
                    else
                    {
                        DebugHandler.TraceMessage("NO RESULTS FROM NIBL AT ALL FOR ANIME: " + title, DebugSource.TASK, DebugType.WARNING);
                    }
                }
            });

            return niblResults;
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

        private async Task<JArray> CombineAnimeEpisodesParsedNiblResults(JsonKitsuAnimeInfo result, Dictionary<string, Dictionary<int, List<JObject>>> parsedNiblResult)
        {
            DebugHandler.TraceMessage("CombineAnimeEpisodesParsedNiblResults called", DebugSource.TASK, DebugType.ENTRY_EXIT);
           // DebugHandler.TraceMessage("Anime Info: ", DebugSource.TASK, DebugType.PARAMETERS);
           // DebugHandler.TraceMessage(result.ToJson(), DebugSource.TASK, DebugType.PARAMETERS);


            JArray new_anime_episodes = result.anime_episodes;

            await Task.Run(() =>
            {
                int index = 0;
                foreach (JObject episode in result.anime_episodes)
                {
                    int episodeNumber = episode["attributes"].Value<int>("number");
                    
                    JObject fileList = new JObject();

                    foreach (KeyValuePair<string, Dictionary<int, List<JObject>>> resolution in parsedNiblResult)
                    {
                        

                        if (resolution.Value.ContainsKey(episodeNumber))
                        {
                            List<JObject> files = resolution.Value[episodeNumber];
                            
                            JArray filesArray = JArray.FromObject(files);

                            fileList[resolution.Key] = filesArray;
                        }

                    }

                    new_anime_episodes[index]["files"] = fileList;

                    index++;
                }
            });

            return new_anime_episodes;          
        }

        private async Task<JsonKitsuAnimeInfo> AnimeLatestEpisode(JsonKitsuAnimeInfo result)
        {
            DebugHandler.TraceMessage("LatestEpisode called", DebugSource.TASK, DebugType.ENTRY_EXIT);

            if (result.anime_info["data"][0].Value<JObject>("attributes").ContainsKey("episodeCount"))
            {
                result.anime_total_episodes = result.anime_info["data"][0]["attributes"].Value<int>("episodeCount");
                result.anime_total_episode_pages = (int)Math.Ceiling(((double)((result.anime_total_episodes) / (double)20) + 0.5));
            }

            if (result.anime_info["data"][0]["attributes"].Value<string>("status") == "current")
            {
                List<string> animeTitles = await ParseTitles(result);
                int seasonNumber = 1;
                int previousEpisodeEnd = -1;
                JObject niblResults = new JObject();

                try
                {
                    seasonNumber = await GetSeasonFromAnime(result);
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

                foreach (string title in animeTitles)
                {
                    JObject latestEpisode = await NiblHandler.SearchNibl(title, new int[] { currentLatestEpisode, currentLatestEpisode + 1 });

                    resultNibl.Merge(latestEpisode);
                    DebugHandler.TraceMessage("Nibl result latest episode: " + resultNibl.ToString(), DebugSource.TASK, DebugType.INFO);
                }

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
                result.anime_total_episode_pages = (int)Math.Ceiling(((double)((currentLatestEpisode) / (double)20) + 0.5));

            }

            return result;
        }

    }
}
