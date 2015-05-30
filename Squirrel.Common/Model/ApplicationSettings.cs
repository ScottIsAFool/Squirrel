using PropertyChanged;

namespace Squirrel.Model
{
    [ImplementPropertyChanged]
    public class ApplicationSettings
    {
        public ApplicationSettings()
        {
            DefaultSort = SortType.DateDescending;
            DefaultCacheTimeout = 15;
            DownloadFilesInBackground = true;
            DefaultFontSize = FontSize.Small;
            IncludeCountsOnTile = true;
        }

        public bool RefreshOnOpen { get; set; }
        public SortType DefaultSort { get; set; }
        public int DefaultCacheTimeout { get; set; }
        public bool IsBoxViewDefault { get; set; }
        public TileColour TileColour { get; set; }
        public bool OnlyDownloadOnWifi { get; set; }
        public bool DownloadFilesInBackground { get; set; }
        public bool DownloadFilesWhenNotRunning { get; set; }
        public FontSize DefaultFontSize { get; set; }
        public SquirrelTheme DefaultSquirrelTheme { get; set; }
        public bool JustifyArticleText { get; set; }
        public bool IncludeCountsOnTile { get; set; }
        public bool DontPromptForDeletion { get; set; }
    }
}
