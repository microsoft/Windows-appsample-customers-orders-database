using Contoso.Models;
using System;
using Windows.UI.Xaml;

namespace Contoso.App.ViewModels
{
    public static class Converters
    {
        public static bool Not(bool value) => !value;

        public static bool IsNotNull(object value) => value != null;

        public static Visibility CollapsedIf(bool value) =>
            value ? Visibility.Collapsed : Visibility.Visible;

        public static Visibility CollapsedIfNull(object value) =>
            value == null ? Visibility.Collapsed : Visibility.Visible;

        public static Visibility CollapsedIfNullOrEmpty(string value) =>
            string.IsNullOrEmpty(value) ? Visibility.Collapsed : Visibility.Visible;
    }
}
