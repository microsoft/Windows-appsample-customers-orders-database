using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Contoso.App.Navigation
{
    public sealed partial class NavMenuSecondaryButton : Button
    {
        public NavMenuSecondaryButton()
        {
            this.InitializeComponent();
        }

        public string Icon
        {
            get => (string)GetValue(IconProperty); 
            set => SetValue(IconProperty, value); 
        }

        public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
            nameof(Icon),
            typeof(double),
            typeof(NavMenuSecondaryButton),
            null);

        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
            nameof(Label),
            typeof(double),
            typeof(NavMenuSecondaryButton),
            null);
    }
}
