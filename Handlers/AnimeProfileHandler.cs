using LittleWeebLibrary.EventArguments;
using LittleWeebLibrary.GlobalInterfaces;
using LittleWeebLibrary.Models;
using LittleWeebLibrary.StaticClasses;
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
        Task<JsonCurrentlyAiring> GetCurrentlyAiring();
        // Task<JObject> GetAnime(string id, string episodePage, string bot, string resolution);

    }
    public class AnimeProfileHandler : IAnimeProfileHandler
    {
        private readonly IKitsuHandler KitsuHandler;
        private readonly INiblHandler NiblHandler;
        private readonly IDebugHandler DebugHandler;
        private readonly WeebFileNameParser WeebFileNameParser;

        public AnimeProfileHandler(IKitsuHandler kitsuHandler, INiblHandler niblHandler, IDebugHandler debugHandler)
        {
            debugHandler.TraceMessage("Constructor Called.", DebugSource.CONSTRUCTOR, DebugType.ENTRY_EXIT);
            KitsuHandler = kitsuHandler;
            NiblHandler = niblHandler;
            DebugHandler = debugHandler;
            WeebFileNameParser = new WeebFileNameParser();
        }
       

        public async Task<JsonKitsuAnimeInfo> GetAnimeProfile(string id)
        {
            DebugHandler.TraceMessage("GetAnimeProfile called", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage("Anime ID: " + id, DebugSource.TASK, DebugType.PARAMETERS);

            JsonKitsuAnimeInfo result = await KitsuHandler.GetFullAnime(id);

            int seasonNumber = result.anime_episodes[0]["attributes"].Value<int>("seasonNumber");


            DebugHandler.TraceMessage("ANIME INFO: " + id + ", ANIME SEASON: " + seasonNumber, DebugSource.TASK, DebugType.INFO);
            DebugHandler.TraceMessage(result.ToJson(), DebugSource.TASK, DebugType.INFO);

            List<string> animeTitles = new List<string>();

            JObject titles = result.anime_info["data"][0]["attributes"].Value<JObject>("titles");

            DebugHandler.TraceMessage("START PARSING ALL TITLES", DebugSource.TASK, DebugType.INFO);

            foreach (KeyValuePair<string, JToken> title in titles)
            {
                if (!animeTitles.Contains(title.Value.ToString()))
                {
                    DebugHandler.TraceMessage("ADDED (LOCALIZED) TITLE: " + title.Value.ToString(), DebugSource.TASK, DebugType.INFO);
                    animeTitles.Add(title.Value.ToString());
                }
            }

            foreach (string title in result.anime_info["data"][0]["attributes"].Value<JArray>("abbreviatedTitles"))
            {
                if (!animeTitles.Contains(title))
                {
                    DebugHandler.TraceMessage("ADDED (NON-LOCALIZED) TITLE: " + title, DebugSource.TASK, DebugType.INFO);
                    animeTitles.Add(title);
                }
            }


            DebugHandler.TraceMessage("PARSED ANIME TITLES: " + id, DebugSource.TASK, DebugType.INFO);

            JObject niblResults = new JObject();

            foreach (string title in animeTitles)
            {
                DebugHandler.TraceMessage("SEARCHING NIBL FOR: " + title, DebugSource.TASK, DebugType.INFO);
                Dictionary<string, string> parsed = WeebFileNameParser.ParseFullString(title);

                string searchQuery = parsed["MainAnimeTitle"];

                if (parsed["SubAnimeTitle"].Length > 0)
                {
                    searchQuery += " " + parsed["SubAnimeTitle"];
                }


                JObject nibl_result = await NiblHandler.SearchNibl(searchQuery);

                DebugHandler.TraceMessage("FINISHED SEARCHING NIBL FOR: " + title, DebugSource.TASK, DebugType.INFO);
                if (nibl_result.ContainsKey("packs"))
                {
                    if (nibl_result.Value<JArray>("packs").Count > 0)
                    {
                        if (niblResults.ContainsKey("packs"))
                        {
                            DebugHandler.TraceMessage("nibl results already contains packs", DebugSource.TASK, DebugType.INFO);

                            JArray old = niblResults.Value<JArray>("packs");
                            JArray newarray = nibl_result.Value<JArray>("packs");
                            old.Merge(newarray);
                            niblResults["packs"] = old;
                        }
                        else
                        {
                            DebugHandler.TraceMessage("nibl results does not contain packs", DebugSource.TASK, DebugType.INFO);
                            niblResults = nibl_result;
                        }
                    }
                    else
                    {
                        DebugHandler.TraceMessage("NO RESULTS FROM NIBL", DebugSource.TASK, DebugType.INFO);
                    }
                }
                else
                {
                    DebugHandler.TraceMessage("NO RESULTS FROM NIBL AT ALL", DebugSource.TASK, DebugType.INFO);
                }
            }

            Dictionary<string, Dictionary<int, List<JObject>>> resolutions = new Dictionary<string, Dictionary<int, List<JObject>>>()
                {
                    { "UNKNOWN", new Dictionary<int, List<JObject>>()},
                    { "480P", new Dictionary<int, List<JObject>>()},
                    { "720P", new Dictionary<int, List<JObject>>()},
                    { "1080P", new Dictionary<int, List<JObject>>()}
                };

            foreach (JObject pack in niblResults.Value<JArray>("packs"))
            {
                string resolution = "UNKONWN";

                if (pack.ContainsKey("Video_Resolution"))
                {
                    resolution = pack.Value<string>("Video_Resolution");
                }

                if (!resolutions.ContainsKey(resolution))
                {
                    if (pack.ContainsKey("Episode"))
                    {
                        resolutions.Add(resolution, new Dictionary<int, List<JObject>>() { { int.Parse(pack.Value<string>("Episode")), new List<JObject>() { pack } } });
                    }
                    else
                    {
                        resolutions.Add(resolution, new Dictionary<int, List<JObject>>() { { -1, new List<JObject>() { pack } } });
                    }
                }
                else
                {
                    if (pack.ContainsKey("Episode"))
                    {
                        if (resolutions[resolution].ContainsKey(int.Parse(pack.Value<string>("Episode"))))
                        {

                            resolutions[resolution][int.Parse(pack.Value<string>("Episode"))].Add(pack);
                        }
                        else
                        {
                            resolutions[resolution].Add(int.Parse(pack.Value<string>("Episode")), new List<JObject>() { pack });
                        }
                    }
                    else
                    {
                        if (resolutions[resolution].ContainsKey(-1))
                        {
                            resolutions[resolution][-1].Add(pack);
                        }
                        else
                        {
                            resolutions[resolution].Add(-1, new List<JObject>() { pack });
                        }
                    }
                }
            }


            DebugHandler.TraceMessage("FINISHED SEPERATING RESOLUTION: " + resolutions.Count.ToString(), DebugSource.TASK, DebugType.INFO);

            DebugHandler.TraceMessage("PARSING PER EPISODE ", DebugSource.TASK, DebugType.INFO);
            JArray new_anime_episodes = result.anime_episodes;

            int index = 0;
            foreach (JObject episode in result.anime_episodes)
            {
                int episodeNumber = episode["attributes"].Value<int>("number");

                DebugHandler.TraceMessage("PARSING EPISODE: " + episodeNumber, DebugSource.TASK, DebugType.INFO);
                JObject fileList = new JObject();

                foreach (KeyValuePair<string, Dictionary<int, List<JObject>>> resolution in resolutions)
                {

                    DebugHandler.TraceMessage("ADDING RESOLUTION: " + resolution.Key, DebugSource.TASK, DebugType.INFO);

                    if (resolution.Value.ContainsKey(episodeNumber))
                    {
                        List<JObject> files = resolution.Value[episodeNumber];


                        DebugHandler.TraceMessage("FOUND : " + files.Count + " AMOUNT OF FILES FOR EPISODE: " + episodeNumber, DebugSource.TASK, DebugType.INFO);

                        JArray filesArray = JArray.FromObject(files);

                        fileList[resolution.Key] = filesArray;
                    }
                    else
                    {
                        DebugHandler.TraceMessage(" DID NOT FIND FILES FOR EPISODE: " + episodeNumber, DebugSource.TASK, DebugType.INFO);

                    }

                }

                new_anime_episodes[index]["files"] = fileList;

                index++;
            }

            DebugHandler.TraceMessage("FINISHED PARSING EPISODES", DebugSource.TASK, DebugType.INFO);
            result.anime_episodes = new_anime_episodes;


            DebugHandler.TraceMessage("ANIME INFO: " + id, DebugSource.TASK, DebugType.INFO);
            DebugHandler.TraceMessage(result.ToJson(), DebugSource.TASK, DebugType.INFO);

            return result;
        }

        public async Task<JsonCurrentlyAiring> GetCurrentlyAiring()
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

            JObject result = await NiblHandler.GetLatestFiles("21");
            JArray array = result.Value<JArray>("packs");
            List<string> animeIdsAdded = new List<string>();

            foreach (JObject pack in array) {
                if (pack["Video_Resolution"] != null) {
                    if (pack.Value<string>("Video_Resolution") == "720P")
                    {

                        string title_compare = pack.Value<string>("Title").ToLower();

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
                        if (!animeIdsAdded.Contains(currentlyairing.Value<string>("id")) && max > 0.5)
                        {

                            currentlyairing.Add("latest_episode", pack);
                            latestAiringKitsu.Add(currentlyairing);

                            animeIdsAdded.Add(currentlyairing.Value<string>("id"));
                        }
                    }

                }
                else
                {
                    string title_compare = pack.Value<string>("Title").ToLower();

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
                    if (!animeIdsAdded.Contains(currentlyairing.Value<string>("id")) && max > 0.5)
                    {

                        currentlyairing.Add("latest_episode", pack);
                        latestAiringKitsu.Add(currentlyairing);

                        animeIdsAdded.Add(currentlyairing.Value<string>("id"));
                    }
                }
            }

            foreach (JObject anime in latestAiring)
            {
                if (!animeIdsAdded.Contains(anime.Value<string>("id")))
                {
                    nonLatestAiringKitsu.Add(anime);
                }
            }

            JObject resultCurrentlyAiringKitsu =  new JObject { ["airing_ordered"] = JToken.FromObject(latestAiringKitsu), ["airing_non_ordered"] = JToken.FromObject(nonLatestAiringKitsu) };

            JsonCurrentlyAiring airing = new JsonCurrentlyAiring
            {
                result = resultCurrentlyAiringKitsu
            };

            return airing;

        }

    }
}
