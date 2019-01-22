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
        Task<JArray> SearchAnime(string search, int page = 0, int pages = -1);
        Task<JObject> GetAnime(string animeId);
        Task<JArray> GetEpisodes(string animeId, int page = 0, int pages = -1);
        Task<JArray> GetRelations(string animeId);
        Task<JArray> GetGenres(string animeId);
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

        public async Task<JArray> SearchAnime(string search, int page = 0, int pages = -1) {

            DebugHandler.TraceMessage("SearchAnime Called", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage("Search: " + search + ", page: " + page.ToString() + ", pages: " + pages.ToString(), DebugSource.TASK, DebugType.PARAMETERS);
            JArray array = new JArray();

            int totalPages = 2;

            if (pages == -1) {
                pages = totalPages;
            }

            for (int i = 0; i < pages; i++) {

                string searchresult = await Get("anime?filter[text]=" + search + "&page[limit]=20&page[offset]=" + ((page + i - 1) * 20).ToString());

                if (searchresult.Contains("failed:"))
                {
                    break;
                }

                JObject searchresultjobject = JObject.Parse(searchresult);

                totalPages = (int)(searchresultjobject["meta"].Value<int>("count") / 20 - 0.5);

                if (pages == -1)
                {
                    pages = totalPages;
                }

                array.Merge(searchresultjobject.Value<JArray>("data"));

                if ((i + page) >= totalPages)
                {
                    break;
                }
            }

            return array;
        }

        public async Task<JObject> GetAnime(string animeId)
        {

            DebugHandler.TraceMessage("GetAnime Called", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage("Anime ID: " + animeId, DebugSource.TASK, DebugType.PARAMETERS);

            string anime = await Get("anime?filter[id]=" + animeId + "&include=genres,mediaRelationships,categories");

            if (anime.Contains("failed:"))
            {
                return new JObject();
            }
            else
            {
                return JObject.Parse(anime);
            }
        }

        public async Task<JArray> GetEpisodes(string animeId, int page = 0, int pages = -1)
        {
            DebugHandler.TraceMessage("GetEpisodes Called", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage("AnimeID: " + animeId + ", page: " + page.ToString() + ", pages: " + pages.ToString(), DebugSource.TASK, DebugType.PARAMETERS);

            JArray array = new JArray();

            int totalPages = 2;

            if (pages == -1)
            {
                pages = totalPages;
            }


            for (int i = 0; i < pages; i++)
            {
                string episodes = await Get("episodes?filter[mediaType]=Anime&filter[media_id]=" + animeId + "&page[limit]=20&page[offset]=" + ((page + i) * 20).ToString());

                if (episodes.Contains("failed:"))
                {
                    break;
                }

                JObject searchresultjobject = JObject.Parse(episodes);

                totalPages = (int)(searchresultjobject["meta"].Value<int>("count") / 20 - 0.5);

                if (pages == -1)
                {
                    pages = totalPages;
                }

                array.Merge(searchresultjobject.Value<JArray>("data"));

                if ((i + page) >= totalPages)
                {
                    break;
                }
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
                anime_info = await GetAnime(animeId),
                anime_relations = await GetRelations(animeId),
                anime_episodes = await GetEpisodes(animeId),
                anime_categories = await GetCategories(animeId),
                anime_genres = await GetGenres(animeId)
            };


            return jsonKitsuAnimeInfo;

        }

        public async Task<JArray> GetRelations(string animeId)
        {
            DebugHandler.TraceMessage("GetRelations Called", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage("Anime ID: " + animeId, DebugSource.TASK, DebugType.PARAMETERS);
            
            string relations = await Get("media-relationships?filter[source_id]=" + animeId + "&filter[source_type]=Anime&include=destination&sort=role");

            if (relations.Contains("failed:"))
            {
                return new JArray();
            }
            else
            {
                return JObject.Parse(relations).Value<JArray>("included");
            }

        }


        public async Task<JArray> GetCurrentlyAiring()
        {

            DebugHandler.TraceMessage("GetCurrentlyAiring Called", DebugSource.TASK, DebugType.ENTRY_EXIT);

            JArray array = new JArray();

            int totalPages = 2;

            for (int i = 0; i < totalPages; i++)
            {

                string airing = await Get("anime?filter[status]=current&page[limit]=20&page[offset]=" + (i * 20).ToString());

                if (airing.Contains("failed:"))
                {
                    break;
                }

                JObject airingresultjobject = JObject.Parse(airing);

                totalPages = (int)(airingresultjobject["meta"].Value<int>("count") / 20 - 0.5);

                array.Merge(airingresultjobject.Value<JArray>("data"));

                if (i >= totalPages)
                {
                    break;
                }
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

      
    }
}
