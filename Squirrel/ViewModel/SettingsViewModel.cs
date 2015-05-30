using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using JetBrains.Annotations;
using Microsoft.Phone.Scheduler;
using ScottIsAFool.WindowsPhone.ViewModel;
using Squirrel.Model;
using Squirrel.Resources;
using Squirrel.Services;
using Squirrel.Extensions;

namespace Squirrel.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class SettingsViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the LoginViewModel class.
        /// </summary>
        public SettingsViewModel()
        {
            SelectedSortType = SortTypes.FirstOrDefault(x => x.Value == App.Settings.DefaultSort);
            SelectedCachedTimeout = CacheTimeouts.FirstOrDefault(x => x == App.Settings.DefaultCacheTimeout);
            SelectedTileColour = TileColours.FirstOrDefault(x => x.Value == App.Settings.TileColour);
            DownloadFilesInBackground = App.Settings.DownloadFilesInBackground;
            DownloadFilesWhenNotRunning = App.Settings.DownloadFilesWhenNotRunning;
            OnlyDownloadOnWifi = App.Settings.OnlyDownloadOnWifi;

            FontSizes = Enum.GetValues(typeof(FontSize)).ToList<FontSize>();
            Themes = Enum.GetValues(typeof(SquirrelTheme)).ToList<SquirrelTheme>();

            SelectedFontSize = FontSizes.FirstOrDefault(x => x == App.Settings.DefaultFontSize);
            SelectedTheme = Themes.FirstOrDefault(x => x == App.Settings.DefaultSquirrelTheme);
        }

        public KeyValuePair<string, SortType> SelectedSortType { get; set; }
        public KeyValuePair<string, TileColour> SelectedTileColour { get; set; }
        public FontSize SelectedFontSize { get; set; }
        public SquirrelTheme SelectedTheme { get; set; }
        public int SelectedCachedTimeout { get; set; }
        public bool DownloadFilesInBackground { get; set; }
        public bool DownloadFilesWhenNotRunning { get; set; }
        public bool OnlyDownloadOnWifi { get; set; }

        public RelayCommand ClearCacheCommand
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    await CacheService.Current.ClearAllItems();
                    Log.Info("Cache cleared");
                    Messenger.Default.Send(new NotificationMessage(Constants.Messages.ClearCacheMsg));
                    App.ShowMessage("Cache cleared");
                });
            }
        }

        public RelayCommand ClearDownloadedFilesCommand
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    var result = MessageBox.Show("Are you sure you wish to delete the offline files?", "Are you sure?", MessageBoxButton.OKCancel);
                    if (result == MessageBoxResult.OK)
                    {
                        await OfflineService.Current.ClearOfflineFiles();
                        App.ShowMessage("Offline files cleared");
                        Log.Info("Offline files cleared");
                    }
                });
            }
        }

        public Dictionary<string, SortType> SortTypes
        {
            get
            {
                return new Dictionary<string, SortType>
                {
                    {"Title (a-z)", SortType.TitleAscending},
                    {"Title (z-a)", SortType.TitleDescending},
                    {"Date (oldest first)", SortType.DateAscending},
                    {"Date (newest first)", SortType.DateDescending},
                };
            }
        }

        public Dictionary<string, TileColour> TileColours
        {
            get
            {
                return new Dictionary<string, TileColour>
                {
                    {"Squirrel blue", TileColour.SquirrelBlue},
                    {AppResources.LabelSquirrelAccentColour, TileColour.SquirrelAccent},
                    {AppResources.LabelPhoneAccentColour, TileColour.PhoneAccent}
                };
            }
        }

        public List<int> CacheTimeouts
        {
            get
            {
                return new List<int>
                {
                    5,
                    15,
                    30,
                    60
                };
            }
        }

        public List<FontSize> FontSizes { get; private set; }

        public List<SquirrelTheme> Themes { get; private set; }

        [UsedImplicitly]
        private void OnSelectedSortTypeChanged()
        {
            App.Settings.DefaultSort = SelectedSortType.Value;
        }

        [UsedImplicitly]
        private void OnSelectedTileColourChanged()
        {
            App.Settings.TileColour = SelectedTileColour.Value;
            TileService.Current.ChangeTileBackground(SelectedTileColour.Value);
        }

        [UsedImplicitly]
        private void OnDownloadFilesWhenNotRunningChanged()
        {
            Log.Info("DownloadFilesWhenNotRunning changed: {0}", DownloadFilesWhenNotRunning);
            App.Settings.DownloadFilesWhenNotRunning = DownloadFilesWhenNotRunning;
            StopAgentIfStarted();

            if (DownloadFilesWhenNotRunning)
            {
                var agent = new PeriodicTask(Constants.ScheduledTaskName)
                {
                    Description = "Downloads any pending articles in the background."
                };

                Log.Info("Adding agent to scheduled list");
                ScheduledActionService.Add(agent);

                TestScheduledEvent();
            }
        }

        [UsedImplicitly]
        private void OnDownloadFilesInBackgroundChanged()
        {
            Log.Info("DownloadFilesInBackground changed: {0}", DownloadFilesInBackground);
            App.Settings.DownloadFilesInBackground = DownloadFilesInBackground;
            if (DownloadFilesInBackground)
            {
                if (!OfflineService.Current.IsRunning)
                {
                    Log.Info("Start processing any download queue");
                    OfflineService.Current.CheckAndStartDownloads(App.CancellationToken.Token).ConfigureAwait(false);
                }
            }
            else
            {
                Log.Info("Cancelling any processing downloads");
                App.CancellationToken.Cancel();
            }
        }

        [UsedImplicitly]
        private void OnOnlyDownloadOnWifiChanged()
        {
            Log.Info("OnlyDownloadOnWifi changed: {0}", OnlyDownloadOnWifi);
            App.Settings.OnlyDownloadOnWifi = OnlyDownloadOnWifi;
            OfflineService.Current.OnlyDownloadOnWifi = OnlyDownloadOnWifi;
        }

        [UsedImplicitly]
        private void OnSelectedFontSizeChanged()
        {
            App.Settings.DefaultFontSize = SelectedFontSize;
        }

        [UsedImplicitly]
        private void OnSelectedThemeChanged()
        {
            App.Settings.DefaultSquirrelTheme = SelectedTheme;
        }
        
        [Conditional("DEBUG")]
        private void TestScheduledEvent()
        {
            //ScheduledActionService.LaunchForTest(Constants.ScheduledTaskName, new TimeSpan(0, 0, 5));
        }

        private void StopAgentIfStarted()
        {
            if (ScheduledActionService.Find(Constants.ScheduledTaskName) != null)
            {
                Log.Info("Stopping Agent");
                ScheduledActionService.Remove(Constants.ScheduledTaskName);
            }
        }
    }
}