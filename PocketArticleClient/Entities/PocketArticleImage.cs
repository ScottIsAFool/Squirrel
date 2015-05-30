using Newtonsoft.Json;

namespace PocketArticle.Entities
{
    public class PocketArticleImage
    {
        [JsonProperty("item_id")]
        public string ItemId { get; set; }

        [JsonProperty("image_id")]
        public string ImageId { get; set; }

        [JsonProperty("src")]
        public string Source { get; set; }

        [JsonProperty("width")]
        public string Width { get; set; }

        [JsonProperty("height")]
        public string Height { get; set; }

        [JsonProperty("credit")]
        public string Credit { get; set; }

        [JsonProperty("caption")]
        public string Caption { get; set; }
    }
}