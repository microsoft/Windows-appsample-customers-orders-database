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

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using muxc = Microsoft.UI.Xaml.Controls;

namespace Contoso.App.UserControls
{
    public sealed partial class CollapsibleSearchBox : UserControl
    {
        public CollapsibleSearchBox()
        {
            InitializeComponent();
            Loaded += CollapsableSearchBox_Loaded;
            Unloaded += CollapsibleSearchBox_Unloaded;
            Window.Current.SizeChanged += Current_SizeChanged;
            myAutoSuggestBox = SearchBox;
        }

        public double CollapseWidth
        {
            get { return (double)GetValue(CollapseWidthProperty); }
            set { SetValue(CollapseWidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CollapseWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CollapseWidthProperty =
            DependencyProperty.Register("CollapseWidth", typeof(double), typeof(CollapsibleSearchBox), new PropertyMetadata(0.0));

        private AutoSuggestBox myAutoSuggestBox;
        public AutoSuggestBox AutoSuggestBox
        {
            get { return myAutoSuggestBox; }
            private set { myAutoSuggestBox = value; }
        }

        private void CollapsableSearchBox_Loaded(object sender, RoutedEventArgs e)
        {
            SearchButton.AddHandler(PointerPressedEvent,
                new PointerEventHandler(ToggleButton_PointerPressed), true);
            SearchButton.AddHandler(UIElement.PointerReleasedEvent,
                new PointerEventHandler(ToggleButton_PointerReleased), true);

            SetState(Window.Current.Bounds.Width);
        }

        private void CollapsibleSearchBox_Unloaded(object sender, RoutedEventArgs e)
        {
            SearchButton.RemoveHandler(UIElement.PointerPressedEvent,
                (PointerEventHandler)ToggleButton_PointerPressed);
            SearchButton.RemoveHandler(UIElement.PointerReleasedEvent,
                (PointerEventHandler)ToggleButton_PointerReleased);
        }

        private void Current_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            SetState(e.Size.Width);
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            SetState(Window.Current.Bounds.Width);
            SearchButton.IsChecked = false;
        }

        private void SetState(double width)
        {
            if (width <= CollapseWidth)
            {
                VisualStateManager.GoToState(this, "CollapsedState", true);
            }
            else
            {
                VisualStateManager.GoToState(this, "OpenState", true);
            }
        }

        private void SearchButton_Checked(object sender, RoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "OpenState", true);
            if (SearchBox != null)
            {
                SearchBox.Focus(FocusState.Programmatic);
            }
        }

        // Set states for animated icon in toggle button.
        private void ToggleButton_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            muxc.AnimatedIcon.SetState((UIElement)sender, "PointerOver");
        }

        private void ToggleButton_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            muxc.AnimatedIcon.SetState((UIElement)sender, "Pressed");
        }

        private void ToggleButton_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            muxc.AnimatedIcon.SetState((UIElement)sender, "Normal");
        }

        private void ToggleButton_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            muxc.AnimatedIcon.SetState((UIElement)sender, "Normal");
        }

        private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (string.IsNullOrWhiteSpace(sender.Text))
            {
                VisualStateManager.GoToState(this, "NonFilteredState", true);
            }
            else
            {
                VisualStateManager.GoToState(this, "FilteredState", true);
            }
        }
    }
}
