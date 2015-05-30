namespace Squirrel
{
    public class Constants
    {
        public const string PocketConsumerKey = "19680-36ec0229493f832d212232cd";
        public const string CallBackUri = "squirrel:PocketAuthorise?ReturnUri={0}";
        public const string FacebookApiUrl = "https://graph.facebook.com/me?fields=first_name,username,email&access_token={0}";
        public const string FacebookAppId = "1425717100985696";
        public const string ImageTemplate = "<!--IMG_{0}-->";
        public const string VideoTemplate = "<!--VIDEO_{0}-->";
        public const string ScheduledTaskName = "Squirrel.BackgroundAgent";

        public class Messages
        {
            public const string ArticleLoadMsg = "ArticleLoadMsg";
            public const string SortChangeMsg = "SortChangeMsg";
            public const string DetailsFromExternalMsg = "DetailsFromExternalMsg";
            public const string AuthTypeMsg = "AuthTypeMsg";
            public const string PullToRefreshMsg = "PullToRefreshMsg";
            public const string ShowTagsMainPageMsg = "ShowTagsMainPageMsg";
            public const string AddTagToSelectedMsg = "AddTagToSelectedMsg";
            public const string CloseTagsMsg = "CloseTagsMsg";
            public const string ClearCacheMsg = "ClearCacheMsg";
            public const string RemoveFavouriteMsg = "RemoveFavouriteMsg";
            public const string SaveToCacheMsg = "SaveToCacheMsg";
            public const string MarkAsReadMsg = "MarkAsReadMsg";
            public const string SetSysTrayColour = "SetSysTrayColour";
        }

        public class Pages
        {
            private const string Views = "/Views/";
            public const string MainPage = Views + "MainPage.xaml";
            public const string SettingsView = Views + "SettingsView.xaml";
            public const string AdvancedAddView = Views + "AdvancedAddView.xaml";
            public const string AboutView = Views + "AboutView.xaml";

            public class Login
            {
                private const string LoginPath = Views + "Login/";
                public const string LoginView = LoginPath + "LoginView.xaml?ReturnUri={0}";
                public const string AuthorisingView = LoginPath + "AuthorisingView.xaml";
                public const string NewUserView = LoginPath + "NewUserView.xaml";
            }

            public class Search
            {
                private const string SearchPath = Views + "Search/";
                public const string SearchView = SearchPath + "SearchView.xaml";
                public const string SearchResultsView = SearchPath + "SearchResultsView.xaml";
            }

            public class Article
            {
                private const string ArticlePath = Views + "Article/";
                public const string ArticleView = ArticlePath + "ArticleView.xaml";
                public const string ImagesView = ArticlePath + "ImagesView.xaml";
            }

            public class FirstRun
            {
                private const string FirstRunPath = Views + "FirstRun/";
                public const string OfflineView = FirstRunPath + "OfflineView.xaml";
            }
        }

        public class StorageSettings
        {
            public const string AccessToken = "AccessToken";
            public const string AppSettings = "AppSettings";
            public const string FacebookAccessToken = "FacebookAccessToken";
            public const string FacebookExpiry = "FacebookExpiry";
        }
    }
}
