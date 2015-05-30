using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Coding4Fun.Toolkit.Controls;
using HtmlAgilityPack;
using System.Diagnostics;
using System.Text;
using System.Collections.Generic;
using Microsoft.Phone.Tasks;
using Squirrel.Model;

namespace Krempel.WP7.Core.Controls
{
    [TemplatePart(Name = HtmlTextBlock.PART_ItemsControl, Type = typeof(ItemsControl))]
    public class HtmlTextBlock : Control
    {
        private const string PART_ItemsControl = "PART_ItemsControl";

        public static readonly DependencyProperty HtmlProperty =
            DependencyProperty.Register("Html", typeof(string), typeof(HtmlTextBlock), new PropertyMetadata(default(string), HtmlChanged));

        public static readonly DependencyProperty ImageTappedCommandProperty =
            DependencyProperty.Register("ImageTappedCommand", typeof(ICommand), typeof(HtmlTextBlock), new PropertyMetadata(default(ICommand)));

        public static readonly DependencyProperty ArticleFontSizeProperty =
            DependencyProperty.Register("ArticleFontSize", typeof (FontSize), typeof (HtmlTextBlock), new PropertyMetadata(default(FontSize), OnArticletFontSizeChanged));

        public static readonly DependencyProperty ArticleForegroundProperty =
            DependencyProperty.Register("ArticleForeground", typeof (SolidColorBrush), typeof (HtmlTextBlock), new PropertyMetadata(default(SolidColorBrush), OnForegroundChanged));

        public static readonly DependencyProperty JustifyTextProperty =
            DependencyProperty.Register("JustifyText", typeof(bool), typeof(HtmlTextBlock), new PropertyMetadata(default(bool), OnJustifyTextChanged));

        private static void OnJustifyTextChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var htb = sender as HtmlTextBlock;
            if (htb == null || htb.internalItemsControl == null)
            {
                return;
            }

            var justifyText = (bool)e.NewValue;

            foreach (var item in htb.internalItemsControl.Items)
            {
                var richTextBox = item as RichTextBox;
                if (richTextBox == null)
                {
                    continue;
                }

                richTextBox.TextAlignment = justifyText ? TextAlignment.Justify : TextAlignment.Left;
            }
        }

        private static void OnArticletFontSizeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var htb = sender as HtmlTextBlock;
            if (htb == null || htb.internalItemsControl == null)
            {
                return;
            }

            var fontSize = (FontSize) e.NewValue;

