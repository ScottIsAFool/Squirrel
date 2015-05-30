using System.Globalization;
using System.Windows;

namespace Squirrel.Controls.Tiles
{
    public partial class MediumTileFront
    {
        public MediumTileFront()
        {
            InitializeComponent();
            DataContext = this;
        }

        public override void UpdateTile()
        {
            LayoutRoot.Background = Background;
            CountText.Visibility = QueuedCount == 0 ? Visibility.Collapsed : Visibility.Visible;
            CountText.Text = QueuedCount < 1000 ? QueuedCount.ToString(CultureInfo.InvariantCulture) : "+";
        }
    }
}
