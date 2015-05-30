using System;
using System.Windows;
using System.Windows.Media;
using Microsoft.Phone.Shell;
using Squirrel.Model;

namespace Squirrel.Extensions
{
    public static class ExtensionsMethods
    {
        public static void Reset(this IApplicationBar appBar)
        {
            if (appBar == null)
            {
                return;
            }

            appBar.BackgroundColor = ConvertStringToColor("#EAEAEA");
            appBar.Opacity = 0.7;
            appBar.ForegroundColor = ConvertStringToColor("#086FA1");
            appBar.StateChanged += (sender, args) =>
            {
                appBar.Opacity = args.IsMenuVisible ? 1 : 0.85;
            };
        }

        public static SolidColorBrush ToBrush(this TileColour tileColour)
        {
            switch (tileColour)
            {
                case TileColour.PhoneAccent:
                    return new SolidColorBrush(Colors.Transparent);
                case TileColour.SquirrelAccent:
                    return (SolidColorBrush) Application.Current.Resources["PhoneAccentBrush"];
                default:
                    return (SolidColorBrush) Application.Current.Resources["PhoneBackgroundBrush"];
            }
        }

        public static Color ConvertStringToColor(string hex)
        {
            //remove the # at the front
            hex = hex.Replace("#", "");

            byte a = 255;
            byte r = 255;
            byte g = 255;
            byte b = 255;

            int start = 0;

            //handle ARGB strings (8 characters long)
            if (hex.Length == 8)
            {
                a = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                start = 2;
            }

            //convert RGB characters to bytes
            r = byte.Parse(hex.Substring(start, 2), System.Globalization.NumberStyles.HexNumber);
            g = byte.Parse(hex.Substring(start + 2, 2), System.Globalization.NumberStyles.HexNumber);
            b = byte.Parse(hex.Substring(start + 4, 2), System.Globalization.NumberStyles.HexNumber);

            return Color.FromArgb(a, r, g, b);
        }
    }
}
