using Newtonsoft.Json;

namespace PocketArticle.Entities
{
    public class PocketArticleVideo
    {
        [JsonProperty("item_id")]
        public string ItemId { get; set; }

        [JsonProperty("video_id")]
        public string VideoId { get; set; }

        [JsonProperty("src")]
        public string Source { get; set; }

        [JsonProperty("width")]
        public string Width { get; set; }

        [JsonProperty("height")]
        public string Height { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("vid")]
        public string VideoSitesId { get; set; }
    }
}