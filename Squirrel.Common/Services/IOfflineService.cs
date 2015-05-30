using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cimbalino.Phone.Toolkit.Services;
using PocketArticle;
using PocketArticle.Entities;
using Squirrel.Model;

namespace Squirrel.Services
{
    public interface IOfflineService
    {
        void StartService(IAsyncStorageService storage, IPocketArticleClient reader);
        Task TakeOffline(ExtendedPocketItem item, PocketArticleItem article = null, CancellationToken cancellationToken = default(CancellationToken));
        Task<PocketArticleItem> GetArticleContent(string articleId);
        Task<bool> ArticleIsOffline(string articleId);
        Task ClearOfflineFiles();
        Task AddToDownloadQueue(ExtendedPocketItem item, bool startNow, CancellationToken cancellationToken = default(CancellationToken));
        Task AddToDownloadQueue(IEnumerable<ExtendedPocketItem> items, bool startNow, CancellationToken cancellationToken = default(CancellationToken));
        Task RemoveFromDownloadQueue(ExtendedPocketItem item, CancellationToken cancellationToken = default(CancellationToken));
        string GetImageUrl(string articleId, string filename);
        Task CheckAndStartDownloads(CancellationToken cancellationToken = default(CancellationToken));
        Task<bool> CanStartDownloading();
        Task RemoveLockFile();

        bool OnlyDownloadOnWifi { get; set; }
        bool IsDownloading { get; set; }
    }
}