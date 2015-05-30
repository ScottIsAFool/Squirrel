using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace Squirrel.Controls
{
    public class FaveOfflineIndicator : Control
    {
        private Path _halfAndHalf;
        private Grid _favouritesFull;
        private Grid _offlineFull;

        public static readonly DependencyProperty IsOfflineProperty =
            DependencyProperty.Register("IsOffline", typeof (bool), typeof (FaveOfflineIndicator), new PropertyMetadata(default(bool), ShowSegments));

        public bool IsOffline
        {
            get { return (bool) GetValue(IsOfflineProperty); }
            set { SetValue(IsOfflineProperty, value); }
        }

        public static readonly DependencyProperty IsFavouriteProperty =
            DependencyProperty.Register("IsFavourite", typeof (bool), typeof (FaveOfflineIndicator), new PropertyMetadata(default(bool), ShowSegments));

        public bool IsFavourite
        {
            get { return (bool) GetValue(IsFavouriteProperty); }
            set { SetValue(IsFavouriteProperty, value); }
        }

        public static readonly DependencyProperty IconHeightProperty =
            DependencyProperty.Register("IconHeight", typeof (int), typeof (FaveOfflineIndicator), new PropertyMetadata(default(int)));

        public int IconHeight
        {
            get { return (int) GetValue(IconHeightProperty); }
            set { SetValue(IconHeightProperty, value); }
        }

        public static readonly DependencyProperty IconWidthProperty =
            DependencyProperty.Register("IconWidth", typeof (int), typeof (FaveOfflineIndicator), new PropertyMetadata(default(int)));

        public int IconWidth
        {
            get { return (int) GetValue(IconWidthProperty); }
            set { SetValue(IconWidthProperty, value); }
        }

        public FaveOfflineIndicator()
        {
            DefaultStyleKey = typeof (FaveOfflineIndicator);
        }

        public override void OnApplyTemplate()
        {
            _halfAndHalf = GetTemplateChild("HalfAndHalf") as Path;
            _favouritesFull = GetTemplateChild("FavouritesFull") as Grid;
            _offlineFull = GetTemplateChild("OfflineFull") as Grid;

            SetUi(this);

            base.OnApplyTemplate();
        }

        private static void ShowSegments(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var foi = sender as FaveOfflineIndicator;
            SetUi(foi);
        }

        private static void SetUi(FaveOfflineIndicator foi)
        {
            if (foi == null || foi._favouritesFull == null /*|| foi._halfAndHalf == null*/ || foi._offlineFull == null)
            {
                return;
            }

            foi._offlineFull.Visibility = /*foi._halfAndHalf.Visibility =*/ foi._favouritesFull.Visibility = Visibility.Collapsed;

            if (foi.IsFavourite && foi.IsOffline)
            {
                //foi._halfAndHalf.Visibility = Visibility.Visible;
                foi._favouritesFull.Visibility = Visibility.Visible;
                foi._offlineFull.Visibility = Visibility.Visible;
            }
            else if (foi.IsFavourite)
            {
                foi._favouritesFull.Visibility = Visibility.Visible;
            }
            else if (foi.IsOffline)
            {
                foi._offlineFull.Visibility = Visibility.Visible;
            }
        }
    }
}
