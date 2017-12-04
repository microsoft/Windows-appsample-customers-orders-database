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

using Contoso.App.Diagnostics;
using Contoso.App.Views;
using Contoso.Repository;
using Contoso.Repository.Sql;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.IO;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Globalization;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;

namespace Contoso.App
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        private readonly string _demoDatabasePath = Package.Current.InstalledLocation.Path + @"\Assets\Contoso.db";
        private readonly string _databasePath = ApplicationData.Current.LocalFolder.Path + @"\Contoso.db";

        /// <summary>
        /// Pipeline for interacting with backend service or database.
        /// </summary>
        public static IContosoRepository Repository { get; private set; }

        /// <summary>
        /// Service for collecting feedback and diagnostic data.
        /// </summary>
        public static DiagnosticService Diagnostics { get; private set; }

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.
        /// </summary>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            // Start app diagnostics.

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            Diagnostics = new DiagnosticService();

            // Load the database. If one doesn't already exist, copy over a demo one. 

            if (!File.Exists(_databasePath))
            {
                File.Copy(_demoDatabasePath, _databasePath);
            }
            var dbOptions = new DbContextOptionsBuilder<ContosoContext>().UseSqlite(
                "Data Source=" + _databasePath);
            Repository = new SqlContosoRepository(dbOptions);

            // Prepare the app shell and window content.

            AppShell shell = Window.Current.Content as AppShell ?? new AppShell();
            shell.Language = ApplicationLanguages.Languages[0];
            Window.Current.Content = shell;

            if (shell.AppFrame.Content == null)
            {
                // When the navigation stack isn't restored, navigate to the first page
                // suppressing the initial entrance animation.

                shell.AppFrame.Navigate(typeof(CustomerListPage), e.Arguments,
                    new SuppressNavigationTransitionInfo());
            }

            Window.Current.Activate();
            Diagnostics.TrackLaunch(stopwatch.Elapsed);
        }
    }
}