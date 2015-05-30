using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Cimbalino.Phone.Toolkit.Services;
using Squirrel.Model;

namespace Squirrel.Services
{
    public interface ICacheService
    {
        int CacheTimeout { get; }
        void StartService(IAsyncStorageService storage, IApplicationSettingsService settings);
        void CreateCacheForUser();
        void SetTimeout(int timeout);
        Task<Tuple<ObservableCollection<ExtendedPocketItem>, bool>> GetRecentItemsFromCache();
        Task SaveRecentItems(ObservableCollection<ExtendedPocketItem> items);
        Task ClearRecentItems();
        Task<Tuple<ObservableCollection<ExtendedPocketItem>, bool>> GetFavouritesFromCache();
        Task SaveFavourites(ObservableCollection<ExtendedPocketItem> items);
        Task ClearFavourites();
        Task<Tuple<ObservableCollection<ExtendedPocketItem>, bool>> GetArchivedItemsFromCache();
        Task SaveArchivedItems(ObservableCollection<ExtendedPocketItem> items);
        Task ClearArchivedItems();
        Task<ObservableCollection<ExtendedPocketItem>> GetOfflineFromCache();
        Task SaveOffline(ObservableCollection<ExtendedPocketItem> items);
        Task ClearAllItems();
        void InvalidateCache();
        void InvalidateFavouritesCache();
        void InvalidateArchiveCache();
        void InvalidateQueuedCache();
        DateTime? GetSinceDate(int type);
        void SetSinceDate(int type, DateTime? date = null);
        void ClearAllSinceDates();
        Task SaveTags(ObservableCollection<string> tags);
        Task<ObservableCollection<string>> GetTags();
        Task ClearTags();
        Task<Tuple<int, int, int>> GetCounts(int? recentExisting, int? favouriteExisting, int? archivedExisting);
    }
}