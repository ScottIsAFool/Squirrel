using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Cimbalino.Phone.Toolkit.Services;
using PocketArticle;
using PocketArticle.Entities;
using PocketSharp;
using PocketSharp.Models;
using ScottIsAFool.WindowsPhone.Extensions;
using ScottIsAFool.WindowsPhone.Logging;
using ServiceStack.Text;
using Squirrel.Extensions;
using Squirrel.Model;

namespace Squirrel.Services
{
    public class OfflineService : IOfflineService
    {
        private const string OfflineFolder = "Offline";
        private const string DownloadQueueFile = "DownloadQueue";
        private const string ArticleTemplate = OfflineFolder + "\\{0}\\article";
        private const string ImageTemplate = "{0}\\{1}\\{2}";
        private const string ImageUrlTemplate = "isostore:/" + OfflineFolder + "/{0}/{1}";
        private const string LockFile = "LockFile";

        private static OfflineService _current;

        private readonly ILog _logger;
        private IAsyncStorageService _storage;
        private IPocketArticleClient _reader;
        private List<ExtendedPocketItem> _downloadQueue;

        public static OfflineService Current { get { return _current ?? (_current = new OfflineService()); } }

        public bool OnlyDownloadOnWifi { get; set; }
        public bool IsDownloading { get; set; }
        public bool IsRunning { get; private set; }

        public OfflineService()
        {
            _logger = new WPLogger(GetType());
        }

        public async void StartService(IAsyncStorageService storage, IPocketArticleClient reader)
        {
            _logger.Info("Starting Service");
            _storage = storage;
            _reader = reader;

            CheckAndCreateOfflineFolder().ConfigureAwait(false);
            _downloadQueue = await GetDownloadQueue();
        }

        public async Task TakeOffline(ExtendedPocketItem item, PocketArticleItem article = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (item == null || item.Uri == null)
            {
                await RemoveFromDownloadQueue(item, cancellationToken);
                return;
            }

            if (await _storage.FileExistsAsync(GetArticleFile(item.ID.ToString(CultureInfo.InvariantCulture))))
            {
                await RunNextItem(item, article, cancellationToken);
                return;
            }

            IsDownloading = true;

            if (article == null)
            {
                try
                {
                    var articleDetails = await _reader.GetArticleAsync(item.Uri.ToString(), useImagePlaceholders: true, useVideoPlaceholders: true, cancellationToken: cancellationToken);
                    article = articleDetails;
                }
                catch (PocketException ex)
                {
                    ex.Log("TakeOffline()", _logger);
                    return;
                }
                catch (Exception ex)
                {
                    _logger.ErrorException("TakeOffline()", ex);
                    return;
                }
            }

            if (cancellationToken.IsCancellationRequested)
            {
                IsRunning = false;
                return;
            }

            await SaveImages(item.Images, article.ResolvedId, cancellationToken).ContinueWith(async task =>
            {
                await RunNextItem(item, article, cancellationToken);
            }, cancellationToken);
        }

        private async Task RunNextItem(ExtendedPocketItem item, PocketArticleItem article, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                IsRunning = false;
                return;
            }

            if (article != null)
            {
                var json = article.ToJson();

                await _storage.WriteAllTextAsync(GetArticleFile(article.ResolvedId), json);
            }

            await RemoveFromDownloadQueue(item, cancellationToken);
        }

        public async Task<PocketArticleItem> GetArticleContent(string articleId)
        {
            if (!await _storage.FileExistsAsync(GetArticleFile(articleId)))
            {
                return null;
            }

            var json = await _storage.ReadAllTextAsync(GetArticleFile(articleId));

            return json.FromJson<PocketArticleItem>();
        }

        public async Task<bool> ArticleIsOffline(string articleId)
        {
            return await _storage.FileExistsAsync(GetArticleFile(articleId));
        }

        public async Task ClearOfflineFiles()
        {
            if (await _storage.DirectoryExistsAsync(OfflineFolder))
            {
                await _storage.DeleteDirectoryAsync(OfflineFolder);
            }

            await _storage.CreateDirectoryAsync(OfflineFolder);
        }

