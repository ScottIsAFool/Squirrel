using System.Windows.Navigation;
using Squirrel.ViewModel;

namespace Squirrel.Views.Login
{
    public partial class LoginView
    {
        public LoginView()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            string returnUri;
            if (NavigationContext.QueryString.TryGetValue("ReturnUri", out returnUri))
            {
                ((LoginViewModel) DataContext).ReturnUri = returnUri;
            }
        }
    }
}