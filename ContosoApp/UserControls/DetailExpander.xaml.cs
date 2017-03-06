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

using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace ContosoApp.UserControls
{
    public sealed partial class DetailExpander : UserControl
    {
        public DetailExpander()
        {
            InitializeComponent();
        }

        public UIElement Pane
        {
            get { return (UIElement)GetValue(PaneProperty); }
            set { SetValue(PaneProperty, value); }
        }

        /// <summary>
        /// Dependency property as the backing store for Pane. This enables animation, styling, binding, etc.
        /// </summary>
        public static readonly DependencyProperty PaneProperty =
            DependencyProperty.Register("Pane", typeof(UIElement), typeof(DetailExpander), new PropertyMetadata(null));

        public UIElement HeaderContent
        {
            get { return (UIElement)GetValue(HeaderContentProperty); }
            set { SetValue(HeaderContentProperty, value); }
        }

        /// <summary>
        /// Dependency property as the backing store for HeaderContent. This enables animation, sytling, binding, etc.
        /// </summary>
        public static readonly DependencyProperty HeaderContentProperty =
            DependencyProperty.Register("HeaderContent", typeof(UIElement), typeof(DetailExpander), new PropertyMetadata(null));

        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }

        /// <summary>
        /// Dependency property as the backing store for Label. This enables animation, sytling, binding, etc.
        /// </summary>
        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register("Label", typeof(string), typeof(DetailExpander), new PropertyMetadata(null));

        public Style LabelStyle
        {
            get { return (Style)GetValue(LabelStyleProperty); }
            set { SetValue(LabelStyleProperty, value); }
        }

        /// <summary>
        /// Dependency property as the backing store for LabelStyleProperty. This enables animation, sytling, binding, etc.
        /// </summary>
        public static readonly DependencyProperty LabelStyleProperty =
            DependencyProperty.Register("LabelStyle", typeof(Style), typeof(DetailExpander), new PropertyMetadata(null));

        private void ToggleButton_Checked(object sender, RoutedEventArgs e) =>
            (sender as ToggleButton).Style = Resources["ExpanderButtonOpenStyle"] as Style;

        private void ToggleButton_Unchecked(object sender, RoutedEventArgs e) =>
            (sender as ToggleButton).Style = Resources["ExpanderButtonClosedStyle"] as Style;

        private void Header_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            Toggle.IsChecked = !Toggle.IsChecked;
        }
    }


    /// <summary>
    /// Converts a nullable bool to bool.
    /// </summary>
    class NullableBooleanConverter : IValueConverter
    {
        /// <summary>
        /// Converts nullable bool to bool.
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, string language) =>
            (bool)value;

        /// <summary>
        /// Converts bool to nullable bool.
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, string language) =>
            (bool)value;
    }
}
