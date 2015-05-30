using System;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;
using PocketArticle.Converters;
using PropertyChanged;

namespace PocketArticle.Entities
{
    [ImplementPropertyChanged]
    [JsonObject]
    [DebuggerDisplay("Url = {ResolvedUrl}, Id = {ResolvedId}")]
    public class PocketArticleItem
    {
        public string OriginalUrl { get; set; }

        [JsonProperty("resolvedUrl")]
        public string ResolvedUrl { get; set; }

        [JsonProperty("host")]
        public string Domain { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("datePublished")]
        [JsonConverter(typeof(PocketDateTimeConverter))]
        public DateTime? PublishedDate { get; set; }

        [JsonProperty("responseCode")]
        public int? ResponseCode { get; set; }

        [JsonProperty("excerpt")]
        public string Excerpt { get; set; }

        [JsonProperty("wordCount")]
        public int? WordCount { get; set; }

        [JsonProperty("isArticle")]
        [JsonConverter(typeof(BoolConverter))]
        public bool? IsArticle { get; set; }

        [JsonProperty("isIndex")]
        [JsonConverter(typeof(BoolConverter))]
        public bool? IsIndex { get; set; }

        [JsonProperty("usedFallback ")]
        [JsonConverter(typeof(BoolConverter))]
        public bool? UsedFallback { get; set; }

        [JsonProperty("article")]
        public string ArticleContent { get; set; }

        [JsonProperty("resolved_id")]
        public string ResolvedId { get; set; }

        [JsonProperty("timePublished")]
        public int? TimePublished { get; set; }

        [JsonProperty("authors")]
        [JsonConverter(typeof(ObjectToArrayConverter<PocketArticleAuthor>))]
        public List<PocketArticleAuthor> Authors { get; set; }
        
        [JsonProperty("images")]
        [JsonConverter(typeof(ObjectToArrayConverter<PocketArticleImage>))]
        public List<PocketArticleImage> Images { get; set; }

        [JsonProperty("videos")]
        [JsonConverter(typeof(ObjectToArrayConverter<PocketArticleVideo>))]
        public List<PocketArticleVideo> Videos { get; set; }

        [JsonProperty("isVideo")]
        [JsonConverter(typeof(BoolConverter))]
        public bool? IsVideo { get; set; }

        [JsonProperty("requiresLogin")]
        [JsonConverter(typeof(BoolConverter))]
        public bool? RequiresLogin { get; set; }
    }
}
