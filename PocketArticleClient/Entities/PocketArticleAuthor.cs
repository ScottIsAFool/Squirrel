using Newtonsoft.Json;

namespace PocketArticle.Entities
{
    public class PocketArticleAuthor
    {
        [JsonProperty("author_id")]
        public string AuthorId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }
}