        public async Task AddToDownloadQueue(IEnumerable<ExtendedPocketItem> items, bool startNow, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (_downloadQueue.IsNullOrEmpty())
            {
                _downloadQueue = new List<ExtendedPocketItem>();
            }

            _downloadQueue.AddRange(items);
            await _storage.WriteAllTextAsync(DownloadQueueFile, _downloadQueue.ToJson());

            if (startNow)
            {
                CheckAndStartDownloads(cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task AddToDownloadQueue(ExtendedPocketItem item, bool startNow, CancellationToken cancellationToken = default(CancellationToken))
        {
            await AddToDownloadQueue(new List<ExtendedPocketItem> {item}, startNow, cancellationToken);
        }

        public async Task RemoveFromDownloadQueue(ExtendedPocketItem item, CancellationToken cancellationToken = default(CancellationToken))
        {
            var itemToRemove = _downloadQueue.FirstOrDefault(x => x.ID == item.ID);
            if (itemToRemove != null)
            {
                _downloadQueue.Remove(itemToRemove);
                await _storage.WriteAllTextAsync(DownloadQueueFile, _downloadQueue.ToJson());
            }

            if (_downloadQueue.Any())
            {
                var nextItem = _downloadQueue.FirstOrDefault();
                TakeOffline(nextItem, cancellationToken: cancellationToken).ConfigureAwait(false);
            }
            else
            {
                IsDownloading = false;
            }
        }

        public async Task CheckAndStartDownloads(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!await NavigationUtils.CheckNetwork(OnlyDownloadOnWifi))
            {
                return;
            }

            if (_downloadQueue.IsNullOrEmpty())
            {
                return;
            }

            var item = _downloadQueue.FirstOrDefault();
            IsRunning = true;
            await TakeOffline(item, cancellationToken: cancellationToken);
        }

        public async Task<bool> CanStartDownloading()
        {
            if (await _storage.FileExistsAsync(LockFile))
            {
                return false;
            }

            try
            {
                await _storage.CreateFileAsync(LockFile);

                return true;
            }
            catch (Exception ex)
            {
                _logger.ErrorException("CanStartDownloading()", ex);
                return false;
            }
        }

        public async Task RemoveLockFile()
        {
            if (await _storage.FileExistsAsync(LockFile))
            {
                await _storage.DeleteFileAsync(LockFile);
            }
        }

        private async Task<List<ExtendedPocketItem>> GetDownloadQueue()
        {
            if (!await _storage.FileExistsAsync(DownloadQueueFile))
            {
                return new List<ExtendedPocketItem>();
            }

            var json = await _storage.ReadAllTextAsync(DownloadQueueFile);

            return json.FromJson<List<ExtendedPocketItem>>();
        }

        private async Task SaveImages(IList<PocketImage> images, string articleId, CancellationToken cancellationToken)
        {
            if (images.IsNullOrEmpty())
            {
                return;
            }

            var client = new HttpClient();
            foreach (var image in images)
            {
                var response = await client.GetAsync(image.Uri, cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    var filename = GetImageFileName(image.Uri);
                    var bytes = await response.Content.ReadAsByteArrayAsync();
                    try
                    {
                        await _storage.WriteAllBytesAsync(GetImagePath(articleId, filename), bytes);
                    }
                    catch (Exception ex)
                    {
                        _logger.ErrorException("SaveImages(" + filename + ")", ex);
                    }
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    IsRunning = false;
                    break;
                }
            }
        }

        public string GetImageFileName(Uri imageUri)
        {
            var filename = Path.GetFileName(imageUri.ToString());
            return string.IsNullOrEmpty(imageUri.Query) ? filename : filename.Replace(imageUri.Query, string.Empty);
        }

        public string GetImageFileName(string imageUri)
        {
            return GetImageFileName(new Uri(imageUri, UriKind.Absolute));
        }

        private string GetImagePath(string articleId, string filename)
        {
            return string.Format(ImageTemplate, OfflineFolder, articleId, filename);
        }

        public string GetImageUrl(string articleId, string filename)
        {
            return string.Format(ImageUrlTemplate, articleId, filename);
        }

        private string GetArticleFile(string articleId)
        {
            return string.Format(ArticleTemplate, articleId);
        }

        private async Task CheckAndCreateOfflineFolder()
        {
            if (!await _storage.DirectoryExistsAsync(OfflineFolder))
            {
                await _storage.CreateDirectoryAsync(OfflineFolder);
            }
        }
    }
}
