using LittleWeebLibrary.EventArguments;
using LittleWeebLibrary.GlobalInterfaces;
using LittleWeebLibrary.Models;
using LittleWeebLibrary.StaticClasses;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace LittleWeebLibrary.Handlers
{
    public interface IKitsuHandler
    {

        Task<JsonKitsuAnimeInfo> GetFullAnime(string animeId);
        Task<JsonKistuSearchResult> SearchAnime(string search, Dictionary<string,string> queryList, int page = 0, int pages = -1);
        Task<JObject> GetAnime(string animeId);
        Task<JArray> GetEpisodes(string animeId, int page = 0, int pages = -1, int results = 20, string order = "ASC");
        Task<JObject> GetEpisode(string animeId, int episodeNumber);
        Task<JArray> GetRelations(string animeId);
        Task<JArray> GetGenres(string animeId);
        Task<JArray> GetAllGenres();
        Task<JArray> GetAllCategories();
        Task<JArray> GetCategories(string animeId);
        Task<JArray> GetCurrentlyAiring();
    }

    public class KitsuHandler :  IKitsuHandler
    {       

        private readonly IDebugHandler DebugHandler;

        public KitsuHandler(IDebugHandler debugHandler)
        {
            debugHandler.TraceMessage("Constructor Called", DebugSource.CONSTRUCTOR, DebugType.ENTRY_EXIT);
            DebugHandler = debugHandler;
        }

        public async Task<JsonKistuSearchResult> SearchAnime(string search, Dictionary<string,string> queryList, int page = 0, int pages = -1) {

            DebugHandler.TraceMessage("SearchAnime Called", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage("Search: " + search + ", page: " + page.ToString() + ", pages: " + pages.ToString(), DebugSource.TASK, DebugType.PARAMETERS);
            JArray array = new JArray();

            int totalPages = 2;

            if (pages == -1) {
                pages = totalPages;
            }

            string query = "";

            foreach (KeyValuePair<string, string> attributeValue in queryList)
            {
                query += "&filter[" + attributeValue.Key + "]=" + attributeValue.Value;
            }

            if (search != string.Empty)
            {
                query = "&filter[text]=" + search + query;
            }


            string searchresult = await Get("anime?" + query.Substring(1) + "&fields[anime]=canonicalTitle,averageRating,subtype,status,coverImage,posterImage,abbreviatedTitles,titles&page[limit]=20&page[offset]=0");

            if (searchresult.Contains("failed:"))
            {
                JsonKistuSearchResult resultfail = new JsonKistuSearchResult()
                {
                    result = array
                };

                return resultfail;
            }

            JObject searchresultjobject = JObject.Parse(searchresult);

            totalPages = (int)Math.Ceiling(((double)((searchresultjobject["meta"].Value<int>("count")) / (double)20) + 0.5));

            if (pages == -1)
            {
                pages = totalPages;
            }

            array.Merge(searchresultjobject.Value<JArray>("data"));
            for (int i = 1; i < pages; i++) {

                searchresult = await Get("anime?" + query.Substring(1) + "&fields[anime]=canonicalTitle,averageRating,subtype,status,coverImage,posterImage,abbreviatedTitles,titles&page[limit]=20&page[offset]=" + ((page + i - 1) * 20).ToString());

                if (searchresult.Contains("failed:"))
                {
                    break;
                }

                searchresultjobject = JObject.Parse(searchresult);

                array.Merge(searchresultjobject.Value<JArray>("data"));

                if ((i + page) >= totalPages)
                {
                    break;
                }
            }
            JsonKistuSearchResult result = new JsonKistuSearchResult()
            {
                result = array
            };
            return result;
        }

        public async Task<JObject> GetAnime(string animeId)
        {

            DebugHandler.TraceMessage("GetAnime Called", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage("Anime ID: " + animeId, DebugSource.TASK, DebugType.PARAMETERS);

            string anime = await Get("anime?filter[id]=" + animeId);

            if (anime.Contains("failed:"))
            {
                return new JObject();
            }
            else
            {
                return JObject.Parse(anime);
            }
        }

        public async Task<JArray> GetEpisodes(string animeId, int page = 0, int pages = -1, int results = 20, string order = "ASC")
        {
            DebugHandler.TraceMessage("GetEpisodes Called", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage("AnimeID: " + animeId + ", page: " + page.ToString() + ", pages: " + pages.ToString(), DebugSource.TASK, DebugType.PARAMETERS);

            string orderquery= "";

            if (order == "DESC")
            {
                orderquery = "-";
            }

            string episodes = await Get("episodes?filter[mediaType]=Anime&filter[media_id]=" + animeId + "&page[limit]=20&page[offset]=" + ((page) * results).ToString() + "&sort=" + orderquery + "number");

            JObject searchresultjobject = JObject.Parse(episodes);

            JArray array = new JArray();

            int totalPages = 1;

            if (pages == -1)
            {
                totalPages = (int)Math.Ceiling(((double)((searchresultjobject["meta"].Value<int>("count")) / (double)20) + 0.5));
                pages = totalPages;
            }

            List<Task<JObject>> tasks = new List<Task<JObject>>();   

            array.Merge(searchresultjobject.Value<JArray>("data"));

            DebugHandler.TraceMessage("Amount of pages to interate: " + totalPages, DebugSource.TASK, DebugType.INFO);

            for (int i = 1; i < totalPages; i++)
            {
                if (episodes.Contains("failed:"))
                {
                    break;
                }

                
                tasks.Add(GetEpisodesFromKitsu(animeId, page, i));
                              

                if ((i + page) >= totalPages)
                {
                    break;
                }
            }


            Task.WaitAll(tasks.ToArray());

            foreach (Task<JObject> task in tasks)
            {
                array.Merge(task.Result.Value<JArray>("data"));
                task.Dispose();
            }

            return array;           

        }

        public async Task<JsonKitsuAnimeInfo> GetFullAnime(string animeId)
        {
            DebugHandler.TraceMessage("GetFullAnime Called", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage("Anime ID: " + animeId, DebugSource.TASK, DebugType.PARAMETERS);

            string anime = await Get("episodes?filter[mediaType]=Anime&filter[media_id]=" + animeId);


            JsonKitsuAnimeInfo jsonKitsuAnimeInfo = new JsonKitsuAnimeInfo
            {
                anime_id = animeId,
                anime_info = await GetAnime(animeId),
                anime_relations = await GetRelations(animeId),
                anime_categories = await GetCategories(animeId),
                anime_genres = await GetGenres(animeId)
            };
            
           //DebugHandler.TraceMessage("Finished getting anime profile, making values 'episodeCount' & 'total episode pages global': " + jsonKitsuAnimeInfo.anime_info["data"][0]["attributes"].Value<int>("episodeCount"), DebugSource.TASK, DebugType.PARAMETERS);            

            return jsonKitsuAnimeInfo;

        }

        public async Task<JArray> GetRelations(string animeId)
        {
            DebugHandler.TraceMessage("GetRelations Called", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage("Anime ID: " + animeId, DebugSource.TASK, DebugType.PARAMETERS);
            
            string relations = await Get("media-relationships?filter[source_id]=" + animeId + "&filter[source_type]=Anime&fields[anime]=canonicalTitle,averageRating,subtype,status,coverImage,posterImage,abbreviatedTitles,titles&include=destination&sort=role");

            if (relations.Contains("failed:"))
            {
                return new JArray();
            }
            else
            {
                JObject result = JObject.Parse(relations);
                JArray data = result.Value<JArray>("data");
                JArray included = result.Value<JArray>("included");

                int i = 0;
                foreach (JObject roles in data)
                {
                    JObject anime = included[i].Value<JObject>();

                    anime.Add(new JProperty("role", roles["attributes"].Value<string>("role")));

                    included[i] = anime;

                    i++;

                }

                return included;
            }

        }


        public async Task<JArray> GetCurrentlyAiring()
        {

            DebugHandler.TraceMessage("GetCurrentlyAiring Called", DebugSource.TASK, DebugType.ENTRY_EXIT);

            JArray array = new JArray();

            int totalPages = 2;

            List<Task<string>> tasks = new List<Task<string>>();

            string airing = await Get("anime?filter[status]=current&fields[anime]=canonicalTitle,averageRating,subtype,status,coverImage,posterImage,abbreviatedTitles,titles&page[limit]=20&page[offset]=" + (0 * 20).ToString());
            JObject airingresultjobject = JObject.Parse(airing);

            array.Merge(airingresultjobject.Value<JArray>("data"));


            totalPages = (int)(airingresultjobject["meta"].Value<int>("count") / 20 - 0.5);

            for (int i = 1; i < totalPages; i++)
            {
                if (airing.Contains("failed:"))
                {
                    break;
                }


                tasks.Add(Get("anime?filter[status]=current&fields[anime]=canonicalTitle,averageRating,subtype,status,coverImage,posterImage,abbreviatedTitles,titles&page[limit]=20&page[offset]=" + (i * 20).ToString()));


                if (i >= totalPages)
                {
                    break;
                }
            }

            Task.WaitAll(tasks.ToArray());

            foreach (Task<string> task in tasks)
            {
                airingresultjobject = JObject.Parse(task.Result);

                try
                {
                    array.Merge(airingresultjobject.Value<JArray>("data"));
                }
                catch (Exception e)
                {
                    DebugHandler.TraceMessage("Failed merging data: " + e.ToString(), DebugSource.TASK, DebugType.WARNING);
                }
                task.Dispose();
            }



            return array;
        }

        public async Task<JArray> GetGenres(string animeId)
        {
            DebugHandler.TraceMessage("GetGenres Called", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage("Anime ID: " + animeId, DebugSource.TASK, DebugType.PARAMETERS);

            string genres = await Get("anime/" + animeId + "/genres");

            if (genres.Contains("failed:"))
            {
                return new JArray();
            }
            else
            {
                return JObject.Parse(genres).Value<JArray>("data");
            }
        }

        public async Task<JArray> GetCategories(string animeId)
        {
            DebugHandler.TraceMessage("GetCategories Called", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage("Anime ID: " + animeId, DebugSource.TASK, DebugType.PARAMETERS);

            string categories = await Get("anime/" + animeId + "/categories");

            if (categories.Contains("failed:"))
            {
                return new JArray();
            }
            else
            {
                return JObject.Parse(categories).Value<JArray>("data");
            }
        }


        private async Task<JObject> GetEpisodesFromKitsu(string animeId, int page, int offset)
        {
            string episodes = await Get("episodes?filter[mediaType]=Anime&filter[media_id]=" + animeId + "&page[limit]=20&page[offset]=" + ((page + offset) * 20).ToString() + "&sort=number");
            return JObject.Parse(episodes);
        }

        private async Task<string> Get(string url)
        {
            DebugHandler.TraceMessage("Get called", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage("URL: " + url, DebugSource.TASK, DebugType.PARAMETERS);
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync("https://kitsu.io/api/edge/" + url);

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

        public async Task<JObject> GetEpisode(string animeId, int episodeNumber)
        {
            string episode = await Get("episodes?filter[mediaType]=Anime&filter[media_id]=" + animeId + "&filter[number]=" + episodeNumber);
            if (episode.Contains("failed:"))
            {
                return new JObject();
            }
            else
            {
                JObject searchresultjobject = JObject.Parse(episode);

                return searchresultjobject["data"].Value<JObject>(0);
            }
        }

        public async Task<JArray> GetAllGenres()
        {
            DebugHandler.TraceMessage("GetAllGenres Called", DebugSource.TASK, DebugType.ENTRY_EXIT);

            JArray array = new JArray();

            int totalPages = 0;

            List<Task<string>> tasks = new List<Task<string>>();

            string genres = await Get("genres?fields[genres]=name&page[limit]=20&page[offset]=" + (0 * 20).ToString());
            JObject genresresultjobject = JObject.Parse(genres);

            array.Merge(genresresultjobject.Value<JArray>("data"));


            totalPages = (int)(genresresultjobject["meta"].Value<int>("count") / 20 - 0.5);

            for (int i = 1; i < totalPages; i++)
            {
                if (genres.Contains("failed:"))
                {
                    break;
                }

                tasks.Add(Get("genres?fields[genres]=name&page[limit]=20&page[offset]=" + (i * 20).ToString()));
                
               
            }

            Task.WaitAll(tasks.ToArray());

            foreach (Task<string> task in tasks)
            {
                genresresultjobject = JObject.Parse(task.Result);

                try
                {
                    array.Merge(genresresultjobject.Value<JArray>("data"));
                }
                catch (Exception e)
                {
                    DebugHandler.TraceMessage("Failed merging data: " + e.ToString(), DebugSource.TASK, DebugType.WARNING);
                }
                task.Dispose();
            }

            return array;
        }

        public async Task<JArray> GetAllCategories()
        {
            DebugHandler.TraceMessage("GetAllCategories Called", DebugSource.TASK, DebugType.ENTRY_EXIT);

            JArray array = new JArray();

            int totalPages = 0;

            List<Task<string>> tasks = new List<Task<string>>();

            string categories = await Get("categories?fields[categories]=title&page[limit]=20&page[offset]=" + (0 * 20).ToString());
            JObject categoriesresultjobject = JObject.Parse(categories);

            array.Merge(categoriesresultjobject.Value<JArray>("data"));


            totalPages = (int)(categoriesresultjobject["meta"].Value<int>("count") / 20 - 0.5);

            for (int i = 1; i < totalPages; i++)
            {
                if (categories.Contains("failed:"))
                {
                    break;
                }

                tasks.Add(Get("categories?fields[categories]=title&page[limit]=20&page[offset]=" + (i * 20).ToString()));

            }

            Task.WaitAll(tasks.ToArray());

            foreach (Task<string> task in tasks)
            {
                categoriesresultjobject = JObject.Parse(task.Result);

                try
                {
                    array.Merge(categoriesresultjobject.Value<JArray>("data"));
                }
                catch (Exception e)
                {
                    DebugHandler.TraceMessage("Failed merging data: " + e.ToString(), DebugSource.TASK, DebugType.WARNING);
                }
                task.Dispose();
            }

            return array;
        }
    }
}
