using System.Windows.Navigation;
using Squirrel.Extensions;

namespace Squirrel.Views
{
    public partial class AdvancedAddView 
    {
        public AdvancedAddView()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ApplicationBar.Reset();
        }
    }
}