using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using Newtonsoft.Json;
using PocketSharp.Models;
using PropertyChanged;
using ScottIsAFool.WindowsPhone.Extensions;

namespace Squirrel.Model
{
    [ImplementPropertyChanged]
    public class ExtendedPocketItem : PocketItem
    {
        public ExtendedPocketItem()
        {
            Visible = Visibility.Visible;
        }

        public bool IsOffline { get; set; }
        public bool IsRead { get; set; }

        public List<PocketImage> PocketImages { get; set; }
        public List<PocketTag> PocketTags { get; set; }
        [JsonIgnore]
        public Visibility Visible { get; set; }
        
        public string DisplayDate
        {
            get
            {
                return UpdateTime.HasValue ? UpdateTime.Value.ToString("D") : string.Empty;
            }
        }

        public string DisplayTitle
        {
            get
            {
                return string.IsNullOrEmpty(FullTitle) ? Title : FullTitle;
            }
        }

        public void SetVisibility(string filter)
        {
            switch (filter)
            {
                case "all items":
                    Visible = Visibility.Visible;
                    break;
                case "articles":
                    Visible = IsArticle ? Visibility.Visible : Visibility.Collapsed;
                    break;
                case "videos":
                    Visible = HasVideo || !Videos.IsNullOrEmpty() ? Visibility.Visible : Visibility.Collapsed;
                    break;
                case "images":
                    Visible = HasImage || Videos.IsNullOrEmpty() ? Visibility.Visible : Visibility.Collapsed;
                    break;
                case "read":
                    Visible = IsRead ? Visibility.Visible : Visibility.Collapsed;
                    break;
                case "unread":
                    Visible = !IsRead ? Visibility.Visible : Visibility.Collapsed;
                    break;
            }
        }
    }
}
