using System.Threading;
using System.Threading.Tasks;
using PocketArticle.Entities;
using PocketArticle.Entities.Requests;

namespace PocketArticle
{
    public interface IPocketArticleClient
    {
        string ConsumerKey { get; set; }

        Task<PocketArticleItem> GetArticleAsync(
            string url, 
            bool? useImagePlaceholders = null, 
            bool? useVideoPlaceholders = null,
            bool? isRefresh = null,
            CancellationToken cancellationToken = default(CancellationToken));

        Task<PocketArticleItem> GetArticleAsync(ArticleRequest request, CancellationToken cancellationToken = default(CancellationToken));
    }
}