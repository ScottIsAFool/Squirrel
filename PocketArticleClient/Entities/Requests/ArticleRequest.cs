using Newtonsoft.Json;
using PocketArticle.Converters;
using PropertyChanged;

namespace PocketArticle.Entities.Requests
{
    [ImplementPropertyChanged]
    [JsonObject]
    public class ArticleRequest
    {
        [JsonProperty("images")]
        [JsonConverter(typeof(BoolConverter))]
        public bool? UseImagePlaceholders { get; set; }
        
        [JsonProperty("videos")]
        [JsonConverter(typeof(BoolConverter))]
        public bool? UseVideoPlaceholders { get; set; }
        
        [JsonProperty("refresh")]
        [JsonConverter(typeof(BoolConverter))]
        public bool? IsRefresh { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("consumer_key")]
        internal string ConsumerKey { get; set; }
    }
}
