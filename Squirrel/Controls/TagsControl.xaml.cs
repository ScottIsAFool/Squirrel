using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GalaSoft.MvvmLight.Messaging;
using ScottIsAFool.WindowsPhone.Extensions;
using Squirrel.Extensions;

namespace Squirrel.Controls
{
    public partial class TagsControl
    {
        public static readonly DependencyProperty TagsProperty =
            DependencyProperty.Register("Tags", typeof (ObservableCollection<string>), typeof (TagsControl), new PropertyMetadata(default(ObservableCollection<string>)));

        public ObservableCollection<string> Tags
        {
            get { return (ObservableCollection<string>) GetValue(TagsProperty); }
            set { SetValue(TagsProperty, value); }
        }

        public static readonly DependencyProperty SelectedTagsProperty =
            DependencyProperty.Register("SelectedTags", typeof(ObservableCollection<string>), typeof(TagsControl), new PropertyMetadata(default(ObservableCollection<string>)));

        public ObservableCollection<string> SelectedTags
        {
            get { return (ObservableCollection<string>)GetValue(SelectedTagsProperty); }
            set { SetValue(SelectedTagsProperty, value); }
        }

        public static readonly DependencyProperty AcceptCommandProperty =
            DependencyProperty.Register("AcceptCommand", typeof (ICommand), typeof (TagsControl), new PropertyMetadata(default(ICommand)));

        public ICommand AcceptCommand
        {
            get { return (ICommand) GetValue(AcceptCommandProperty); }
            set { SetValue(AcceptCommandProperty, value); }
        }

        public TagsControl()
        {
            InitializeComponent();

            Messenger.Default.Register<NotificationMessage>(this, m =>
            {
                if (m.Notification.Equals(Constants.Messages.AddTagToSelectedMsg))
                {
                    var tagList = m.Sender as List<string>;
                    if (!tagList.IsNullOrEmpty())
                    {
                        foreach (var tag in tagList)
                        {
                            TagSelector.SelectedItems.Add(tag);
                        }
                    }
                }
            });
        }

        public event EventHandler AcceptPressed;
        public event EventHandler CancelPressed;

        private void AcceptButton_OnTap(object sender, GestureEventArgs e)
        {
            if (AcceptPressed != null)
            {
                AcceptPressed(sender, EventArgs.Empty);
            }

            if (AcceptCommand != null)
            {
                AcceptCommand.Execute(null);
            }
        }

        private void CancelButton_OnTap(object sender, GestureEventArgs e)
        {
            if (CancelPressed != null)
            {
                CancelPressed(sender, EventArgs.Empty);
            }
        }

        private void LongListMultiSelector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectedTags == null)
            {
                SelectedTags = new ObservableCollection<string>();
            }

            if (e.AddedItems.Count > 0)
            {
                foreach (var item in e.AddedItems)
                {
                    SelectedTags.Add(item.ToString());
                }
            }

            if (e.RemovedItems.Count > 0)
            {
                foreach (var item in e.RemovedItems)
                {
                    SelectedTags.Remove(item.ToString());
                }
            }
        }
    }
}