            foreach (var item in htb.internalItemsControl.Items)
            {
                var richTextBox = item as RichTextBox;
                if (richTextBox == null)
                {
                    continue;
                }

                richTextBox.FontSize = (int) fontSize;
            }
        }

        private static void OnForegroundChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var htb = sender as HtmlTextBlock;
            if (htb == null || htb.internalItemsControl == null)
            {
                return;
            }

            var colour = (SolidColorBrush) e.NewValue;

            foreach (var item in htb.internalItemsControl.Items)
            {
                var richTextBox = item as RichTextBox;
                if (richTextBox == null)
                {
                    continue;
                }

                richTextBox.Foreground = colour;
            }
        }

        public bool JustifyText
        {
            get { return (bool)GetValue(JustifyTextProperty); }
            set { SetValue(JustifyTextProperty, value); }
        }

        public SolidColorBrush ArticleForeground
        {
            get { return (SolidColorBrush)GetValue(ArticleForegroundProperty); }
            set { SetValue(ArticleForegroundProperty, value); }
        }

        public FontSize ArticleFontSize
        {
            get { return (FontSize) GetValue(ArticleFontSizeProperty); }
            set { SetValue(ArticleFontSizeProperty, value); }
        }

        public ICommand ImageTappedCommand
        {
            get { return (ICommand)GetValue(ImageTappedCommandProperty); }
            set { SetValue(ImageTappedCommandProperty, value); }
        }

        public HtmlTextBlock()
        {
            DefaultStyleKey = typeof(HtmlTextBlock);
        }

        public string Html
        {
            get { return (string)GetValue(HtmlProperty); }
            set { SetValue(HtmlProperty, value); }
        }

        private ItemsControl internalItemsControl;

        private static async void HtmlChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null)
            {
                return;
            }

            var html = CleanHtml(e.NewValue.ToString());
            HtmlTextBlock instance = (HtmlTextBlock)o;
            if (instance.internalItemsControl != null)
            {
                instance.AppendHtml(html);
            }
        }

        private static string CleanHtml(string content)
        {
            content = Regex.Replace(content, "<div[^>]*>", "");
            content = Regex.Replace(content, "</div>", "");

            content = Regex.Replace(content, "<span[^>]*>", string.Empty);// "<p>");
            content = Regex.Replace(content, "</span>", string.Empty);//"</p>");

            //content = Regex.Replace(content, "<a[^>]*>", "<p>");
            //content = Regex.Replace(content, "</a>", "</p>");

            var stringBuilder = new StringBuilder(content);

            stringBuilder.Replace("\n", " ");
            stringBuilder.Replace("<ul", "<p");
            stringBuilder.Replace("</ul>", "</p>");
            stringBuilder.Replace("<ol", "<p");
            stringBuilder.Replace("</ol>", "</p>");

            stringBuilder.Replace("<blockquote><p>", "<p><blockquote>");
            stringBuilder.Replace("</blockquote></p>", "</p></blockquote>");
            stringBuilder.Replace("<br /></a>", "</a><br />");

            var existing = stringBuilder.ToString();
            existing = Regex.Replace(existing, "<p[^>]*>", "<p>");
            stringBuilder = new StringBuilder(existing);

            return stringBuilder.ToString();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            internalItemsControl = (ItemsControl)base.GetTemplateChild(HtmlTextBlock.PART_ItemsControl);

            if (!String.IsNullOrWhiteSpace(Html))
            {
                if (textBoxes == null || textBoxes.Count == 0)
                {
                    AppendHtml(Html);
                }
                else
                {
                    foreach (var rtb in textBoxes)
                    {
                        internalItemsControl.Items.Add(rtb);
                    }
                }
            }
        }

        private List<RichTextBox> textBoxes = null;

        private void AppendHtml(string html)
        {
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);
            if (textBoxes == null)
                textBoxes = new List<RichTextBox>();
            textBoxes.Clear();

            internalItemsControl.Items.Clear();

            ProcessChildren(htmlDoc.DocumentNode.ChildNodes);
        }

        private void ProcessChildren(HtmlNodeCollection htmlNodeCollection)
        {
            foreach (var node in htmlNodeCollection)
            {
                RichTextBox rtb = new RichTextBox
                {
                    Foreground = ArticleForeground,
                    TextAlignment = JustifyText ? TextAlignment.Justify : TextAlignment.Left,
                    FontSize = (int)ArticleFontSize
                };
                //textBoxes.Add(rtb);
                AppendParagraph(node, rtb);

                internalItemsControl.Items.Add(rtb);
            }
        }

        private void AppendParagraph(HtmlNode node, RichTextBox rtb)
        {
            Paragraph paragraph = new Paragraph();
            rtb.Blocks.Add(paragraph);
            if (node.Name == "p")
            {
                AppendChildren(node, paragraph, null);
            }
            else
            {
                AppendFromHtml(node, paragraph, null);
            }
        }

        private void AppendChildren(HtmlNode htmlNode, Paragraph paragraph, Span span)
        {
            foreach (var node in htmlNode.ChildNodes)
            {
                AppendFromHtml(node, paragraph, span);
            }
        }

        private void AppendFromHtml(HtmlNode node, Paragraph paragraph, Span span)
        {
            switch (node.Name)
            {
                case "h1":
                case "h2":
                case "h3":
                    AppendSpan(node, paragraph, span, node.Name);
                    break;
                case "p":
                case "ul":
                case "article":
                    AppendSpan(node, paragraph, span, null);
                    break;
                case "blockquote":
                case "q":
                    AppendSpan(node, paragraph, span, null);
                    break;
                case "#text":
                    AppendRun(node, paragraph, span);
                    break;
                case "a":
                    AppendHyperlink(node, paragraph, span);
                    break;
                case "br":
                case "li":
                    AppendLineBreak(node, paragraph, span);
                    break;
                case "image":
                case "img":
                    AppendImage(node, paragraph, span);
                    break;
                case "strong":
                case "b":
                    AddBoldFormatting(node, paragraph);
                    break;
                case "em":
                case "i":
                    AddItalicFormatting(node, paragraph);
                    break;
                case "div":
                case "span":
                    ProcessChildren(node.ChildNodes);
                    break;
                case "pre":
                    var s = "";
                    break;
                default:
                    Debug.WriteLine("Element {0} not implemented", node.Name);
                    break;
            }
        }

        private void AddItalicFormatting(HtmlNode node, Paragraph paragraph)
        {
            var italicInline = new Italic();
            italicInline.Inlines.Add(DecodeAndCleanupHtml(node.InnerText));
            paragraph.Inlines.Add(italicInline);
        }

        private void AddBoldFormatting(HtmlNode node, Paragraph paragraph)
        {
            var boldInline = new Bold();
            boldInline.Inlines.Add(DecodeAndCleanupHtml(node.InnerText));
            paragraph.Inlines.Add(boldInline);
        }

        private void AppendLineBreak(HtmlNode node, Paragraph paragraph, Span span)
        {
            LineBreak lineBreak = new LineBreak();

            if (span != null)
            {
                span.Inlines.Add(lineBreak);
            }
            else if (paragraph != null)
            {
                paragraph.Inlines.Add(lineBreak);
            }

            AppendChildren(node, paragraph, span);
        }

        private void AppendImage(HtmlNode node, Paragraph paragraph, Span span = null)
        {
            var inlineContainer = new InlineUIContainer();
            var image = new SuperImage { Stretch = Stretch.UniformToFill };
            var converter = new ImageSourceConverter();
            var source = (ImageSource)converter.ConvertFromString(node.Attributes["src"].Value);
            image.Source = source;
            image.DoubleTap += (sender, args) =>
            {
                if (ImageTappedCommand != null)
                {
                    ImageTappedCommand.Execute(node.Attributes["src"].Value);
                }
            };

            if (span is Hyperlink && node.Attributes.Contains("tag"))
            {
                var link = span as Hyperlink;
                var uri = HttpUtility.HtmlDecode(link.TargetName);
                image.Tap += (sender, args) => new WebBrowserTask { Uri = new Uri(uri, UriKind.Absolute) }.Show();
                image.Template = (ControlTemplate) Application.Current.Resources["VideoPlayControlTemplate"];
            }

            inlineContainer.Child = image;

            if (paragraph != null)
            {
                paragraph.Inlines.Add(inlineContainer);
            }

            AppendChildren(node, paragraph, null);
        }

        private void AppendHyperlink(HtmlNode node, Paragraph paragraph, Span span)
        {
            Hyperlink hyperlink = new Hyperlink
            {
                Foreground = ArticleForeground,
                FontWeight = FontWeights.Bold
            };
            var href = node.Attributes["href"] == null ? string.Empty : node.Attributes["href"].Value;
            hyperlink.TargetName = HttpUtility.HtmlEncode(href);
            hyperlink.Click += (sender, args) =>
            {
                if (!string.IsNullOrEmpty(href))
                {
                    Windows.System.Launcher.LaunchUriAsync(new Uri(href, UriKind.Absolute));
                }
            };

            if (span != null)
            {
                span.Inlines.Add(hyperlink);
            }
            else if (paragraph != null)
            {
                paragraph.Inlines.Add(hyperlink);
            }

            AppendChildren(node, paragraph, hyperlink);
        }

        private void AppendSpan(HtmlNode node, Paragraph paragraph, Span span, string style)
        {
            Span span2 = new Span();

            if (span != null)
            {
                span.Inlines.Add(span2);
            }
            else if (paragraph != null)
            {
                paragraph.Inlines.Add(span2);
            }

            AppendChildren(node, paragraph, span2);
        }

        private void AppendRun(HtmlNode node, Paragraph paragraph, Span span)
        {
            var text = DecodeAndCleanupHtml(node.InnerText);

            if (!string.IsNullOrEmpty(text))
            {
                Run run = new Run();
                run.Text = text;

                if (span != null)
                {
                    span.Inlines.Add(run);
                }
                else if (paragraph != null)
                {
                    paragraph.Inlines.Add(run);
                }
            }

            AppendChildren(node, paragraph, span);
        }


        private string DecodeAndCleanupHtml(string html)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(HttpUtility.HtmlDecode(html.Replace("\n", string.Empty).Replace("\r", string.Empty)));

            builder.Replace("&nbsp;", " ");

            return builder.ToString();
        }
    }
}
