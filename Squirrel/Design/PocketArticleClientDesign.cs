using System.Threading;
using System.Threading.Tasks;
using PocketArticle;
using PocketArticle.Entities;
using PocketArticle.Entities.Requests;

namespace Squirrel.Design
{
    public class PocketArticleClientDesign : IPocketArticleClient
    {
        public string ConsumerKey { get; set; }
        public async Task<PocketArticleItem> GetArticleAsync(string url, bool? useImagePlaceholders = false, bool? useVideoPlaceholders = false, bool? isRefresh = false, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await GetArticleAsync(new ArticleRequest(), cancellationToken);
        }

        public async Task<PocketArticleItem> GetArticleAsync(ArticleRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            return new PocketArticleItem();
        }
    }
}
