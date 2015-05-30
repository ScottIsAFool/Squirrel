using Cimbalino.Phone.Toolkit.Services;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;
using PocketArticle;
using PocketSharp;
using Squirrel.Design;
using INavigationService = Squirrel.Services.INavigationService;
using NavigationService = Squirrel.Services.NavigationService;

namespace Squirrel.ViewModel
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            if (ViewModelBase.IsInDesignModeStatic)
            {
                if (!SimpleIoc.Default.IsRegistered<IApplicationSettingsService>())
                    SimpleIoc.Default.Register<IApplicationSettingsService, ApplicationSettingsServiceDesign>();

                if (!SimpleIoc.Default.IsRegistered<IAsyncStorageService>())
                    SimpleIoc.Default.Register<IAsyncStorageService, AsyncStorageServiceDesign>();

                if(!SimpleIoc.Default.IsRegistered<IPocketArticleClient>())
                    SimpleIoc.Default.Register<IPocketArticleClient, PocketArticleClientDesign>();

                if(!SimpleIoc.Default.IsRegistered<IPocketClient>())
                    SimpleIoc.Default.Register<IPocketClient, PocketClientDesign>();
            }
            else
            {
                if (!SimpleIoc.Default.IsRegistered<IApplicationSettingsService>())
                    SimpleIoc.Default.Register<IApplicationSettingsService, ApplicationSettingsService>();

                if(!SimpleIoc.Default.IsRegistered<IAsyncStorageService>())
                    SimpleIoc.Default.Register<IAsyncStorageService, AsyncStorageService>();

                if (!SimpleIoc.Default.IsRegistered<IPocketArticleClient>())
                    SimpleIoc.Default.Register<IPocketArticleClient>(() => new PocketArticleClient(Constants.PocketConsumerKey, timeout: 30));

                if (!SimpleIoc.Default.IsRegistered<IPocketClient>())
                    SimpleIoc.Default.Register<IPocketClient>(() => new PocketClient(Constants.PocketConsumerKey));
            }

            if(!SimpleIoc.Default.IsRegistered<INavigationService>())
                SimpleIoc.Default.Register<INavigationService, NavigationService>();
            
            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<LoginViewModel>();
            SimpleIoc.Default.Register<SettingsViewModel>();
            SimpleIoc.Default.Register<ArticleViewModel>(true);
            SimpleIoc.Default.Register<SearchViewModel>();
            SimpleIoc.Default.Register<AdvancedAddViewModel>(true);
        }

        public MainViewModel Main
        {
            get
            {
                return ServiceLocator.Current.GetInstance<MainViewModel>();
            }
        }

        public LoginViewModel Login
        {
            get
            {
                return ServiceLocator.Current.GetInstance<LoginViewModel>();
            }
        }

        public SettingsViewModel Settings
        {
            get
            {
                return ServiceLocator.Current.GetInstance<SettingsViewModel>();
            }
        }

        public ArticleViewModel Article
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ArticleViewModel>();
            }
        }

        public SearchViewModel Search
        {
            get
            {
                return ServiceLocator.Current.GetInstance<SearchViewModel>();
            }
        }

        public AdvancedAddViewModel Add
        {
            get
            {
                return ServiceLocator.Current.GetInstance<AdvancedAddViewModel>();
            }
        }

        public static IAsyncStorageService StorageService
        {
            get
            {
                return ServiceLocator.Current.GetInstance<IAsyncStorageService>();
            }
        }

        public static IApplicationSettingsService SettingsService
        {
            get
            {
                return ServiceLocator.Current.GetInstance<IApplicationSettingsService>();
            }
        }

        public static IPocketClient PocketClient
        {
            get { return ServiceLocator.Current.GetInstance<IPocketClient>(); }
        }

        public static IPocketArticleClient PocketArticleClient
        {
            get { return ServiceLocator.Current.GetInstance<IPocketArticleClient>(); }
        }
        
        public static void Cleanup()
        {
            foreach (var vm in SimpleIoc.Default.GetAllInstances<ViewModelBase>())
            {
                vm.Cleanup();
            }
        }
    }
}