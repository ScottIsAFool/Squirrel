using System;
using System.Windows.Media;
using System.Windows.Navigation;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Phone.Shell;
using Squirrel.Extensions;
using Squirrel.Services;
using Squirrel.ViewModel;
using Telerik.Windows.Controls;

namespace Squirrel.Views
{
    public partial class MainPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();
            InteractionEffectManager.AllowedTypes.Add(typeof(RadDataBoundListBoxItem));

            WireMessages();
        }

        private void WireMessages()
        {
            Messenger.Default.Register<NotificationMessage>(this, m =>
            {
                if (m.Notification.Equals(Constants.Messages.ShowTagsMainPageMsg))
                {
                    ApplicationBar.IsVisible = false;
                    TagsWindow.IsOpen = true;
                }

                if (m.Notification.Equals(Constants.Messages.CloseTagsMsg))
                {
                    TagsWindow.IsOpen = false;
                }

                if (m.Notification.Equals(Constants.Messages.SetSysTrayColour))
                {
                    var brush = (SolidColorBrush) m.Sender;
                    SystemTray.SetForegroundColor(this, brush.Color);
                }
            });
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ApplicationBar.Reset();

            if (e.NavigationMode != NavigationMode.Back)
            {
                ReviewReminderService.Current.Notify();
            }
        }

        private void RadDataBoundListBox_OnRefreshRequested(object sender, EventArgs e)
        {
            Messenger.Default.Send(new NotificationMessageAction(Constants.Messages.PullToRefreshMsg, () =>
            {
                var listbox = sender as RadDataBoundListBox;
                if (listbox != null)
                {
                    listbox.StopPullToRefreshLoading(true, true);
                }
            }));
        }

        private void TagsWindow_OnWindowClosed(object sender, WindowClosedEventArgs e)
        {
            ApplicationBar.IsVisible = true;
        }

        private void TagsControl_OnCancelPressed(object sender, EventArgs e)
        {
            TagsWindow.IsOpen = false;
        }

        private void SelectAllClick(object sender, EventArgs e)
        {
            SelectUnselectAllItems(true);
        }

        private void UnselectAllClick(object sender, EventArgs e)
        {
            SelectUnselectAllItems(false);
        }

        private void SelectUnselectAllItems(bool isSelectAll)
        {
            var index = ContentPivot.SelectedIndex;
            RadDataBoundListBox listBox;
            switch (index)
            {
                case MainViewModel.PivotFavourites:
                    listBox = FavouriteList;
                    break;
                case MainViewModel.PivotArchived:
                    listBox = ArchivedList;
                    break;
                default:
                    listBox = QueuedList;
                    break;
            }

            if (isSelectAll)
            {
                listBox.CheckedItems.CheckAll();
            }
            else
            {
                listBox.CheckedItems.Clear();
            }
        }
    }
}