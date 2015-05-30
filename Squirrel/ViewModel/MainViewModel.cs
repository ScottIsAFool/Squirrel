using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Cimbalino.Phone.Toolkit.Extensions;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using JetBrains.Annotations;
using Microsoft.Phone.Controls;
using PocketSharp;
using PocketSharp.Models;
using ScottIsAFool.WindowsPhone.Extensions;
using Squirrel.Extensions;
using Squirrel.Model;
using Squirrel.Resources;
using Squirrel.Services;
using Telerik.Windows.Controls;
using CustomMessageBox = Squirrel.Controls.CustomMessageBox;
using INavigationService = Squirrel.Services.INavigationService;
using ViewModelBase = ScottIsAFool.WindowsPhone.ViewModel.ViewModelBase;

namespace Squirrel.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        public const int PivotQueued = 0;
        public const int PivotFavourites = 1;
        public const int PivotArchived = 2;
        private const int PivotOffline = 3;

        private readonly INavigationService _navigationService;
        private readonly IPocketClient _pocketClient;

        private bool _recentItemsLoaded;
        private bool _favouriteItemsLoaded;
        private bool _archivedItemsLoaded;

        private IEnumerable<ExtendedPocketItem> _itemsToUpdate;
        private bool _isSingleItem;

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel(INavigationService navigationService, IPocketClient pocketClient)
        {
            _navigationService = navigationService;
            _pocketClient = pocketClient;
            RecentItems = new ObservableCollection<ExtendedPocketItem>();
            FavouritedItems = new ObservableCollection<ExtendedPocketItem>();

            if (IsInDesignMode)
            {
                RecentItems = new ObservableCollection<ExtendedPocketItem>
                {
                    new ExtendedPocketItem
                    {
                        IsOffline = true,
                        AddTime = DateTime.Now,
                        Excerpt = "Oh hell yeah she did!",
                        IsFavorite = true,
                        Title = "Keyara Loses Her Bikini Top in Malibu Pimping 138",
                        FullTitle = "Keyara Loses Her Bikini Top in Malibu Pimping 138",
                        Uri = new Uri("http://egotastic.com")
                    },
                    new ExtendedPocketItem {IsOffline = false, AddTime = DateTime.Now, Excerpt = "This is the excerpt", IsFavorite = true, Title = "My Blog post", FullTitle = "My Blog post"},
                    new ExtendedPocketItem {IsOffline = true, AddTime = DateTime.Now, Excerpt = "This is the excerpt", IsFavorite = false, Title = "My Blog post", FullTitle = "My Blog post"},
                };

                AvailableTags = new ObservableCollection<string>
                {
                    "bikini",
                    "topless",
                    "egotastic",
                    "Lovely"
                };
                IsBoxView = false;
            }
            else
            {
                IsBoxView = App.Settings.IsBoxViewDefault;                
            }

            SelectedIndex = 0;
            SelectedFilter = FilterList.First();
            SelectedSortType = SortTypes.FirstOrDefault(x => x.Value == App.Settings.DefaultSort);
        }

        public ObservableCollection<ExtendedPocketItem> RecentItems { get; set; }
        public ObservableCollection<ExtendedPocketItem> FavouritedItems { get; set; }
        public ObservableCollection<ExtendedPocketItem> ArchivedItems { get; set; }
        public ObservableCollection<ExtendedPocketItem> OfflineItems { get; set; }

        public int SelectedIndex { get; set; }

        public int SelectedAppBarIndex
        {
            get
            {
                if (IsInMultiSelectArchive || IsInMultiSelectFavourites || IsInMultiSelectRecent)
                {
                    if (SelectedIndex == PivotQueued && IsInMultiSelectRecent)
                    {
                        return 1;
                    }

                    if (SelectedIndex == PivotFavourites && IsInMultiSelectFavourites)
                    {
                        return 2;
                    }

                    if (SelectedIndex == PivotArchived && IsInMultiSelectArchive)
                    {
                        return 3;
                    }

                    return 0;
                }

                return 0;
            }
        }
        public bool IsInMultiSelectRecent { get; set; }
        public bool IsInMultiSelectFavourites { get; set; }
        public bool IsInMultiSelectArchive { get; set; }
        public bool IsBoxView { get; set; }
        public bool PullToRefreshLoading { get; set; }

        public List<ExtendedPocketItem> SelectedItemsRecent { get; set; }
        public List<ExtendedPocketItem> SelectedItemsFavourite { get; set; }
        public List<ExtendedPocketItem> SelectedItemsArchive { get; set; }
        public string SelectedFilter { get; set; }
        public KeyValuePair<string, SortType> SelectedSortType { get; set; }

        public ObservableCollection<string> AvailableTags { get; set; }
        public ObservableCollection<string> SelectedTags { get; set; }
        public string TagText { get; set; }

        public bool CanAddTag
        {
            get
            {
                return !string.IsNullOrEmpty(TagText);
            }
        }

        public List<string> FilterList
        {
            get
            {
                return new List<string>
                {
                    AppResources.LabelAllItems,
                    AppResources.LabelArticles,
                    AppResources.LabelVideos,
                    AppResources.LabelImages
                };
            }
        }

        public Dictionary<string, SortType> SortTypes
        {
            get
            {
                return new Dictionary<string, SortType>
                {
                    {AppResources.LabelAtoZ, SortType.TitleAscending},
                    {AppResources.LabelZtoA, SortType.TitleDescending},
                    {AppResources.LabelOldest, SortType.DateAscending},
                    {AppResources.LabelNewest, SortType.DateDescending}
                };
            }
        }

        public bool CanRefresh
        {
            get { return SelectedIndex != 3; }
        }

        public RelayCommand<List<ExtendedPocketItem>> MultiTagsCommand
        {
            get
            {
                return new RelayCommand<List<ExtendedPocketItem>>(items =>
                {
                    _isSingleItem = false;
                    _itemsToUpdate = items;
                    Messenger.Default.Send(new NotificationMessage(Constants.Messages.ShowTagsMainPageMsg));
                });
            }
        }

        public RelayCommand<ExtendedPocketItem> ChangeTagsCommand
        {
            get
            {
                return new RelayCommand<ExtendedPocketItem>(item =>
                {
                    _isSingleItem = true;
                    _itemsToUpdate = new List<ExtendedPocketItem>
                    {
                        item
                    };

                    if (!item.Tags.IsNullOrEmpty())
                    {
                        var itemTags = item.Tags.Select(x => x.Name).ToList();
                        var tags = AvailableTags.Where(itemTags.Contains).ToList();
                        Messenger.Default.Send(new NotificationMessage(tags, Constants.Messages.AddTagToSelectedMsg));
                    }
                    Messenger.Default.Send(new NotificationMessage(Constants.Messages.ShowTagsMainPageMsg));
                });
            }
        }

        public RelayCommand ChangeViewCommand
        {
            get
            {
                return new RelayCommand(() => IsBoxView = !IsBoxView);
            }
        }

        public RelayCommand SignOutCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    AuthenticationService.Current.SignOut();
                    _navigationService.NavigateTo(Constants.Pages.Login.LoginView + "?clearbackstack=true");
                });
            }
        }

        public RelayCommand NavigateToSettings
        {
            get
            {
                return new RelayCommand(() => _navigationService.NavigateTo(Constants.Pages.SettingsView));
            }
        }

        public RelayCommand NavigateToAdd
        {
            get
            {
                return new RelayCommand(() => _navigationService.NavigateTo(Constants.Pages.AdvancedAddView));
            }
        }

        public RelayCommand NavigateToAbout
        {
            get
            {
                return new RelayCommand(() => _navigationService.NavigateTo(Constants.Pages.AboutView));
            }
        }

        public RelayCommand<ExtendedPocketItem> NavigateToArticleCommand
        {
            get
            {
                return new RelayCommand<ExtendedPocketItem>(article =>
                {
                    ObservableCollection<ExtendedPocketItem> selectedList;
                    switch (SelectedIndex)
                    {
                        case PivotFavourites:
                            selectedList = FavouritedItems;
                            break;
                        case PivotArchived:
                            selectedList = ArchivedItems;
                            break;
                        default:
                            selectedList = RecentItems;
                            break;
                    }

                    Messenger.Default.Send(new NotificationMessage(article, selectedList, Constants.Messages.ArticleLoadMsg));
                    _navigationService.NavigateTo(Constants.Pages.Article.ArticleView);
                });
            }
        }

        public RelayCommand RefreshCommand
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    await GetData(true);
                });
            }
        }

        public RelayCommand MainPageLoaded
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    await GetData(App.Settings.RefreshOnOpen);
                });
            }
        }

        public RelayCommand NavigateToSearchCommand
        {
            get
            {
                return new RelayCommand(() => _navigationService.NavigateTo(Constants.Pages.Search.SearchView));
            }
        }

        public RelayCommand AddToFavouritesQueuedCommand
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    await AddRemoveFavouritesInternal(true, SelectedItemsRecent);

                    SelectedItemsRecent = new List<ExtendedPocketItem>();
                });
            }
        }

        public RelayCommand AddToFavouritesArchivedCommand
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    await AddRemoveFavouritesInternal(true, SelectedItemsArchive);

                    SelectedItemsFavourite = new List<ExtendedPocketItem>();
                });
            }
        }

        public RelayCommand RemoveFromFavouritesCommand
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    await AddRemoveFavouritesInternal(false, SelectedItemsFavourite);

                    SelectedItemsArchive = new List<ExtendedPocketItem>();
                });
            }
        }

        public RelayCommand ArchiveQueuedCommand
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    await ArchiveItemsInternal(true, SelectedItemsRecent);

                    SelectedItemsRecent = new List<ExtendedPocketItem>();
                });
            }
        }

        public RelayCommand ArchiveFavouritesCommand
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    await ArchiveItemsInternal(true, SelectedItemsFavourite);

                    SelectedItemsFavourite = new List<ExtendedPocketItem>();
                });
            }
        }

        public RelayCommand UnArchiveCommand
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    await ArchiveItemsInternal(false, SelectedItemsArchive);

                    SelectedItemsArchive = new List<ExtendedPocketItem>();
                });
            }
        }

        public RelayCommand<ExtendedPocketItem> CopyToClipboardCommand
        {
            get
            {
                return new RelayCommand<ExtendedPocketItem>(item =>
                {
                    if (item == null)
                    {
                        return;
                    }

                    Clipboard.SetText(item.Uri.ToString());
                });
            }
        }

        public RelayCommand<List<ExtendedPocketItem>> DeleteItemsCommand
        {
            get
            {
                return new RelayCommand<List<ExtendedPocketItem>>(async selectedItems =>
                {
                    if (selectedItems.IsNullOrEmpty())
                    {
                        return;
                    }

                    if (!App.Settings.DontPromptForDeletion)
                    {
                        var question = new CustomMessageBox
                        {
                            Message = AppResources.MessageDeleteItem,
                            Title = AppResources.MessageTitleAreYouSure,
                            LeftButtonContent = AppResources.LabelYes,
                            RightButtonContent = AppResources.LabelNo,
                            Content = Utils.CreateDontShowCheckBox("DontPromptForDeletion")
                        };
                        var result = await question.ShowAsync();
                        if (result == CustomMessageBoxResult.RightButton)
                        {
                            return;
                        }
                    }

                    SetProgressBar(AppResources.SysTrayDeleting);

                    foreach (var item in selectedItems)
                    {
                        await DeleteItem(item);
                    }

                    CacheService.Current.InvalidateCache();

                    selectedItems = new List<ExtendedPocketItem>();

                    await GetData(true);

                    SetProgressBar();
                });
            }
        }

        public RelayCommand QuickAddCommand
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    var inputScope = new InputScope();
                    var name = new InputScopeName {NameValue = InputScopeNameValue.Url};
                    inputScope.Names.Add(name);

                    var textbox = new RadTextBox
                    {
                        Watermark = AppResources.WatermarkEnterLink,
                        VerticalAlignment = VerticalAlignment.Top,
                        ActionButtonVisibility = Visibility.Collapsed,
                        InputScope = inputScope
                    };

                    var messageBox = new CustomMessageBox
                    {
                        LeftButtonContent = AppResources.AppBarAdd,
                        RightButtonContent = AppResources.LabelCancel,
                        Title = AppResources.MessageTitleAddALink,
                        Content = textbox,
                        Background = (SolidColorBrush)Application.Current.Resources["SquirrelAltBackgroundBrush"],
                        Foreground = (SolidColorBrush)Application.Current.Resources["SquirrelBackgroundBrush"],
                        BorderThickness = new Thickness(2),
                        BorderBrush = (SolidColorBrush)Application.Current.Resources["SquirrelBackgroundBrush"]
                    };

                    Messenger.Default.Send(new NotificationMessage(Application.Current.Resources["SquirrelBackgroundBrush"], Constants.Messages.SetSysTrayColour));

                    var result = await messageBox.ShowAsync();

                    Messenger.Default.Send(new NotificationMessage(Application.Current.Resources["PhoneForegroundBrush"], Constants.Messages.SetSysTrayColour));

                    if (result == CustomMessageBoxResult.RightButton)
                    {
                        return;
                    }

                    var link = textbox.Text;
                    if (string.IsNullOrEmpty(link))
                    {
                        return;
                    }

                    if (!link.StartsWith("http"))
                    {
                        link = "http://" + link;
                    }

                    if (!Uri.IsWellFormedUriString(link, UriKind.RelativeOrAbsolute))
                    {
                        App.ShowMessage(AppResources.ErrorInvalidLink);
                        return;
                    }

                    try
                    {
                        SetProgressBar(AppResources.SysTrayAdding);
                        var response = await _pocketClient.Add(new Uri(link));
                        var item = response.Extend();

                        if (SelectedSortType.Value == SortType.TitleAscending || SelectedSortType.Value == SortType.TitleDescending)
                        {
                            var mergedItems = RecentItems.ToObservableCollection();
                            mergedItems.Add(item);

                            RecentItems = await mergedItems.Sort(SelectedSortType.Value);
                        }
                        else
                        {
                            if (SelectedSortType.Value == SortType.DateDescending)
                            {
                                RecentItems.Insert(0, item);
                            }
                            else if (SelectedSortType.Value == SortType.DateAscending)
                            {
                                RecentItems.Insert(RecentItems.Count - 1, item);
                            }
                        }

                        await CacheService.Current.SaveRecentItems(RecentItems);
                        OfflineService.Current.AddToDownloadQueue(item, App.Settings.DownloadFilesInBackground, App.CancellationToken.Token).ConfigureAwait(false);
                    }
                    catch (PocketException ex)
                    {
                        ex.Log("QuickAddCommand", Log);
                    }
                    catch (Exception ex)
                    {
                        Log.ErrorException("QuickAddCommand", ex);
                    }

                    SetProgressBar();
                });
            }
        }

        public RelayCommand<ExtendedPocketItem> DeleteCommand
        {
            get
            {
                return new RelayCommand<ExtendedPocketItem>(async item =>
                {
                    if (item == null)
                    {
                        return;
                    }

                    if (!App.Settings.DontPromptForDeletion)
                    {
                        var question = new CustomMessageBox
                        {
                            Message = AppResources.MessageDeleteItem,
                            Title = AppResources.MessageTitleAreYouSure,
                            LeftButtonContent = AppResources.LabelYes,
                            RightButtonContent = AppResources.LabelNo,
                            Content = Utils.CreateDontShowCheckBox("DontPromptForDeletion")
                        };
                        var result = await question.ShowAsync();
                        if (result == CustomMessageBoxResult.RightButton)
                        {
                            return;
                        }
                    }

                    SetProgressBar("Deleting...");

                    await item.DeleteItem(_pocketClient, Log, CacheService.Current, async () =>
                    {
                        CacheService.Current.InvalidateCache();
                        await GetData(true);
                    });

                    SetProgressBar();
                });
            }
        }

        public RelayCommand CreateNewTagCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (string.IsNullOrEmpty(TagText))
                    {
                        return;
                    }

                    if (SelectedTags == null)
                    {
                        SelectedTags = new ObservableCollection<string>();
                    }

                    if (AvailableTags == null)
                    {
                        AvailableTags = new ObservableCollection<string>();
                    }

                    AvailableTags.Add(TagText);
                    SelectedTags.Add(TagText);

                    Messenger.Default.Send(new NotificationMessage(new List<string> { TagText }, Constants.Messages.AddTagToSelectedMsg));

                    TagText = string.Empty;

                    CacheService.Current.SaveTags(AvailableTags).ConfigureAwait(false);
                });
            }
        }

        public RelayCommand UpdateTagsCommand
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    if (_itemsToUpdate.IsNullOrEmpty() || !await _navigationService.IsNetworkAvailable())
                    {
                        return;
                    }

                    SetProgressBar(AppResources.SysTraypUpdatingTags);

                    try
                    {
                        foreach (var item in _itemsToUpdate)
                        {
                            await _pocketClient.AddTags(item, SelectedTags.ToArray());
                        }

                        if (_isSingleItem)
                        {
                            var tagsToRemove = _itemsToUpdate.First().Tags.Where(x => !SelectedTags.Contains(x.Name)).Select(y => y.Name).ToArray();
                            if (!tagsToRemove.IsNullOrEmpty())
                            {
                                await _pocketClient.RemoveTags(_itemsToUpdate.First(), tagsToRemove);
                            }
                        }

                        CacheService.Current.InvalidateCache();

                        IsInMultiSelectRecent = IsInMultiSelectFavourites = IsInMultiSelectArchive = false;

                        Messenger.Default.Send(new NotificationMessage(Constants.Messages.CloseTagsMsg));

                        App.ShowMessage(AppResources.MessageTagsUpdated);
                    }
                    catch (PocketException ex)
                    {
                        ex.Log("UpdateTagsCommand", Log);
                    }
                    catch (Exception ex)
                    {
                        Log.ErrorException("UpdateTagsCommand", ex);
                    }

                    SetProgressBar();
                });
            }
        }

        public RelayCommand<ExtendedPocketItem> MakeOfflineCommand
        {
            get
            {
                return new RelayCommand<ExtendedPocketItem>(item => OfflineService.Current.AddToDownloadQueue(item, App.Settings.DownloadFilesInBackground, App.CancellationToken.Token));
            }
        }

        public RelayCommand<ExtendedPocketItem> FavouriteCommand
        {
            get
            {
                return new RelayCommand<ExtendedPocketItem>(async item =>
                {
                    if (item == null || !await _navigationService.IsNetworkAvailable())
                    {
                        return;
                    }

                    SetProgressBar(item.IsFavorite ? AppResources.SysTrayRemovingFromFavourites : AppResources.SysTrayAddingToFavourites);

                    await item.FavouriteItem(_pocketClient, Log, CacheService.Current);

                    await ProcessFavourite(item);

                    SetProgressBar();

                    await GetData(false);
                });
            }
        }

        private async Task ProcessFavourite(ExtendedPocketItem item)
        {
            if (!item.IsFavorite)
            {
                FavouritedItems.Remove(item);
            }

            await CacheService.Current.SaveFavourites(FavouritedItems);
        }

        public RelayCommand<ExtendedPocketItem> ArchiveCommand
        {
            get
            {
                return new RelayCommand<ExtendedPocketItem>(async item =>
                {
                    if (item == null || !await _navigationService.IsNetworkAvailable())
                    {
                        return;
                    }

                    SetProgressBar(item.IsArchive ? AppResources.SysTrayRestoring : AppResources.SysTrayArchiving);

                    await item.ArchiveItem(_pocketClient, Log, CacheService.Current);

                    if (item.IsArchive)
                    {
                        var archivedItem = RecentItems.FirstOrDefault(x => x.ResolvedId == item.ResolvedId);
                        if (archivedItem != null)
                        {
                            RecentItems.Remove(archivedItem);
                        }
                    }
                    else
                    {
                        var nonArchivedItem = ArchivedItems.FirstOrDefault(x => x.ResolvedId == item.ResolvedId);
                        if (nonArchivedItem != null)
                        {
                            ArchivedItems.Remove(nonArchivedItem);
                        }
                    }

                    await CacheService.Current.SaveRecentItems(RecentItems);

                    SetProgressBar();

                    await GetData(false);
                });
            }
        }

        public override void WireMessages()
        {
            Messenger.Default.Register<NotificationMessageAction>(this, async m =>
            {
                if (m.Notification.Equals(Constants.Messages.PullToRefreshMsg))
                {
                    PullToRefreshLoading = true;

                    await GetData(true);

                    PullToRefreshLoading = false;

                    m.Execute();
                }
            });

            Messenger.Default.Register<NotificationMessage>(this, async m =>
            {
                if (m.Notification.Equals(Constants.Messages.ClearCacheMsg))
                {
                    RecentItems = null;
                    FavouritedItems = null;
                    ArchivedItems = null;
                }

                if (m.Notification.Equals(Constants.Messages.RemoveFavouriteMsg))
                {
                    await ProcessFavourite((ExtendedPocketItem) m.Sender);
                }

                if (m.Notification.Equals(Constants.Messages.SaveToCacheMsg))
                {
                    SaveAllItems();
                }
            });
        }

        private void SaveAllItems()
        {
            CacheService.Current.SaveRecentItems(RecentItems).ConfigureAwait(false);
            CacheService.Current.SaveFavourites(FavouritedItems).ConfigureAwait(false);
            CacheService.Current.SaveArchivedItems(ArchivedItems).ConfigureAwait(false);
        }

        private async Task DeleteItem(ExtendedPocketItem item)
        {
            try
            {
                await _pocketClient.Delete(item);
            }
            catch (PocketException ex)
            {
                ex.Log("DeleteItem()", Log);
            }
            catch (Exception ex)
            {
                Log.ErrorException("DeleteItem()", ex);
            }
        }

        private async Task ArchiveItemsInternal(bool isArchive, List<ExtendedPocketItem> selectedItems)
        {
            if (selectedItems.IsNullOrEmpty())
            {
                return;
            }

            SetProgressBar(isArchive ? AppResources.SysTrayArchiving :AppResources.SysTrayRemovingFromArchive);

            if (isArchive)
            {
                var itemsToArchive = selectedItems.Where(x => !x.IsArchive).ToList();
                foreach (var item in itemsToArchive)
                {
                    await AddRemoveArchive(item, true);
                }
            }
            else
            {
                var itemsToUnarchive = selectedItems.Where(x => x.IsArchive).ToList();
                foreach (var item in itemsToUnarchive)
                {
                    await AddRemoveArchive(item, false);
                }
            }

            CacheService.Current.InvalidateArchiveCache();

            SetProgressBar();
        }

        private async Task AddRemoveArchive(ExtendedPocketItem item, bool isArchive)
        {
            try
            {
                var result = isArchive
                    ? await _pocketClient.Archive(item)
                    : await _pocketClient.Unarchive(item);

                if (result)
                {
                    item.Status = item.IsArchive ? 0 : 1;
                }
            }
            catch (PocketException ex)
            {
                ex.Log("AddRemoveArchive(" + isArchive + ")", Log);
            }
            catch (Exception ex)
            {
                Log.ErrorException("AddRemoveArchive()", ex);
            }

            SetProgressBar();
        }

        private async Task AddRemoveFavouritesInternal(bool isAdd, List<ExtendedPocketItem> selectedItems)
        {
            if (selectedItems.IsNullOrEmpty())
            {
                return;
            }

            SetProgressBar(isAdd ? AppResources.SysTrayRemovingFromFavourites : AppResources.SysTrayAddingToFavourites);

            if (isAdd)
            {
                var itemsToAdd = selectedItems.Where(x => !x.IsFavorite).ToList();
                foreach (var item in itemsToAdd)
                {
                    await AddRemoveFavourite(item, true);
                }
            }
            else
            {
                var itemsToRemove = selectedItems.Where(x => x.IsFavorite).ToList();
                foreach (var item in itemsToRemove)
                {
                    await AddRemoveFavourite(item, false);

                    FavouritedItems.Remove(item);
                }
            }

            SetProgressBar();
        }

        private async Task AddRemoveFavourite(ExtendedPocketItem item, bool isAdd)
        {
            try
            {
                var result = isAdd
                    ? await _pocketClient.Unfavorite(item)
                    : await _pocketClient.Favorite(item);

                if (result)
                {
                    item.IsFavorite = !item.IsFavorite;
                    CacheService.Current.InvalidateFavouritesCache();
                }
            }
            catch (PocketException ex)
            {
                ex.Log("AddRemoveFavourite(" + isAdd + ")", Log);
            }
            catch (Exception ex)
            {
                Log.ErrorException("AddRemoveFavourite()", ex);
            }
        }

        private async Task SortItems()
        {
            SetProgressBar(AppResources.SysTraySorting);
            var sort = SelectedSortType.Value;
            switch (SelectedIndex)
            {
                case PivotQueued:
                    RecentItems = await RecentItems.Sort(sort);
                    break;
                case PivotFavourites:
                    FavouritedItems = await FavouritedItems.Sort(sort);
                    break;
                case PivotArchived:
                    ArchivedItems = await ArchivedItems.Sort(sort);
                    break;
                case PivotOffline:
                    OfflineItems = await OfflineItems.Sort(sort);
                    break;
            }

            SetProgressBar();
        }

        [UsedImplicitly]
        private async void OnSelectedSortTypeChanged()
        {
            await SortItems();
        }

        [UsedImplicitly]
        private async void OnSelectedIndexChanged()
        {
            if (IsInDesignMode)
            {
                return;
            }

            await GetData(false);
        }

        [UsedImplicitly]
        private void OnSelectedFilterChanged()
        {
            if (!RecentItems.IsNullOrEmpty())
            {
                foreach (var item in RecentItems)
                {
                    item.SetVisibility(SelectedFilter);
                }
            }

            if (!FavouritedItems.IsNullOrEmpty())
            {
                foreach (var item in FavouritedItems)
                {
                    item.SetVisibility(SelectedFilter);
                }
            }

            if (!ArchivedItems.IsNullOrEmpty())
            {
                foreach (var item in ArchivedItems)
                {
                    item.SetVisibility(SelectedFilter);
                }
            }
        }

        private async Task GetData(bool isRefresh)
        {
            AuthenticationService.Current.SetAccessToken(_pocketClient);

            SetProgressBar(AppResources.SysTrayLoadingItems);

            if (AvailableTags.IsNullOrEmpty())
            {
                AvailableTags = await CacheService.Current.GetTags();
            }

            ObservableCollection<ExtendedPocketItem> items;
            Tuple<ObservableCollection<ExtendedPocketItem>, bool> cacheResponse;
            bool cacheExpired;
            bool goToWeb;
            switch (SelectedIndex)
            {
                case 0: // Queued
                    Log.Info("Refreshing queued items");
                    cacheResponse = await CacheService.Current.GetRecentItemsFromCache();
                    items = cacheResponse.Item1;
                    cacheExpired = cacheResponse.Item2;

                    Log.Info("Cache contained {0} items", items.IsNullOrEmpty() ? 0 : items.Count);

                    if (RecentItems.IsNullOrEmpty())
                    {
                        RecentItems = items;
                    }

                    _recentItemsLoaded = !items.IsNullOrEmpty();
                    goToWeb = cacheExpired || !_recentItemsLoaded || isRefresh;

                    if (goToWeb || items.IsNullOrEmpty())
                    {
                        Log.Info("Checking web for new items");
                        _recentItemsLoaded = await GetQueuedItems();
                    }

                    if (!_recentItemsLoaded && RecentItems.IsNullOrEmpty())
                    {
                        RecentItems = items;
                    }

                    UpdateTile().ConfigureAwait(false);

                    foreach (var item in RecentItems.Where(x => !x.IsOffline))
                    {
                        item.IsOffline = await OfflineService.Current.ArticleIsOffline(item.ResolvedId);
                    }

                    await CacheService.Current.SaveRecentItems(RecentItems);
                    break;
                case 1: // Favourites
                    cacheResponse = await CacheService.Current.GetFavouritesFromCache();
                    items = cacheResponse.Item1;
                    cacheExpired = cacheResponse.Item2;
                    if (FavouritedItems.IsNullOrEmpty())
                    {
                        FavouritedItems = items;
                    }

                    _favouriteItemsLoaded = !items.IsNullOrEmpty();
                    goToWeb = cacheExpired || !_favouriteItemsLoaded || isRefresh;
                    if (goToWeb || items.IsNullOrEmpty())
                    {
                        _favouriteItemsLoaded = await GetFavouritedItems();
                    }

                    if (!_favouriteItemsLoaded && FavouritedItems.IsNullOrEmpty())
                    {
                        FavouritedItems = items;
                    }

                    UpdateTile().ConfigureAwait(false);

                    foreach (var item in FavouritedItems.Where(x => !x.IsOffline))
                    {
                        item.IsOffline = await OfflineService.Current.ArticleIsOffline(item.ResolvedId);
                    }

                    await CacheService.Current.SaveFavourites(FavouritedItems);
                    break;
                case 2: // Archived
                    cacheResponse = await CacheService.Current.GetArchivedItemsFromCache();
                    items = cacheResponse.Item1;
                    cacheExpired = cacheResponse.Item2;
                    if (ArchivedItems.IsNullOrEmpty())
                    {
                        ArchivedItems = items;
                    }

                    _archivedItemsLoaded = !items.IsNullOrEmpty();
                    goToWeb = cacheExpired || !_archivedItemsLoaded || isRefresh;
                    if (goToWeb || items.IsNullOrEmpty())
                    {
                        _archivedItemsLoaded = await GetArchivedItems();
                    }

                    if (!_archivedItemsLoaded && ArchivedItems.IsNullOrEmpty())
                    {
                        ArchivedItems = items;
                    }

                    UpdateTile().ConfigureAwait(false);

                    break;
            }

            SetProgressBar();
        }

        private async Task UpdateTile()
        {
            int? q = null, f = null, a = null;
            if (!RecentItems.IsNullOrEmpty()) q = RecentItems.Count;
            if (!FavouritedItems.IsNullOrEmpty()) f = FavouritedItems.Count;
            if (!ArchivedItems.IsNullOrEmpty()) a = ArchivedItems.Count;

            var counts = await CacheService.Current.GetCounts(q, f, a);

            TileService.Current.UpdateTile(
                counts.Item1, 
                counts.Item2, 
                counts.Item3,
                App.Settings.TileColour).ConfigureAwait(false);
        }

        private async Task<bool> GetFavouritedItems()
        {
            if (!await _navigationService.IsNetworkAvailable())
            {
                return false;
            }

            var result = false;
            SetProgressBar(AppResources.SysTrayGettingFavourites);

            try
            {
                var since = CacheService.Current.GetSinceDate(PivotFavourites);
                var response = await _pocketClient.Get(since: since, favorite: true);
                var removedResponse = await _pocketClient.Get(since: since, favorite: false);
                var items = response.Select(x => x.Extend()).ToList();
                var removedItems = removedResponse.Select(x => x.Extend()).ToList();

                var noFavourites = FavouritedItems.IsNullOrEmpty();
                foreach (var item in removedItems)
                {
                    if (noFavourites)
                    {
                        break;
                    }

                    foreach (var r in FavouritedItems.ToList())
                    {
                        if (r.ID == item.ID)
                        {
                            FavouritedItems.Remove(r);
                        }
                    }

                    items.Remove(item);
                }
                
                foreach (var a in items.ToList())
                {
                    if (noFavourites)
                    {
                        break;
                    }

                    if (!FavouritedItems.IsNullOrEmpty())
                    {
                        var existingItem = FavouritedItems.FirstOrDefault(x => x.ResolvedId == a.ResolvedId);
                        if (existingItem != null)
                        {
                            Utils.CopyItem(a, existingItem);
                            items.Remove(a);
                        }
                    }
                }

                var tags = items.AllTagsToList();

                if (!tags.IsNullOrEmpty())
                {
                    AvailableTags.AddRange(tags);
                    CacheService.Current.SaveTags(AvailableTags).ConfigureAwait(false);
                }

                OfflineService.Current.AddToDownloadQueue(items, App.Settings.DownloadFilesInBackground, App.CancellationToken.Token).ConfigureAwait(false);

                if (noFavourites)
                {
                    FavouritedItems = await items.Sort(App.Settings.DefaultSort);
                }
                else
                {
                    if (SelectedSortType.Value == SortType.TitleAscending || SelectedSortType.Value == SortType.TitleDescending)
                    {
                        var mergedItems = items.Union(FavouritedItems).ToObservableCollection();
                        FavouritedItems = await mergedItems.Sort(SelectedSortType.Value);
                    }
                    else
                    {
                        var i = 0;
                        foreach (var item in items)
                        {
                            if (SelectedSortType.Value == SortType.DateDescending)
                            {
                                FavouritedItems.Insert(i, item);
                            }
                            else if (SelectedSortType.Value == SortType.DateAscending)
                            {
                                FavouritedItems.Insert(FavouritedItems.Count - 1, item);
                            }

                            i++;
                        }
                    }
                }

                await CacheService.Current.SaveFavourites(FavouritedItems);
                CacheService.Current.SetSinceDate(PivotFavourites, DateTime.Now);
                result = true;
            }
            catch (PocketException ex)
            {
                ex.Log("GetFavouritedItems()", Log);
            }
            catch (Exception ex)
            {
                Log.ErrorException("GetFavouritedItems()", ex);
            }

            SetProgressBar();

            return result;
        }

        private async Task<bool> GetQueuedItems()
        {
            if (!await _navigationService.IsNetworkAvailable())
            {
                return false;
            }

            var result = false;
            SetProgressBar(AppResources.SysTrayGettingQueuedItems);

            try
            {
                var since = CacheService.Current.GetSinceDate(PivotQueued);
                var response = await _pocketClient.Get(since: since);
                var items = response.Select(x => x.Extend()).ToList();

                Log.Info("Brought back {0} items", items.Count);

                var noRecentItems = RecentItems.IsNullOrEmpty();

                Log.Info("Deleting {0} items", items.Count(x => x.IsDeleted));

                // Handle deleted items here
                foreach (var item in items.Where(x => x.IsDeleted).ToList())
                {
                    if (noRecentItems)
                    {
                        break;
                    }

                    foreach (var r in RecentItems.ToList())
                    {
                        if (r.ID == item.ID)
                        {
                            RecentItems.Remove(r);
                        }
                    }

                    items.Remove(item);
                }

                
                Log.Info("Archiving {0} items", items.Count(x => x.IsArchive));
                foreach (var item in items.Where(x => x.IsArchive).ToList())
                {
                    if (noRecentItems)
                    {
                        break;
                    }

                    foreach (var r in RecentItems.ToList())
                    {
                        if (r.ID == item.ID)
                        {
                            RecentItems.Remove(r);
                        }
                    }

                    items.Remove(item);
                }

                foreach (var a in items.ToList())
                {
                    if (noRecentItems)
                    {
                        break;
                    }

                    var existingItem = RecentItems.FirstOrDefault(x => x.ResolvedId == a.ResolvedId);
                    if (existingItem != null)
                    {
                        Utils.CopyItem(a, existingItem);
                        items.Remove(a);
                    }
                }

                var tags = items.AllTagsToList();

                if (!tags.IsNullOrEmpty())
                {
                    AvailableTags.AddRange(tags);
                    CacheService.Current.SaveTags(AvailableTags).ConfigureAwait(false);
                }

                Log.Info("Adding new items to download queue");
                OfflineService.Current.AddToDownloadQueue(items, App.Settings.DownloadFilesInBackground, App.CancellationToken.Token).ConfigureAwait(false);

                Log.Info("Adding new items to UI");
                if (noRecentItems)
                {
                    RecentItems = await items.Sort(App.Settings.DefaultSort);
                }
                else
                {
                    if (SelectedSortType.Value == SortType.TitleAscending || SelectedSortType.Value == SortType.TitleDescending)
                    {
                        var mergedItems = items.Union(RecentItems).ToObservableCollection();
                        RecentItems = await mergedItems.Sort(SelectedSortType.Value);
                    }
                    else
                    {
                        var i = 0;

                        foreach (var item in items)
                        {
                            if (SelectedSortType.Value == SortType.DateDescending)
                            {
                                RecentItems.Insert(i, item);
                            }
                            else if (SelectedSortType.Value == SortType.DateAscending)
                            {
                                RecentItems.Insert(RecentItems.Count - 1, item);
                            }

                            i++;
                        }
                    }
                }

                await CacheService.Current.SaveRecentItems(RecentItems);
                CacheService.Current.SetSinceDate(PivotQueued, DateTime.Now);
                result = true;
            }
            catch (PocketException ex)
            {
                ex.Log("GetQueuedItems()", Log);
            }
            catch (Exception ex)
            {
                Log.ErrorException("GetQueuedItems()", ex);
            }

            SetProgressBar();

            return result;
        }

        private async Task<bool> GetArchivedItems()
        {
            if (!await _navigationService.IsNetworkAvailable())
            {
                return false;
            }

            var result = false;
            SetProgressBar(AppResources.SysTrayGettingArchivedItems);

            try
            {
                var since = CacheService.Current.GetSinceDate(PivotArchived);
                var response = await _pocketClient.Get(State.archive, since: since);
                var items = response.Select(x => x.Extend()).ToList();

                var noArchives = ArchivedItems.IsNullOrEmpty();
                foreach (var item in items.Where(x => !x.IsArchive).ToList())
                {
                    if (noArchives)
                    {
                        break;
                    }

                    foreach (var r in ArchivedItems.ToList())
                    {
                        if (r.ID == item.ID)
                        {
                            ArchivedItems.Remove(r);
                        }
                    }

                    items.Remove(item);
                }

                foreach (var a in items.ToList())
                {
                    if (noArchives)
                    {
                        break;
                    }

                    var existingItem = ArchivedItems.FirstOrDefault(x => x.ResolvedId == a.ResolvedId);
                    if (existingItem != null)
                    {
                        Utils.CopyItem(a, existingItem);
                        items.Remove(a);
                    }
                }

                items = items.Where(x => x.IsArchive).ToList();

                if (ArchivedItems.IsNullOrEmpty())
                {
                    ArchivedItems = await items.Sort(App.Settings.DefaultSort);
                }
                else
                {
                    if (SelectedSortType.Value == SortType.TitleAscending || SelectedSortType.Value == SortType.TitleDescending)
                    {
                        var mergedItems = items.Union(ArchivedItems).ToObservableCollection();
                        ArchivedItems = await mergedItems.Sort(SelectedSortType.Value);
                    }
                    else
                    {
                        var i = 0;
                        foreach (var item in items)
                        {
                            if (SelectedSortType.Value == SortType.DateDescending)
                            {
                                ArchivedItems.Insert(i, item);
                            }
                            else if (SelectedSortType.Value == SortType.DateAscending)
                            {
                                ArchivedItems.Insert(ArchivedItems.Count - 1, item);
                            }

                            i++;
                        }
                    }
                }

                await CacheService.Current.SaveArchivedItems(ArchivedItems);
                CacheService.Current.SetSinceDate(PivotArchived, DateTime.Now);
                result = true;
            }
            catch (PocketException ex)
            {
                ex.Log("GetArchivedItems()", Log);
            }
            catch (Exception ex)
            {
                Log.ErrorException("GetArchivedItems", ex);
            }

            SetProgressBar();

            return result;
        }
    }
}