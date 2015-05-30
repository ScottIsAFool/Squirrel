using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Interactivity;
using Squirrel.Model;
using Telerik.Windows.Controls;

namespace Squirrel.Behaviours
{
    public class AddToCheckedItemBehaviorBase<T> : Behavior<RadDataBoundListBox> where T : class
    {
        public bool TurnSelectionOffOnEmpty { get; set; }
        public bool AddTappedItemOnOpen { get; set; }
        public virtual List<T> SelectedItems { get; set; }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.ItemCheckedStateChanged += AssociatedObject_ItemCheckedStateChanged;
            AssociatedObject.IsCheckModeActiveChanged += AssociatedObjectOnIsCheckModeActiveChanged;
        }

        private void AssociatedObjectOnIsCheckModeActiveChanged(object sender, IsCheckModeActiveChangedEventArgs e)
        {
            if (!e.CheckBoxesVisible || e.TappedItem == null || !AddTappedItemOnOpen)
            {
                return;
            }

            if (SelectedItems == null)
            {
                SelectedItems = new List<T>();
            }

            var item = e.TappedItem as T;
            if (item == null)
            {
                return;
            }

            if (!SelectedItems.Contains(item))
            {
                SelectedItems.Add(item);
            }

            AssociatedObject.CheckedItems.Add(item);
        }

        private void AssociatedObject_ItemCheckedStateChanged(object sender, ItemCheckedStateChangedEventArgs e)
        {
            if (SelectedItems == null)
            {
                SelectedItems = new List<T>();
            }

            var item = e.Item as T ?? ((Telerik.Windows.Data.IDataSourceItem)e.Item).Value as T;

            if (item == null)
            {
                return;
            }

            if (e.IsChecked)
            {
                if (SelectedItems.Contains(item))
                {
                    return;
                }

                SelectedItems.Add(item);
            }
            else
            {
                SelectedItems.Remove(item);

                if (SelectedItems != null && !SelectedItems.Any() && TurnSelectionOffOnEmpty)
                {
                    AssociatedObject.IsCheckModeActive = false;
                }
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.ItemCheckedStateChanged -= AssociatedObject_ItemCheckedStateChanged;
            AssociatedObject.IsCheckModeActiveChanged -= AssociatedObjectOnIsCheckModeActiveChanged;
        }
    }

    public class AddItemsToCheckedItems : AddToCheckedItemBehaviorBase<ExtendedPocketItem>
    {
        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.Register("SelectedItems", typeof(List<ExtendedPocketItem>), typeof(AddItemsToCheckedItems), new PropertyMetadata(default(List<ExtendedPocketItem>)));

        public override List<ExtendedPocketItem> SelectedItems
        {
            get { return (List<ExtendedPocketItem>)GetValue(SelectedItemsProperty); }
            set { SetValue(SelectedItemsProperty, value); }
        }

        public AddItemsToCheckedItems()
        {
            AddTappedItemOnOpen = true;
            TurnSelectionOffOnEmpty = true;
        }
    }
}
