//  ---------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// 
//  The MIT License (MIT)
// 
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
// 
//  The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
// 
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.
//  ---------------------------------------------------------------------------------

using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace ContosoApp.UserControls
{
    class ContosoListView : ListView
    {
        // For context menu handling.
        private bool _isShiftPressed = false;
        private bool _isPointerPressed = false;
        // This API check has a small cost, so just do it once and save the result.
        private bool _isContextFlyoutAPIPresent = false;
        

        public MenuFlyout ContextMenu
        {
            get { return (MenuFlyout)GetValue(ContextMenuProperty); }
            set { SetValue(ContextMenuProperty, value); }
        }

        /// <summary>
        /// Property for the backing store for ContextMenu. This enables animation, styling, binging, etc.
        /// </summary>
        public static readonly DependencyProperty ContextMenuProperty =
            DependencyProperty.Register("ContextMenu", typeof(MenuFlyout), typeof(ContosoListView), new PropertyMetadata(null));


        public ContosoListView()
        {
        }

        /// <summary>
        /// Builds a visual template for the list view.
        /// </summary>
        protected override void OnApplyTemplate()
        {
            if (_isContextFlyoutAPIPresent == true && ContextMenu != null)
            {
                ContextMenu.Opened += (s, e) =>
                {
                    (ContextMenu.Target as ListViewItem).DataContext = (ContextMenu.Target as ListViewItem).Content;
                };

                var listViewItemStyle = new Style();
                listViewItemStyle.TargetType = typeof(ListViewItem);
                listViewItemStyle.Setters.Add(new Setter(ContextFlyoutProperty, ContextMenu));

                ItemContainerStyle = listViewItemStyle;
            }

            base.OnApplyTemplate();
        }

        #region Handlers for down-level systems
        /// <summary>
        /// Handles key down keyboard control for the context menu.
        /// </summary>
        protected override void OnKeyDown(KeyRoutedEventArgs e)
        {
            if (_isContextFlyoutAPIPresent == false)
            {
                // Handle Shift+F10 and the Menu key.
                if (e.Key == Windows.System.VirtualKey.Shift)
                {
                    _isShiftPressed = true;
                }

                // Shift+F10
                // The 'Menu' key next to Right Ctrl on most keyboards
                else if ((_isShiftPressed && e.Key == Windows.System.VirtualKey.F10) ||
                          e.Key == Windows.System.VirtualKey.Application)
                {
                    var focusedElement = FocusManager.GetFocusedElement() as UIElement;
                    ShowContextMenu(focusedElement, new Point(0, 0));
                    e.Handled = true;
                }
            }

            base.OnKeyDown(e);
        }

        /// <summary>
        /// Handles key up keyboard control for the context menu.
        /// </summary>
        protected override void OnKeyUp(KeyRoutedEventArgs e)
        {
            if (_isContextFlyoutAPIPresent == false)
            {
                if (e.Key == Windows.System.VirtualKey.Shift)
                {
                    _isShiftPressed = false;
                }
            }

            base.OnKeyUp(e);
        }

        /// <summary>
        /// Handles key holding control for the context menu.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnHolding(HoldingRoutedEventArgs e)
        {
            if (_isContextFlyoutAPIPresent == false)
            {
                // Responding to HoldingState.Started will show a context menu while your finger is still down, while 
                // HoldingState.Completed will wait until you have removed your finger. 
                if (e.HoldingState == Windows.UI.Input.HoldingState.Completed)
                {
                    ListViewItem container = null;

                    // Find the ListViewItem that is pressed. Also cancel direct manipulations on it to
                    // prevent any scrollviewers from continuing to pan once the context menu is displayed.    
                    var pressedElements = VisualTreeHelper.FindElementsInHostCoordinates(e.GetPosition(null), this);
                    foreach (var element in pressedElements)
                    {
                        if (element is ListViewItem)
                        {
                            element.CancelDirectManipulations();
                            container = element as ListViewItem;
                        }
                    }

                    if (container != null)
                    {
                        ShowContextMenu(container, e.GetPosition(container));
                    }

                    e.Handled = true;

                    // This, combined with a check in OnRightTapped prevents the firing of RightTapped from
                    // launching another context menu.
                    _isPointerPressed = false;
                }
            }

            base.OnHolding(e);
        }

        /// <summary>
        /// Handles pointer control for the context menu. 
        /// </summary>
        protected override void OnPointerPressed(PointerRoutedEventArgs e)
        {
            if (_isContextFlyoutAPIPresent == false)
            {
                _isPointerPressed = true;
            }

            base.OnPointerPressed(e);
        }

        /// <summary>
        /// Handles right click pointer control for the context menu
        /// </summary>
        protected override void OnRightTapped(RightTappedRoutedEventArgs e)
        {
            if (_isContextFlyoutAPIPresent == false)
            {
                if (_isPointerPressed)
                {
                    var focusedElement = FocusManager.GetFocusedElement() as ListViewItem;

                    if (focusedElement != null)
                    {
                        ShowContextMenu(focusedElement, e.GetPosition(focusedElement));
                    }

                    e.Handled = true;
                }
            }

            base.OnRightTapped(e);
        }

        /// <summary>
        /// Shows the context menu when called.
        /// </summary>
        private void ShowContextMenu(UIElement target, Point offset)
        {
            if (ContextMenu != null)
            {
                if (target is ContentControl)
                {
                    (target as ContentControl).DataContext = (target as ContentControl).Content;
                }

                ContextMenu.ShowAt(target, offset);
            }
        } 
        #endregion
    }
}
