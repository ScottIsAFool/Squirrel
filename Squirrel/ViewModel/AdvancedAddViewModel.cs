using System;
using System.Windows;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using PocketSharp;
using PocketWP;
using ScottIsAFool.WindowsPhone.Extensions;
using ScottIsAFool.WindowsPhone.ViewModel;
using Squirrel.Extensions;
using Squirrel.Resources;
using Squirrel.Services;

namespace Squirrel.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class AdvancedAddViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;
        private readonly IPocketClient _pocketClient;

        /// <summary>
        /// Initializes a new instance of the AdvancedAddViewModel class.
        /// </summary>
        public AdvancedAddViewModel(INavigationService navigationService, IPocketClient pocketClient)
        {
            _navigationService = navigationService;
            _pocketClient = pocketClient;
        }

        public PocketData ExternalItem { get; set; }
        public PocketDataItem PocketDataItem { get; set; }

        public RelayCommand AddItemCommand
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    if (ExternalItem == null || ExternalItem.Items.IsNullOrEmpty())
                    {
                        Log.Debug("ExternalItem or Items is null/empty");
                        return;
                    }

                    if (string.IsNullOrEmpty(PocketDataItem.Uri))
                    {
                        Log.Debug("Item Uri is empty");
                        return;
                    }

                    if (!Uri.IsWellFormedUriString(PocketDataItem.Uri, UriKind.RelativeOrAbsolute))
                    {
                        App.ShowMessage(AppResources.ErrorInvalidLink);
                        return;
                    }

                    SetProgressBar(AppResources.SysTrayAdding);

                    try
                    {
                        await _pocketClient.Add(new Uri(PocketDataItem.Uri, UriKind.Absolute), PocketDataItem.TagsArray, PocketDataItem.Title, PocketDataItem.TweetId);
                        CacheService.Current.InvalidateQueuedCache();

                        if (string.IsNullOrEmpty(ExternalItem.CallbackUri))
                        {
                            if (_navigationService.CanGoBack)
                            {
                                _navigationService.GoBack();
                            }
                            else
                            {
                                _navigationService.NavigateTo(Constants.Pages.MainPage + "?clearbackstack=true");
                            }
                        }
                        else
                        {
                            Windows.System.Launcher.LaunchUriAsync(new Uri(ExternalItem.CallbackUri));
                            Application.Current.Terminate();
                        }
                    }
                    catch (PocketException ex)
                    {
                        ex.Log("AddItemCommand", Log);
                    }
                    catch (Exception ex)
                    {
                        Log.ErrorException("AddItemCommand", ex);
                    }

                    SetProgressBar();
                });
            }
        }

        public RelayCommand AdvancedAddPageLoaded
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (PocketDataItem == null)
                    {
                        PocketDataItem = new PocketDataItem();
                    }
                });
            }
        }

        public override void WireMessages()
        {
            Messenger.Default.Register<NotificationMessage>(this, m =>
            {
                if (m.Notification.Equals(Constants.Messages.DetailsFromExternalMsg))
                {
                    ExternalItem = (PocketData)m.Sender;
                    PocketDataItem = ExternalItem.Items[0];
                }
            });
        }
    }
}