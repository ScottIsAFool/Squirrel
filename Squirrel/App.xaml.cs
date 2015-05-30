﻿using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Navigation;
using Cimbalino.Phone.Toolkit.Helpers;
using Coding4Fun.Toolkit.Controls;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using ScottIsAFool.WindowsPhone.Logging;
using Squirrel.Model;
using Squirrel.Resources;
using Squirrel.Services;
using Squirrel.ViewModel;
using Telerik.Windows.Controls;

namespace Squirrel
{
    public partial class App
    {
        private readonly ILog _logger;

        private static bool _popupOpen;

        public static CancellationTokenSource CancellationToken { get; set; }

        public static void ShowMessage(string message, string title = "", Action action = null)
        {
            if (_popupOpen)
            {
                return;
            }

            var prompt = new ToastPrompt
            {
                Title = title,
                Message = message,
                Background = (SolidColorBrush) Current.Resources["SquirrelSelectedBrush"],
                Margin = new Thickness(0, 32, 0, 0)
            };

            if (action != null)
            {
                prompt.Tap += (s, e) => action();
            }

            prompt.Completed += (sender, eventArgs) =>
            {
                _popupOpen = false;
            };

            _popupOpen = true;

            prompt.Show();
        }

        public static ApplicationSettings Settings
        {
            get
            {
                return (ApplicationSettings)Current.Resources["Settings"];
            }
        }

        /// <summary>
        /// Provides easy access to the root frame of the Phone Application.
        /// </summary>
        /// <returns>The root frame of the Phone Application.</returns>
        public static PhoneApplicationFrame RootFrame { get; private set; }

        /// <summary>
        /// Constructor for the Application object.
        /// </summary>
        public App()
        {
            _logger = new WPLogger(typeof(App));

            // Global handler for uncaught exceptions.
            UnhandledException += ApplicationUnhandledException;

            // Standard XAML initialization
            InitializeComponent();

            // Phone-specific initialization
            InitializePhoneApplication();

            // Language display initialization
            InitializeLanguage();

            ThemeManager.OverrideOptions = ThemeManagerOverrideOptions.SystemTrayAndApplicationBars;
            var rd = Current.Resources.MergedDictionaries[0];
            ThemeManager.SetCustomTheme(rd, Theme.Dark);
            ThemeManager.SetAccentColor(((SolidColorBrush)Current.Resources["PhoneAccentBrush"]).Color);

            WPLogger.AppVersion = ApplicationManifest.Current.App.Version;
            WPLogger.LogConfiguration.LogType = LogType.WriteToFile;
            WPLogger.LogConfiguration.LoggingIsEnabled = true;

            CancellationToken = new CancellationTokenSource();

            // Show graphics profiling information while debugging.
            if (Debugger.IsAttached)
            {
                // Display the current frame rate counters.
                Application.Current.Host.Settings.EnableFrameRateCounter = true;

                // Show the areas of the app that are being redrawn in each frame.
                //Application.Current.Host.Settings.EnableRedrawRegions = true;

                // Enable non-production analysis visualization mode,
                // which shows areas of a page that are handed off to GPU with a colored overlay.
                //Application.Current.Host.Settings.EnableCacheVisualization = true;

                // Prevent the screen from turning off while under the debugger by disabling
                // the application's idle detection.
                // Caution:- Use this under debug mode only. Application that disables user idle detection will continue to run
                // and consume battery power when the user is not using the phone.
                PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
            }

        }

        // Code to execute when the application is launching (eg, from Start)
        // This code will not execute when the application is reactivated
        private void ApplicationLaunching(object sender, LaunchingEventArgs e)
        {
            ApplicationUsageHelper.Init(ApplicationManifest.Current.App.Version);
            AppStartup();
        }

        private static void AppStartup()
        {
            AuthenticationService.Current.StartService(ViewModelLocator.SettingsService, ViewModelLocator.PocketClient);
            CacheService.Current.StartService(ViewModelLocator.StorageService, ViewModelLocator.SettingsService);
            OfflineService.Current.StartService(ViewModelLocator.StorageService, ViewModelLocator.PocketArticleClient);
            var settingsService = ViewModelLocator.SettingsService;
            var settings = settingsService.Get<ApplicationSettings>(Constants.StorageSettings.AppSettings, null);

            if (settings != null)
            {
                Utils.CopyItem(settings, Settings);
            }

            CacheService.Current.SetTimeout(Settings.DefaultCacheTimeout);
            if (Settings.DownloadFilesInBackground)
            {
                OfflineService.Current.OnlyDownloadOnWifi = Settings.OnlyDownloadOnWifi;
                OfflineService.Current.CheckAndStartDownloads(CancellationToken.Token).ConfigureAwait(false);
            }

            TileService.Current.IncludeCounts = Settings.IncludeCountsOnTile;
        }

