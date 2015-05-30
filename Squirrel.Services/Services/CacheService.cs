using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Cimbalino.Phone.Toolkit.Extensions;
using Cimbalino.Phone.Toolkit.Services;
using Newtonsoft.Json;
using ScottIsAFool.WindowsPhone.Extensions;
using ScottIsAFool.WindowsPhone.Logging;
using Squirrel.Extensions;
using Squirrel.Model;

namespace Squirrel.Services
{
    public class CacheService : ICacheService
    {
        private readonly ILog _logger;
        private const string CacheFolder = "DataCache.{0}";
        private const string RecentKey = "Recent";
        private const string FavouritesKey = "Favourites";
        private const string ArchivedKey = "Archived";
        private const string OfflineKey = "Offline";
        private const string CacheTime = "{0}_Cache_Date";
        private const string SinceTime = "{0}_Since_Date";
        private const string TagsKey = "Tags";
        private static CacheService _current;

        private IAsyncStorageService _storage;
        private IApplicationSettingsService _settings;

        public static CacheService Current
        {
            get
            {
                return _current ?? (_current = new CacheService());
            }
        }

        public int CacheTimeout { get; private set; }

        public CacheService()
        {
            _logger = new WPLogger(GetType());
        }

        public void StartService(IAsyncStorageService storage, IApplicationSettingsService settings)
        {
            _storage = storage;
            _settings = settings;

            if (AuthenticationService.Current.IsLoggedIn)
            {
                CheckAndCreateCacheFolder().ConfigureAwait(false);
            }
        }

        public void CreateCacheForUser()
        {
            CheckAndCreateCacheFolder().ConfigureAwait(false);
        }

        public void SetTimeout(int timeout)
        {
            CacheTimeout = timeout;
        }

        public async Task<Tuple<ObservableCollection<ExtendedPocketItem>, bool>> GetRecentItemsFromCache()
        {
            var expired = CacheHasExpired(GetCacheTimeoutKey(RecentKey));
            return new Tuple<ObservableCollection<ExtendedPocketItem>, bool>(await GetItems<ObservableCollection<ExtendedPocketItem>>(GetCacheKey(RecentKey)), expired);
        }

        public async Task SaveRecentItems(ObservableCollection<ExtendedPocketItem> items)
        {
            _settings.Set(GetCacheTimeoutKey(RecentKey), DateTime.Now);
            _settings.Save();
            await SaveItems(GetCacheKey(RecentKey), items);
        }

        public async Task ClearRecentItems()
        {
            await ClearItems(GetCacheKey(RecentKey));
        }

        public async Task<Tuple<ObservableCollection<ExtendedPocketItem>, bool>> GetFavouritesFromCache()
        {
            var expired = CacheHasExpired(GetCacheTimeoutKey(FavouritesKey));
            return new Tuple<ObservableCollection<ExtendedPocketItem>, bool>(await GetItems<ObservableCollection<ExtendedPocketItem>>(GetCacheKey(FavouritesKey)), expired);
        }

        public async Task SaveFavourites(ObservableCollection<ExtendedPocketItem> items)
        {
            _settings.Set(GetCacheTimeoutKey(FavouritesKey), DateTime.Now);
            _settings.Save();
            await SaveItems(GetCacheKey(FavouritesKey), items);
        }

        public async Task ClearFavourites()
        {
            await ClearItems(GetCacheKey(FavouritesKey));
        }

        public async Task<Tuple<ObservableCollection<ExtendedPocketItem>, bool>> GetArchivedItemsFromCache()
        {
            var expired = CacheHasExpired(GetCacheTimeoutKey(ArchivedKey));
            return new Tuple<ObservableCollection<ExtendedPocketItem>, bool>(await GetItems<ObservableCollection<ExtendedPocketItem>>(GetCacheKey(ArchivedKey)), expired);
        }

        public async Task SaveArchivedItems(ObservableCollection<ExtendedPocketItem> items)
        {
            _settings.Set(GetCacheTimeoutKey(ArchivedKey), DateTime.Now);
            _settings.Save();
            await SaveItems(GetCacheKey(ArchivedKey), items);
        }

        public async Task ClearArchivedItems()
        {
            await ClearItems(GetCacheKey(ArchivedKey));
        }

        public async Task<ObservableCollection<ExtendedPocketItem>> GetOfflineFromCache()
        {
            return await GetItems<ObservableCollection<ExtendedPocketItem>>(GetCacheKey(OfflineKey));
        }

        public async Task SaveOffline(ObservableCollection<ExtendedPocketItem> items)
        {
            await SaveItems(GetCacheKey(OfflineKey), items);
        }

        public async Task ClearAllItems()
        {
            await ClearRecentItems();
            await ClearArchivedItems();
            await ClearFavourites();
            InvalidateCache();
            ClearAllSinceDates();
        }

        public void InvalidateCache()
        {
            _settings.Reset(GetCacheTimeoutKey(RecentKey));
            _settings.Reset(GetCacheTimeoutKey(FavouritesKey));
            _settings.Reset(GetCacheTimeoutKey(ArchivedKey));
            _settings.Save();
        }

        public void InvalidateFavouritesCache()
        {
            _settings.Reset(GetCacheTimeoutKey(FavouritesKey));
            _settings.Save();
        }

