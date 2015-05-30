using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using Windows.Phone.Speech.Synthesis;
using Cimbalino.Phone.Toolkit.Services;
using Coding4Fun.Toolkit.Controls;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using HtmlAgilityPack;
using JetBrains.Annotations;
using Microsoft.Phone.Controls;
using PocketArticle;
using PocketArticle.Entities;
using PocketSharp;
using PocketSharp.Models;
using ScottIsAFool.WindowsPhone.Extensions;
using ScottIsAFool.WindowsPhone.ViewModel;
using Squirrel.Extensions;
using Squirrel.Model;
using Squirrel.Resources;
using Squirrel.Services;
using CustomMessageBox = Squirrel.Controls.CustomMessageBox;
using INavigationService = Squirrel.Services.INavigationService;

namespace Squirrel.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class ArticleViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;
        private readonly IPocketClient _pocketClient;
        private readonly IPocketArticleClient _reader;
        private readonly IAsyncStorageService _storage;

        private ExtendedPocketItem _selectedItem;

        /// <summary>
        /// Initializes a new instance of the LoginViewModel class.
        /// </summary>
        public ArticleViewModel(INavigationService navigationService, IPocketClient pocketClient, IPocketArticleClient reader, IAsyncStorageService storage)
        {
            _navigationService = navigationService;
            _pocketClient = pocketClient;
            _reader = reader;
            _storage = storage;

            if (IsInDesignMode)
            {
                SelectedArticle = new ExtendedPocketItem
                {
                    IsOffline = true,
                    AddTime = DateTime.Now,
                    Excerpt = "Oh hell yeah she did!",
                    IsFavorite = true,
                    Title = "Keyara Loses Her Bikini Top in Malibu Pimping 138",
                    FullTitle = "Keyara Loses Her Bikini Top in Malibu Pimping 138",
                    PocketTags = new List<PocketTag>
                    {
                        new PocketTag{Name = "bikini"},
                        new PocketTag{Name = "topless"},
                        new PocketTag{Name = "egotastic"},
                        new PocketTag{Name = "Lovely"}
                    },
                    UpdateTime = new DateTime(2013, 11, 27, 22, 30, 20)
                };

                SquirrelTheme = SquirrelTheme.Acorn;
            }

            DisplayArticleMessage = false;
        }

        public ObservableCollection<ExtendedPocketItem> SelectedList { get; set; }
        public ExtendedPocketItem SelectedArticle { get; set; }
        public PocketArticleItem PocketArticle { get; set; }
        public PocketArticleImage SelectedImage { get; set; }
        public string ArticleContent { get; set; }
        public FontSize ArticleFontSize { get; set; }
        public SquirrelTheme SquirrelTheme { get; set; }
        public bool DisplayArticleMessage { get; set; }

        public RelayCommand ArticlePageLoaded
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    if (SelectedArticle == null)
                    {
                        // TODO: Display error
                        return;
                    }

                    SetProgressBar("Getting article...");

                    await GetArticleDetails();

                    SelectedArticle.IsRead = true;

                    Messenger.Default.Send(new NotificationMessage(PocketTypes.All, Constants.Messages.SaveToCacheMsg));

                    SetProgressBar();
                });
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

                    if (!item.IsFavorite)
                    {
                        Messenger.Default.Send(new NotificationMessage(item, Constants.Messages.RemoveFavouriteMsg));
                    }

                    SetProgressBar();
                });
            }
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

                    SetProgressBar(item.IsArchive ? "Removing from archive..." : "Archiving...");

                    await item.ArchiveItem(_pocketClient, Log, CacheService.Current);

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
                    if (item == null || !await _navigationService.IsNetworkAvailable())
                    {
                        return;
                    }

                    if (!App.Settings.DontPromptForDeletion)
                    {
                        var question = new CustomMessageBox
                        {
                            Message = "Are you sure you wish to delete this item from your Pocket?",
                            Title = "Are you sure?",
                            LeftButtonContent = "yes",
                            RightButtonContent = "no",
                            Content = Utils.CreateDontShowCheckBox("DontPromptForDeletion")
                        };
                        var result = await question.ShowAsync();
                        if (result == CustomMessageBoxResult.RightButton)
                        {
                            return;
                        }
                    }

                    SetProgressBar("Deleting...");

                    await item.DeleteItem(_pocketClient, Log, CacheService.Current, () =>
                    {
                        SelectedList.Remove(item);
                        Messenger.Default.Send(new NotificationMessage(Constants.Messages.SaveToCacheMsg));

                        if (SelectedList.IsNullOrEmpty())
                        {
                            _navigationService.GoBack();
                        }
                    });

                    SetProgressBar();
                });
            }
        }
        
        public RelayCommand OpenLinkCommand
        {
            get
            {
                return new RelayCommand(() => new WebBrowserService().Show(SelectedArticle.Uri));
            }
        }

        public RelayCommand<string> ImagesTappedCommand
        {
            get
            {
                return new RelayCommand<string>(selectedUrl =>
                {
                    SelectedImage = PocketArticle.Images.FirstOrDefault(x => x.Source == selectedUrl);
                    _navigationService.NavigateTo(Constants.Pages.Article.ImagesView);
                });
            }
        }

        public RelayCommand PlayArticleCommand
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    var doc = new HtmlDocument();
                    doc.LoadHtml(ArticleContent);

                    var text = doc.DocumentNode.InnerText;

                    await new SpeechSynthesizer().SpeakTextAsync(HttpUtility.HtmlDecode(text));
                });
            }
        }

        public RelayCommand SaveImageCommand
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    if (SelectedArticle.IsOffline)
                    {
                        if (await _storage.FileExistsAsync(SelectedImage.Source))
                        {
                            var image = await _storage.ReadAllBytesAsync(SelectedImage.Source);
                            var mediaLibrary = new MediaLibraryService();
                            var filename = string.Format("squirrel-{0}.jpg", DateTime.Now.ToString("yy-MM-dd-hh-mm-ss"));
                            mediaLibrary.SavePicture(filename, image);

                            App.ShowMessage("Image saved.");
                        }
                    }
                    else
                    {
                        var client = new HttpClient();
                        var response = await client.GetAsync(SelectedImage.Source);

                        if (response.IsSuccessStatusCode)
                        {
                            var mediaLibrary = new MediaLibraryService();
                            var filename = string.Format("squirrel-{0}.jpg", DateTime.Now.ToString("yy-MM-dd-hh-mm-ss"));
                            mediaLibrary.SavePicture(filename, await response.Content.ReadAsStreamAsync());

                            App.ShowMessage("Image saved.");
                        }
                    }
                });
            }
        }

        public override void WireMessages()
        {
            Messenger.Default.Register<NotificationMessage>(this, m =>
            {
                if (m.Notification.Equals(Constants.Messages.ArticleLoadMsg))
                {
                    var selectedList = (ObservableCollection<ExtendedPocketItem>) m.Target;
                    _selectedItem = (ExtendedPocketItem)m.Sender;
                    ResetView();

                    SelectedList = selectedList;
                    SelectedArticle = SelectedList.FirstOrDefault(x => x.ResolvedId == _selectedItem.ResolvedId);
                }
            });
        }

        private void ResetView()
        {
            ArticleContent = string.Empty;
            PocketArticle = null;
            SquirrelTheme = App.Settings.DefaultSquirrelTheme;
            ArticleFontSize = App.Settings.DefaultFontSize;
            DisplayArticleMessage = false;
        }

        [UsedImplicitly]
        private async void OnSelectedArticleChanged()
        {
            if (IsInDesignMode)
            {
                return;
            }

            SetProgressBar("Getting article...");

            ArticleContent = string.Empty;
            await GetArticleDetails();

            SetProgressBar();
        }

        private async Task<bool> GetArticleDetails()
        {
            if (SelectedArticle == null || SelectedArticle.Uri == null)
            {
                return false;
            }

            if (await OfflineService.Current.ArticleIsOffline(SelectedArticle.ResolvedId))
            {
                var article = await OfflineService.Current.GetArticleContent(SelectedArticle.ResolvedId);
                var isVideo = article.IsVideo.HasValue && article.IsVideo.Value && article.Videos != null;

                if (article.Images != null)
                {
                    foreach (var image in article.Images)
                    {
                        // <!--IMG_1-->
                        var filename = OfflineService.Current.GetImageFileName(image.Source);
                        var template = isVideo ? Constants.VideoTemplate : Constants.ImageTemplate;
                        var imageTag = string.Format(template, image.ImageId);

                        var imageUrl = OfflineService.Current.GetImageUrl(article.ResolvedId, filename);
                        var replaceWithTag = isVideo
                            ? string.Format("<a href=\"{0}\"><img src=\"{1}\" tag=\"IsVideo\"/></a>", article.ResolvedUrl, imageUrl)
                            : string.Format("<img src=\"{0}\" />", imageUrl);

                        article.ArticleContent = article.ArticleContent.Replace(imageTag, replaceWithTag);
                    }
                }

                if (article.Videos != null)
                {
                    foreach (var video in article.Videos)
                    {
                        
                    }
                }

                PocketArticle = article;
                ArticleContent = article.ArticleContent;
                DisplayArticleMessage = (article.IsIndex.HasValue && article.IsIndex.Value) || (article.IsArticle.HasValue && !article.IsArticle.Value);
                return true;
            }

            try
            {
                if (!await _navigationService.IsNetworkAvailable())
                {
                    return false;
                }

                var article = await _reader.GetArticleAsync(SelectedArticle.Uri.ToString(), useImagePlaceholders: true, useVideoPlaceholders: true, cancellationToken: App.CancellationToken.Token);

                var isVideo = article.IsVideo.HasValue && article.IsVideo.Value && article.Videos != null;
                if (article.Images != null)
                {
                    foreach (var image in article.Images)
                    {
                        // <!--IMG_1-->
                        var template = isVideo ? Constants.VideoTemplate : Constants.ImageTemplate;
                        var imageTag = string.Format(template, image.ImageId);
                        var replaceWithTag = isVideo
                            ? string.Format("<a href=\"{0}\"><img src=\"{1}\" tag=\"IsVideo\"/></a>", article.ResolvedUrl, image.Source)
                            : string.Format("<img src=\"{0}\" />", image.Source);
                        article.ArticleContent = article.ArticleContent.Replace(imageTag, replaceWithTag);
                    }
                }

                if (article.Videos != null)
                {
                    foreach (var video in article.Videos)
                    {

                    }
                }

                PocketArticle = article;
                ArticleContent = article.ArticleContent;
                //OfflineService.Current.AddToDownloadQueue(SelectedArticle, App.CancellationToken.Token).ConfigureAwait(false);

                return true;
            }
            catch (Exception ex)
            {
                Log.ErrorException("GetArticleDetails(" + SelectedArticle.Uri + ")", ex);
            }

            return false;
        }
    }
}