using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Squirrel.Model;
using ScottIsAFool.WindowsPhone.Extensions;

namespace Squirrel.Extensions
{
    public static class SquirrelExtensions
    {
        public static List<string> AllTagsToList(this IList<ExtendedPocketItem> items)
        {
            if (items.IsNullOrEmpty())
            {
                return new List<string>();
            }

            try
            {
                return items.Where(x => x != null && !x.Tags.IsNullOrEmpty())
                            .SelectMany(y => y.Tags)
                            .Select(z => z.Name)
                            .ToList();
            }
            catch
            {
                return new List<string>();
            }
        }

        public static List<T> ToList<T>(this Array array)
        {
            return (from object item in array select (T) item).ToList();
        }

        public static string GetYoutubeVideoIdFromUrl(this string targetUrl)
        {
            try
            {

                if (targetUrl.Contains("m.youtube.com") && targetUrl.Contains("desktop_uri"))
                {
                    targetUrl = "http://www.youtube.com" + targetUrl.Substring(targetUrl.IndexOf("desktop_uri", StringComparison.Ordinal) + 12);
                    targetUrl = HttpUtility.UrlDecode(targetUrl);
                }

                var firstRegex = new Regex(@"((you(tu.be\/()()(?<VideoId2>[^#\&\?]*)|tube.com(\/|\/\/|\/#\/)(v\/|u\/\w\/|embed\/|watch\?feature=(.*)&v=|watch\?v=|\&v=)(?<VideoId>[^#\&\?]*))))", RegexOptions.IgnoreCase);

                var results = firstRegex.Match(targetUrl);

                var newUrl = results.Groups["VideoId"].Value;

                if (string.IsNullOrEmpty(newUrl))
                {
                    newUrl = results.Groups["VideoId2"].Value;
                }

                return newUrl;

            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        //private UIElement GetYoutubeContent(string targetUrl)
        //{

        //    string newUrl = targetUrl.GetYoutubeVideoIdFromUrl();

        //    if (string.IsNullOrEmpty(newUrl))
        //        return null;

        //    const int CanvasWidth = 460;
        //    const int CanvasHeight = 460;

        //    var canvas = new Canvas { Width = CanvasWidth, Height = CanvasHeight, Margin = new Thickness(12) };

        //    string thumbNail = string.Format("http://img.youtube.com/vi/{0}/hqdefault.jpg", newUrl);

        //    var sourceImage = new BitmapImage(new Uri(thumbNail, UriKind.Absolute))
        //    {
        //        DecodePixelWidth = CanvasWidth
        //    };

        //    var image = new Image
        //    {
        //        MaxWidth = CanvasWidth,
        //        Stretch = Stretch.UniformToFill,
        //        VerticalAlignment = VerticalAlignment.Top
        //    };

        //    Canvas.SetLeft(image, 0);
        //    Canvas.SetTop(image, 0);

        //    canvas.Children.Add(image);

        //    // now the grey'd area
        //    var rect = new Rectangle
        //    {
        //        Width = CanvasWidth,
        //        Height = CanvasHeight,
        //        Fill = new SolidColorBrush(Colors.Black),
        //        Opacity = 0.3
        //    };

        //    sourceImage.ImageOpened += (sender, args) =>
        //    {
        //        canvas.Height = image.ActualHeight;
        //        rect.Height = image.ActualHeight;
        //    };
        //    image.Source = sourceImage;

        //    Canvas.SetLeft(rect, 0);
        //    Canvas.SetTop(rect, 0);

        //    canvas.Children.Add(rect);

        //    // now the button
        //    var playImageUri = new Uri("/images/bigplay.png", UriKind.Relative);
        //    var playImageBitmap = new BitmapImage(playImageUri);
        //    var playImage = new Image { Source = playImageBitmap };

        //    Canvas.SetLeft(playImage, 176);
        //    Canvas.SetTop(playImage, 110);

        //    canvas.Children.Add(playImage);

        //    playImage.Tap += (sender, e) => UiHelper.SafeDispatch(() => Windows.System.Launcher.LaunchUriAsync(new System.Uri("vnd.youtube:" + newUrl)));

        //    return canvas;

        //}
    }
}
