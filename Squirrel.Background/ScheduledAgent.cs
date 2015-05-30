using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using Cimbalino.Phone.Toolkit.Services;
using Microsoft.Phone.Scheduler;
using PocketArticle;
using ScottIsAFool.WindowsPhone.Logging;
using Squirrel.Model;
using Squirrel.Services;

namespace Squirrel.Background
{
    public class ScheduledAgent : ScheduledTaskAgent
    {
        private static readonly ILog Logger;

        private static readonly CancellationTokenSource CancellationToken;

        /// <remarks>
        /// ScheduledAgent constructor, initializes the UnhandledException handler
        /// </remarks>
        static ScheduledAgent()
        {
            Logger = new WPLogger(typeof(ScheduledAgent));
            var storage = new AsyncStorageService();
            var articleClient = new PocketArticleClient(Constants.PocketConsumerKey, timeout: 30);
            CancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(20));

            OfflineService.Current.StartService(storage, articleClient);

            var settingsService = new ApplicationSettingsService();
            var settings = settingsService.Get(Constants.StorageSettings.AppSettings, new ApplicationSettings());

            OfflineService.Current.OnlyDownloadOnWifi = settings.OnlyDownloadOnWifi;

            // Subscribe to the managed exception handler
            Deployment.Current.Dispatcher.BeginInvoke(delegate 
            {
                Application.Current.UnhandledException += UnhandledException;
            });
        }

        /// Code to execute on Unhandled Exceptions
        private static async void UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                Debugger.Break();
            }

            Logger.FatalException("UnhandledException", e.ExceptionObject);

            await OfflineService.Current.RemoveLockFile();
        }

        /// <summary>
        /// Agent that runs a scheduled task
        /// </summary>
        /// <param name="task">
        /// The invoked task
        /// </param>
        /// <remarks>
        /// This method is called when a periodic or resource intensive task is invoked
        /// </remarks>
        protected override async void OnInvoke(ScheduledTask task)
        {
            if (!await OfflineService.Current.CanStartDownloading())
            {
                NotifyComplete();
                return;
            }

            await OfflineService.Current.CheckAndStartDownloads(CancellationToken.Token).ContinueWith(async theTask =>
            {
                await OfflineService.Current.RemoveLockFile();
            });

            NotifyComplete();
        }
    }
}