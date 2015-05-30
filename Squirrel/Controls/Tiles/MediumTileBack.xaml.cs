using System.Windows;
using Squirrel.Resources;

namespace Squirrel.Controls.Tiles
{
    public partial class MediumTileBack
    {
        public MediumTileBack()
        {
            InitializeComponent();
            DataContext = this;
        }

        public override void UpdateTile()
        {
            LayoutRoot.Background = Background;
            QueuedItemsCount.Visibility = QueuedCount == 0 ? Visibility.Collapsed : Visibility.Visible;
            QueuedItemsCount.Text = string.Format("Queued: {0}", QueuedCount);

            FavouriteItemsCount.Visibility = FavouriteCount == 0 ? Visibility.Collapsed : Visibility.Visible;
            FavouriteItemsCount.Text = string.Format("{0}: {1}", AppResources.LabelFavourites, FavouriteCount);

            ArchivedItemsCount.Visibility = ArchivedCount == 0 ? Visibility.Collapsed : Visibility.Visible;
            ArchivedItemsCount.Text = string.Format("Archived: {0}", ArchivedCount);
        }
    }
}
