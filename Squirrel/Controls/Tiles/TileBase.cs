using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Squirrel.Controls.Tiles
{
    public abstract class TileBase : UserControl
    {
        public static readonly DependencyProperty QueuedCountProperty =
            DependencyProperty.Register("QueuedCount", typeof (int), typeof (TileBase), new PropertyMetadata(default(int), PropertyChanged));

        public int QueuedCount
        {
            get { return (int) GetValue(QueuedCountProperty); }
            set { SetValue(QueuedCountProperty, value); }
        }

        public static readonly DependencyProperty FavouriteCountProperty =
            DependencyProperty.Register("FavouriteCount", typeof(int), typeof(TileBase), new PropertyMetadata(default(int), PropertyChanged));

        public int FavouriteCount
        {
            get { return (int) GetValue(FavouriteCountProperty); }
            set { SetValue(FavouriteCountProperty, value); }
        }

        public static readonly DependencyProperty ArchivedCountProperty =
            DependencyProperty.Register("ArchivedCount", typeof(int), typeof(TileBase), new PropertyMetadata(default(int), PropertyChanged));

        public int ArchivedCount
        {
            get { return (int) GetValue(ArchivedCountProperty); }
            set { SetValue(ArchivedCountProperty, value); }
        }

        public static readonly DependencyProperty BackgroundProperty =
            DependencyProperty.Register("Background", typeof(SolidColorBrush), typeof(TileBase), new PropertyMetadata(default(SolidColorBrush), PropertyChanged));

        public SolidColorBrush Background
        {
            get { return (SolidColorBrush) GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }

        private static void PropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var tile = sender as TileBase;
            if (tile == null)
            {
                return;
            }

            tile.UpdateTile();
        }

        public virtual void UpdateTile()
        {
        }
    }
}