        // Code to execute when the application is activated (brought to foreground)
        // This code will not execute when the application is first launched
        private void ApplicationActivated(object sender, ActivatedEventArgs e)
        {
            if (!e.IsApplicationInstancePreserved)
            {
                AppStartup();
            }

            ApplicationUsageHelper.OnApplicationActivated();
        }

        // Code to execute when the application is deactivated (sent to background)
        // This code will not execute when the application is closing
        private void ApplicationDeactivated(object sender, DeactivatedEventArgs e)
        {
            SaveAllTheThings();
        }

        // Code to execute when the application is closing (eg, user hit Back)
        // This code will not execute when the application is deactivated
        private void ApplicationClosing(object sender, ClosingEventArgs e)
        {
            SaveAllTheThings();
        }

        public static void SaveAllTheThings()
        {
            var settingsService = ViewModelLocator.SettingsService;
            settingsService.Set(Constants.StorageSettings.AppSettings, Settings);

            settingsService.Save();
        }

        // Code to execute if a navigation fails
        private void RootFrameNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            if (Debugger.IsAttached)
            {
                // A navigation has failed; break into the debugger
                Debugger.Break();
            }

            _logger.FatalException(string.Format("NavigationFailed, URI: {0}", e.Uri), e.Exception);
        }

        // Code to execute on Unhandled Exceptions
        private void ApplicationUnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                Debugger.Break();
            }

            _logger.FatalException("UnhandledException", e.ExceptionObject);

            e.Handled = true;
        }

        #region Phone application initialization

        // Avoid double-initialization
        private bool phoneApplicationInitialized = false;

        // Do not add any additional code to this method
        private void InitializePhoneApplication()
        {
            if (phoneApplicationInitialized)
                return;

            // Create the frame but don't set it as RootVisual yet; this allows the splash
            // screen to remain active until the application is ready to render.
            RootFrame = new TransitionFrame { Background = new SolidColorBrush(Colors.Transparent) };
            RootFrame.Navigated += CompleteInitializePhoneApplication;

            // Handle navigation failures
            RootFrame.NavigationFailed += RootFrameNavigationFailed;

            // Handle reset requests for clearing the backstack
            RootFrame.Navigated += CheckForResetNavigation;

            RootFrame.UriMapper = new SquirrelUriMapper();

            // Ensure we don't initialize again
            phoneApplicationInitialized = true;
        }

        // Do not add any additional code to this method
        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            // Set the root visual to allow the application to render
            if (RootVisual != RootFrame)
                RootVisual = RootFrame;

            // Remove this handler since it is no longer needed
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        private void CheckForResetNavigation(object sender, NavigationEventArgs e)
        {
            // If the app has received a 'reset' navigation, then we need to check
            // on the next navigation to see if the page stack should be reset
            if (e.NavigationMode == NavigationMode.Reset)
                RootFrame.Navigated += ClearBackStackAfterReset;
        }

        private void ClearBackStackAfterReset(object sender, NavigationEventArgs e)
        {
            // Unregister the event so it doesn't get called again
            RootFrame.Navigated -= ClearBackStackAfterReset;

            // Only clear the stack for 'new' (forward) and 'refresh' navigations
            if (e.NavigationMode != NavigationMode.New && e.NavigationMode != NavigationMode.Refresh)
                return;

            // For UI consistency, clear the entire page stack
            while (RootFrame.RemoveBackEntry() != null)
            {
                ; // do nothing
            }
        }

        #endregion

        private void InitializeLanguage()
        {
            try
            {
                // Set the font to match the display language defined by the
                // ResourceLanguage resource string for each supported language.
                //
                // Fall back to the font of the neutral language if the Display
                // language of the phone is not supported.
                //
                // If a compiler error is hit then ResourceLanguage is missing from
                // the resource file.
                RootFrame.Language = XmlLanguage.GetLanguage(AppResources.ResourceLanguage);

                // Set the FlowDirection of all elements under the root frame based
                // on the ResourceFlowDirection resource string for each
                // supported language.
                //
                // If a compiler error is hit then ResourceFlowDirection is missing from
                // the resource file.
                FlowDirection flow = (FlowDirection)Enum.Parse(typeof(FlowDirection), AppResources.ResourceFlowDirection);
                RootFrame.FlowDirection = flow;
            }
            catch
            {
                // If an exception is caught here it is most likely due to either
                // ResourceLangauge not being correctly set to a supported language
                // code or ResourceFlowDirection is set to a value other than LeftToRight
                // or RightToLeft.

                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }

                throw;
            }
        }
    }
}