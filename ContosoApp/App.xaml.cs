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

using Contoso.App.Views;
using Contoso.Repository;
using Contoso.Repository.Rest;
using Contoso.Repository.Sql;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
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
        private readonly DbContextOptionsBuilder<ContosoContext> _dbOptions = new DbContextOptionsBuilder<ContosoContext>()
            .UseSqlite("Data Source=" + Path.Combine(ApplicationData.Current.LocalFolder.Path, "Contoso.db")); 

        /// <summary>
        /// Pipeline for interacting with backend service or database.
        /// </summary>
        public static IContosoRepository Repository { get; private set; }

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
            Repository = new SqlContosoRepository(_dbOptions);

            // The first time we launch the sample, ensure the database is populated with 
            // demo data.
            await PrepareDemoAsync(); 

            AppShell shell = Window.Current.Content as AppShell;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (shell == null)
            {
                // Create a AppShell to act as the navigation context and navigate to the first page
                shell = new AppShell();
                // Set the default language
                shell.Language = ApplicationLanguages.Languages[0];
                shell.AppFrame.NavigationFailed += (s, args) =>
                    new Exception("Failed to load Page " + args.SourcePageType.FullName);
            }
            // Place our app shell in the current Window
            Window.Current.Content = shell;
            if (shell.AppFrame.Content == null)
            {
                // When the navigation stack isn't restored, navigate to the first page
                // suppressing the initial entrance animation.
                shell.AppFrame.Navigate(typeof(CustomerListPage), e.Arguments, 
                    new SuppressNavigationTransitionInfo());
            }
            // Ensure the current window is active
            Window.Current.Activate();
        }

        /// <summary>
        /// Creates a local Sqlite database and loads demo data into it. 
        /// </summary>
        private async Task PrepareDemoAsync()
        {
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("is_demo_loaded"))
            {
                return; 
            }

            var repository = new RestContosoRepository(Constants.ApiUrl);

            var customersTask = repository.Customers.GetAsync();
            var ordersTask = repository.Orders.GetAsync();
            var productsTask = repository.Products.GetAsync();

            await Task.WhenAll(customersTask, ordersTask, productsTask);

            var db = new ContosoContext(_dbOptions.Options);

            await db.Customers.AddRangeAsync(customersTask.Result);
            await db.Products.AddRangeAsync(productsTask.Result);
            await db.Orders.AddRangeAsync(ordersTask.Result);

            await db.SaveChangesAsync();

            ApplicationData.Current.LocalSettings.Values.Add("is_demo_loaded", true);  
        }
    }
}