        public void InvalidateArchiveCache()
        {
            _settings.Reset(GetCacheTimeoutKey(ArchivedKey));
            _settings.Save();
        }

        public void InvalidateQueuedCache()
        {
            _settings.Reset(GetCacheTimeoutKey(RecentKey));
            _settings.Save();
        }

        public DateTime? GetSinceDate(int type)
        {
            var key = string.Format(SinceTime, type);
            key = GetCacheKey(key);
            var since = _settings.Get<DateTime?>(key, null);
            return since;
        }

        public void SetSinceDate(int type, DateTime? date = null)
        {
            var key = string.Format(SinceTime, type);
            key = GetCacheKey(key);
            if (date.HasValue)
            {
                _settings.Set(key, date);
            }
            else
            {
                _settings.Reset(key);
            }

            _settings.Save();
        }

        public void ClearAllSinceDates()
        {
            for (var i = 0; i <= 2; i++)
            {
                SetSinceDate(i);
            }
        }

        public async Task SaveTags(ObservableCollection<string> tags)
        {
            var key = GetCacheKey(TagsKey);
            await SaveItems(key, tags.OrderBy(x => x));
        }

        public async Task<ObservableCollection<string>> GetTags()
        {
            var key = GetCacheKey(TagsKey);
            var tags = await GetItems<ObservableCollection<string>>(key);

            if (tags.IsNullOrEmpty())
            {
                var items = await GetRecentItemsFromCache();
                var queuedTags = items.Item1.AllTagsToList();

                items = await GetFavouritesFromCache();
                var favouriteTags = items.Item1.AllTagsToList();

                items = await GetArchivedItemsFromCache();
                var archiveTags = items.Item1.AllTagsToList();

                queuedTags.AddRange(favouriteTags);
                queuedTags.AddRange(archiveTags);

                tags = queuedTags.Distinct().OrderBy(x => x).ToObservableCollection();

                SaveTags(tags).ConfigureAwait(false);
            }

            return tags.Distinct().ToObservableCollection();
        }

        public async Task ClearTags()
        {
            var key = GetCacheKey(TagsKey);
            if (await _storage.FileExistsAsync(key))
            {
                await _storage.DeleteFileAsync(key);
            }
        }

        public async Task<Tuple<int, int, int>> GetCounts(int? recentExisting = null, int? favouriteExisting = null, int? archivedExisting = null)
        {
            int recentCount, favouritesCount, archivedCount;

            if (!recentExisting.HasValue)
            {
                var recent = await GetRecentItemsFromCache();
                recentCount = recent.Item1 != null ? recent.Item1.Count : 0;
            }
            else
            {
                recentCount = recentExisting.Value;
            }

            if (!favouriteExisting.HasValue)
            {
                var favourites = await GetFavouritesFromCache();
                favouritesCount = favourites.Item1 != null ? favourites.Item1.Count : 0;
            }
            else
            {
                favouritesCount = favouriteExisting.Value;
            }

            if (!archivedExisting.HasValue)
            {
                var archived = await GetArchivedItemsFromCache();
                archivedCount = archived.Item1 != null ? archived.Item1.Count : 0;
            }
            else
            {
                archivedCount = archivedExisting.Value;
            }

            return new Tuple<int, int, int>(recentCount, favouritesCount, archivedCount);
        }

        private string GetCacheKey(string type)
        {
            var cacheFolder = string.Format(CacheFolder, AuthenticationService.Current.LoggedInUser.Username);
            return string.Format("{0}\\{1}_{2}", cacheFolder, AuthenticationService.Current.LoggedInUser.Username, type);
        }

        private async Task CheckAndCreateCacheFolder()
        {
            var cacheFolder = string.Format(CacheFolder, AuthenticationService.Current.LoggedInUser.Username);
            if (!await _storage.DirectoryExistsAsync(cacheFolder))
            {
                await _storage.CreateDirectoryAsync(cacheFolder);
            }
        }

        private string GetCacheTimeoutKey(string type)
        {
            return GetCacheKey(string.Format(CacheTime, type));
        }

        private async Task<T> GetItems<T>(string key)
        {
            if (await _storage.FileExistsAsync(key))
            {
                var json = await _storage.ReadAllTextAsync(key);
                var items = await JsonConvert.DeserializeObjectAsync<T>(json);
                return items;
            }

            return default(T);
        }

        private async Task SaveItems<T>(string key, IEnumerable<T> items)
        {
            if (items == null)
            {
                return;
            }

            try
            {
                if (await _storage.FileExistsAsync(key))
                {
                    await _storage.DeleteFileAsync(key);
                }

                var itemsToSave = items.ToObservableCollection();

                await _storage.WriteAllTextAsync(key, await JsonConvert.SerializeObjectAsync(itemsToSave));
            }
            catch (Exception ex)
            {
                _logger.ErrorException("SaveItems()", ex);
            }
        }

        private async Task ClearItems(string key)
        {
            if (await _storage.FileExistsAsync(key))
            {
                await _storage.DeleteFileAsync(key);
            }
        }

        private bool CacheHasExpired(string key)
        {
            var date = _settings.Get<DateTime?>(key, null);
            var expired = true;
            if (date.HasValue)
            {
                var difference = DateTime.Now - date.Value;
                expired = difference.TotalMinutes > CacheTimeout;
            }
            return expired;
        }
    }
}
