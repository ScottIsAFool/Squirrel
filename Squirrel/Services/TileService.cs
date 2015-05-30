using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Cimbalino.Phone.Toolkit.Extensions;
using Cimbalino.Phone.Toolkit.Helpers;
using Cimbalino.Phone.Toolkit.Services;
using Squirrel.Controls.Tiles;
using Squirrel.Extensions;
using Squirrel.Model;

namespace Squirrel.Services
{
    public class TileService : ShellTileService, ITileService
    {
        private const int MediumTileSize = 336;
        private const int SmallTileSize = 159;
        private const string MediumTileFront = "shared\\shellcontent\\MediumTileFront.png";
        private const string MediumTileBack = "shared\\shellcontent\\MediumTileBack.png";
        private const string SmallTileFront = "shared\\shellcontent\\SmallTileFront.png";

        private readonly IAsyncStorageService _storageService;
        private static TileService _current;

        public TileService()
        {
            _storageService = new AsyncStorageService();
        }

        public static TileService Current { get { return _current ?? (_current = new TileService()); } }
        public bool IncludeCounts { get; set; }
        
        public void ChangeTileBackground(TileColour colour)
        {
            var tileData = new ShellTileServiceFlipTileData
            {
                Title = ApplicationManifest.Current.App.Title
            };

            switch (colour)
            {
                case TileColour.PhoneAccent:
                    tileData.BackgroundImage = new Uri("/Assets/Tiles/FlipCycleTileMediumTransparent.png", UriKind.Relative);
                    tileData.SmallBackgroundImage = new Uri("/Assets/Tiles/FlipCycleTileSmallTransparent.png", UriKind.Relative);
                    break;
                case TileColour.SquirrelBlue:
                    tileData.BackgroundImage = new Uri("/Assets/Tiles/FlipCycleTileMedium.png", UriKind.Relative);
                    tileData.SmallBackgroundImage = new Uri("/Assets/Tiles/FlipCycleTileSmall.png", UriKind.Relative);
                    break;
                case TileColour.SquirrelAccent:
                    tileData.BackgroundImage = new Uri("/Assets/Tiles/FlipCycleTileMediumAccent.png", UriKind.Relative);
                    tileData.SmallBackgroundImage = new Uri("/Assets/Tiles/FlipCycleTileSmallAccent.png", UriKind.Relative);
                    break;
            }

            var mainTile = ActiveTiles.First();
            mainTile.Update(tileData);
        }

        public async Task UpdateTile(int queuedCount, int favouriteCount, int archiveCount, TileColour tileColour)
        {
            if (!IncludeCounts)
            {
                ChangeTileBackground(tileColour);
                return;
            }

            var background = tileColour.ToBrush();
            var mediumTileBack = new MediumTileBack
            {
                Background = background,
                QueuedCount = queuedCount,
                FavouriteCount = favouriteCount,
                ArchivedCount = archiveCount,
                Height= MediumTileSize, 
                Width = MediumTileSize
            };

            var smallTileFront = new MediumTileFront
            {
                Background = background,
                QueuedCount = queuedCount,
                Height = SmallTileSize,
                Width = SmallTileSize
            };

            await ToImage(smallTileFront, SmallTileFront, SmallTileSize, SmallTileSize);
            await ToImage(smallTileFront, SmallTileFront, SmallTileSize, SmallTileSize);
            await ToImage(mediumTileBack, MediumTileBack, 336, 336);
            await ToImage(mediumTileBack, MediumTileBack, 336, 336);

            smallTileFront.Height = smallTileFront.Width = MediumTileSize;

            await ToImage(smallTileFront, MediumTileFront, 336, 336);
            await ToImage(smallTileFront, MediumTileFront, 336, 336);

            var tile = ActiveTiles.First();
            var tileData = new ShellTileServiceFlipTileData
            {
                BackgroundImage = new Uri("isostore:/" + MediumTileFront, UriKind.Absolute),
                SmallBackgroundImage = new Uri("isostore:/" + SmallTileFront, UriKind.Absolute),
                BackBackgroundImage = new Uri("isostore:/" + MediumTileBack, UriKind.Absolute),
                Title = ApplicationManifest.Current.App.Title
            };

            tile.Update(tileData);
        }

        private async Task ToImage(UIElement element, string filename, double height, double width)
        {
            element.Measure(new Size(width, height));
            element.Arrange(new Rect { Height = height, Width = width });
            element.UpdateLayout();

            var bitmap = new WriteableBitmap((int)width, (int)height);
            bitmap.Render(element, null);
            bitmap.Invalidate();

            await SaveTheImage(bitmap, filename);
        }

        private async Task SaveTheImage(WriteableBitmap bitmap, string filename)
        {
            if (await _storageService.FileExistsAsync(filename))
            {
                await _storageService.DeleteFileAsync(filename);
            }

            using (var fileStream = await _storageService.CreateFileAsync(filename))
            {
                bitmap.SavePng(fileStream);
            }
        }
    }
}
