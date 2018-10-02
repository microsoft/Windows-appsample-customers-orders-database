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

using Contoso.Repository.Sql;
using System;
using Windows.Storage;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Contoso.App.Views
{
    public sealed partial class SettingsPage : Page
    {
        public const string DataSourceKey = "data_source"; 

        /// <summary>
        /// Initializes a new instance of the SettingsPage class.
        /// </summary>
        public SettingsPage()
        {
            InitializeComponent();

            if (App.Repository.GetType() == typeof(SqlContosoRepository))
            {
                SqliteRadio.IsChecked = true;
            }
            else
            {
                RestRadio.IsChecked = true; 
            }
        }

        /// <summary>
        /// Changes the app's data source.
        /// </summary>
        private void OnDataSourceChanged(object sender, RoutedEventArgs e)
        {
            var radio = (RadioButton)sender; 
            switch (radio.Tag)
            {
                case "Sqlite": App.UseSqlite(); break;
                case "Rest": App.UseRest(); break;
                default: throw new NotImplementedException(); 
            }
            ApplicationData.Current.LocalSettings.Values[DataSourceKey] = radio.Tag; 
        }

        /// <summary>
        ///  Launches the privacy statement in the user's default browser.
        /// </summary>
        private async void OnPrivacyButtonClick(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("https://go.microsoft.com/fwlink/?LinkId=521839"));
        }

        /// <summary>
        /// Launches the license terms in the user's default browser.
        /// </summary>
        private async void OnLicenseButtonClick(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("https://go.microsoft.com/fwlink/?LinkId=822631"));
        }

        /// <summary>
        /// Launches the sample's GitHub page in the user's default browser.
        /// </summary>
        private async void OnCodeButtonClick(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("https://github.com/Microsoft/Windows-appsample-customers-orders-database"));
        }
    }
}
