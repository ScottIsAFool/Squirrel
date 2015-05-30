using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PocketSharp;
using PocketSharp.Models;
using ScottIsAFool.WindowsPhone.Logging;
using ServiceStack.Text;
using Squirrel.Model;
using Squirrel.Services;

namespace Squirrel.Extensions
{
    public static class PocketItemExtensions
    {
        public static ExtendedPocketItem Extend(this PocketItem item)
        {
            var json = JsonConvert.SerializeObject(item);

            var extended = JsonConvert.DeserializeObject<ExtendedPocketItem>(json);

            extended.Excerpt = string.Empty;

            return extended;
        }

        public static async Task FavouriteItem(this PocketItem item, IPocketClient pocketClient, ILog logger, ICacheService cacheService)
        {
            try
            {
                var result = item.IsFavorite
                    ? await pocketClient.Unfavorite(item)
                    : await pocketClient.Favorite(item);

                if (result)
                {
                    item.IsFavorite = !item.IsFavorite;
                    cacheService.InvalidateFavouritesCache();
                }
            }
            catch (PocketException ex)
            {
                ex.Log("FavouriteItem(ex)", logger);
            }
            catch (Exception ex)
            {
                logger.ErrorException("FavouriteItem(this)", ex);
            }
        }

        public static async Task ArchiveItem(this PocketItem item, IPocketClient pocketClient, ILog logger, ICacheService cacheService)
        {
            try
            {
                var result = !item.IsArchive
                    ? await pocketClient.Archive(item)
                    : await pocketClient.Unarchive(item);

                if (result)
                {
                    item.Status = item.IsArchive ? 0 : 1;
                    cacheService.InvalidateArchiveCache();
                    cacheService.InvalidateQueuedCache();
                }
            }
            catch (PocketException ex)
            {
                ex.Log("ArchiveItem(ex)", logger);
            }
            catch (Exception ex)
            {
                logger.ErrorException("ArchiveItem(this)", ex);
            }
        }

        public static async Task DeleteItem(this PocketItem item, IPocketClient pocketClient, ILog logger, ICacheService cacheService, Action action = null)
        {
            try
            {
                var result = await pocketClient.Delete(item);
                if (result)
                {
                    cacheService.InvalidateCache();
                    if (action != null)
                    {
                        action();
                    }
                }
            }
            catch (PocketException ex)
            {
                ex.Log("DeleteItem(ex)", logger);
            }
            catch (Exception ex)
            {
                logger.ErrorException("DeleteItem(this)", ex);
            }
        }
    }
}
