﻿// ****************************************************************************
// <copyright file="AutoFocusBehavior.cs" company="Pedro Lamas">
// Copyright © Pedro Lamas 2013
// </copyright>
// ****************************************************************************
// <author>Pedro Lamas</author>
// <email>pedrolamas@gmail.com</email>
// <date>09-08-2013</date>
// <project>Cimbalino.Phone.Toolkit</project>
// <web>http://www.pedrolamas.com</web>
// <license>
// See license.txt in this solution or http://www.pedrolamas.com/license_MIT.txt
// </license>
// ****************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Cimbalino.Phone.Toolkit.Behaviors;
using Cimbalino.Phone.Toolkit.Extensions;
using Telerik.Windows.Controls;

namespace Squirrel.Behaviours
{
    /// <summary>
    /// The behavior that enables automatic control focus.
    /// </summary>
    public class RadAutoFocusBehavior : SafeBehavior<FrameworkElement>
    {
        /// <summary>
        /// Occurs when the focus automatically moves from one control to the next.
        /// </summary>
        public event EventHandler<AfterAutoFocusEventArgs> AfterAutoFocus;

        /// <summary>
        /// Called after the behavior is attached to an AssociatedObject.
        /// </summary>
        /// <remarks>Override this to hook up functionality to the AssociatedObject.</remarks>
        protected override void OnAttached()
        {
            AssociatedObject.KeyUp += AssociatedObjectKeyUp;

            base.OnAttached();
        }

        /// <summary>
        /// Releases all resources used by this instance.
        /// </summary>
        protected override void CleanUp()
        {
            AssociatedObject.KeyUp -= AssociatedObjectKeyUp;

            base.CleanUp();
        }

        /// <summary>
        /// Gets or sets a value indicating whether focus will jump from last control to first one.
        /// </summary>
        /// <value>true if focus will jump from last control to first one; otherwise, false.</value>
        public bool CycleNavigation
        {
            get { return (bool)GetValue(CycleNavigationProperty); }
            set { SetValue(CycleNavigationProperty, value); }
        }

        /// <summary>
        /// Identifier for the <see cref="CycleNavigation" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty CycleNavigationProperty =
            DependencyProperty.Register("CycleNavigation", typeof(bool), typeof(AutoFocusBehavior), new PropertyMetadata(false));

        /// <summary>
        /// Gets or sets a value indicating whether the entire contents of a control will be selected on focus.
        /// </summary>
        /// <value>true if the entire contents of a control will be selected on focus; otherwise, false.</value>
        public bool SelectAllOnFocus
        {
            get { return (bool)GetValue(SelectAllOnFocusProperty); }
            set { SetValue(SelectAllOnFocusProperty, value); }
        }

        /// <summary>
        /// Identifier for the <see cref="SelectAllOnFocus" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectAllOnFocusProperty =
            DependencyProperty.Register("SelectAllOnFocus", typeof(bool), typeof(AutoFocusBehavior), new PropertyMetadata(false));

        /// <summary>
        /// Gets or sets the command to invoke when the focus moves from one control to the next.
        /// </summary>
        /// <value>The command to invoke when the focus moves from one control to the next.</value>
        public ICommand AfterAutoFocusCommand
        {
            get { return (ICommand)GetValue(AfterAutoFocusCommandProperty); }
            set { SetValue(AfterAutoFocusCommandProperty, value); }
        }

        /// <summary>
        /// Identifier for the <see cref="AfterAutoFocusCommand" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty AfterAutoFocusCommandProperty =
            DependencyProperty.Register("AfterAutoFocusCommand", typeof(ICommand), typeof(AutoFocusBehavior), null);

        private void AssociatedObjectKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && (e.OriginalSource is TextBox || e.OriginalSource is PasswordBox || e.OriginalSource is RadTextBox || e.OriginalSource is RadPasswordBox))
            {
                var originalSourceTextBox = e.OriginalSource as TextBox;

                if (originalSourceTextBox == null || !originalSourceTextBox.AcceptsReturn)
                {
                    FocusNextControl((Control)e.OriginalSource);
                }
            }
        }

        private void FocusNextControl(Control fromControl)
        {
            var controls = GetChildControls(AssociatedObject)
                .OrderBy(x => x.TabIndex)
                .ToArray();

            var toControl = controls
                .SkipWhile(x => x != fromControl)
                .Skip(1)
                .FirstOrDefault();

            if (toControl == null && CycleNavigation)
            {
                toControl = controls.FirstOrDefault();
            }

            if (toControl == null)
            {
                var page = fromControl.GetVisualAncestor<Page>();

                if (page != null)
                {
                    page.Focus();
                }
            }
            else
            {
                toControl.Focus();

                if (SelectAllOnFocus)
                {
                    var textBox = toControl as TextBox;

                    if (textBox != null)
                    {
                        textBox.SelectAll();
                    }
                    else
                    {
                        var passwordBox = toControl as PasswordBox;

                        if (passwordBox != null)
                        {
                            passwordBox.SelectAll();
                        }
                    }
                }
            }

            var eventArgs = new AfterAutoFocusEventArgs(fromControl, toControl);

            var eventHandler = AfterAutoFocus;

            if (eventHandler != null)
            {
                eventHandler(this, eventArgs);
            }

            var command = AfterAutoFocusCommand;

            if (command != null)
            {
                command.Execute(eventArgs);
            }
        }

        private IEnumerable<Control> GetChildControls(FrameworkElement control)
        {
            return GetVisibleVisualChilds(control)
                .Descendants(x => (x is TextBox || x is PasswordBox || x is RadPasswordBox) ? new FrameworkElement[] { } : GetVisibleVisualChilds(x))
                .OfType<Control>()
                .Where(x => x.IsEnabled && x.IsTabStop && (x is TextBox || x is PasswordBox || x is RadPasswordBox));
        }

        private IEnumerable<FrameworkElement> GetVisibleVisualChilds(FrameworkElement frameworkElement)
        {
            return frameworkElement.GetVisualChilds<FrameworkElement>()
                .Where(x => x.Visibility == Visibility.Visible);
        }
    }
}