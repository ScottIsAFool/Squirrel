using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Cimbalino.Phone.Toolkit.Extensions;
using GalaSoft.MvvmLight.Command;
using PocketSharp;
using ScottIsAFool.WindowsPhone.Extensions;
using ScottIsAFool.WindowsPhone.ViewModel;
using Squirrel.Extensions;
using Squirrel.Model;
using INavigationService = Squirrel.Services.INavigationService;

namespace Squirrel.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class SearchViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;
        private readonly IPocketClient _pocketClient;

        private bool _resultsLoaded;

        /// <summary>
        /// Initializes a new instance of the LoginViewModel class.
        /// </summary>
        public SearchViewModel(INavigationService navigationService, IPocketClient pocketClient)
        {
            _navigationService = navigationService;
            _pocketClient = pocketClient;

            if (IsInDesignMode)
            {
                SearchTerm = "wpdev";
            }
        }

        public string SearchTerm { get; set; }
        public bool SearchByTags { get; set; }
        public bool SearchInArticleLinks { get; set; }
        public ObservableCollection<ExtendedPocketItem> SearchResults { get; set; }

        public RelayCommand SearchCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (string.IsNullOrEmpty(SearchTerm))
                    {
                        return;
                    }

                    _navigationService.NavigateTo(Constants.Pages.Search.SearchResultsView);
                });
            }
        }

        public RelayCommand SearchResultsPageLoaded
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    if (!await _navigationService.IsNetworkAvailable())
                    {
                        _navigationService.GoBack();
                        return;
                    }

                    if (_resultsLoaded)
                    {
                        return;
                    }

                    _resultsLoaded = await DoSearch();
                });
            }
        }

        private async Task<bool> DoSearch()
        {
            try
            {
                SetProgressBar("Searching...");

                var items = SearchByTags
                    ? await _pocketClient.SearchByTag(SearchTerm)
                    : await _pocketClient.Search(SearchTerm, SearchInArticleLinks);

                if (items.IsNullOrEmpty())
                {
                    return false;
                }

                SearchResults = items.Select(x => x.Extend()).ToObservableCollection();

                SetProgressBar();

                return true;
            }
            catch (PocketException ex)
            {
                ex.Log("DoSearch(" + SearchByTags + ")", Log);
            }
            catch (Exception ex)
            {
                Log.ErrorException("DoSearch()", ex);
            }

            SetProgressBar();

            return false;
        }
    }
}