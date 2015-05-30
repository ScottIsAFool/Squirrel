using System;
using System.Windows.Navigation;
using GalaSoft.MvvmLight.Messaging;
using Squirrel.Model;

namespace Squirrel.Views.Login
{
    public partial class AuthorisingView
    {
        public AuthorisingView()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            string authType;
            var type = AuthType.Pocket;
            if (NavigationContext.QueryString.TryGetValue("authtype", out authType))
            {
                type = (AuthType)Enum.Parse(typeof (AuthType), authType, true);
            }

            Messenger.Default.Send(new NotificationMessage(type, Constants.Messages.AuthTypeMsg));
        }
    }
}