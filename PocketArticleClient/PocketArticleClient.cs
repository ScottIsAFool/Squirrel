using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PocketArticle.Entities;
using PocketArticle.Entities.Requests;

namespace PocketArticle
{
    public class PocketArticleClient : IPocketArticleClient
    {
        private const string ApiUrl = "http://text.readitlater.com/v3beta/text?output=json";

        private readonly HttpClient _httpClient;

        public PocketArticleClient()
            : this(string.Empty)
        {
            
        }

        public PocketArticleClient(string consumerKey, int? timeout = null)
            : this(consumerKey, null, timeout)
        {
            
        }

        public PocketArticleClient(string consumerKey, HttpMessageHandler handler, int? timeout = null)
        {
            _httpClient = handler == null
                ? new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip })
                : new HttpClient(handler);

            if (timeout.HasValue)
            {
                _httpClient.Timeout = TimeSpan.FromSeconds(timeout.Value);
            }

            _httpClient.DefaultRequestHeaders.Add("Accept", "*/*");

            // defines the response format (according to the Pocket docs)
            _httpClient.DefaultRequestHeaders.Add("X-Accept", "application/json");

            ConsumerKey = consumerKey;
        }

        public string ConsumerKey { get; set; }

        public async Task<PocketArticleItem> GetArticleAsync(
            string url, 
            bool? useImagePlaceholders = null, 
            bool? useVideoPlaceholders = null,
            bool? isRefresh = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var request = new ArticleRequest
            {
                ConsumerKey = ConsumerKey,
                IsRefresh = isRefresh,
                Url = url,
                UseImagePlaceholders = useImagePlaceholders,
                UseVideoPlaceholders = useVideoPlaceholders
            };

            return await GetArticleAsync(request, cancellationToken);
        }

        public async Task<PocketArticleItem> GetArticleAsync(ArticleRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (request == null || string.IsNullOrEmpty(request.Url))
            {
                return null;
            }

            if (string.IsNullOrEmpty(request.ConsumerKey))
            {
                request.ConsumerKey = ConsumerKey;
            }

            var postData = new Dictionary<string, string>
            {
                {"consumer_key", request.ConsumerKey},
                {"url", request.Url}
            };

            if (request.IsRefresh.HasValue)
            {
                postData.Add("isRefresh", request.IsRefresh.Value ? "1" : "0");
            }

            if (request.UseImagePlaceholders.HasValue)
            {
                postData.Add("images", !request.UseImagePlaceholders.Value ? "1" : "0");
            }

            if (request.UseVideoPlaceholders.HasValue)
            {
                postData.Add("videos", !request.UseVideoPlaceholders.Value ? "1" : "0");
            }

            var r = new FormUrlEncodedContent(postData);
            var r2 = await r.ReadAsStringAsync();
            var url = string.Format("{0}&{1}", ApiUrl, r2);
            var response = await _httpClient.PostAsync(url, null, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidDataException();
            }

            var responseString = await response.Content.ReadAsStringAsync();

            var item = JsonConvert.DeserializeObject<PocketArticleItem>(responseString);

            item.OriginalUrl = request.Url;

            return item;
        }
    }
}
