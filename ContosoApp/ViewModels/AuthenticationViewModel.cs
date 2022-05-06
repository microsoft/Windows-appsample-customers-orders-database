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
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage;

namespace Contoso.App.ViewModels
{
    /// <summary>
    /// Handles user authentication and getting user info from the Microsoft Graph API.
    /// </summary>
    public class AuthenticationViewModel : BindableBase
    {
        // Generally, your MSAL client will have a lifecycle that matches the lifecycle
        // of the user's session in the application. In this sample, the lifecycle of the
        // MSAL client to the lifecycle of this form.
        private readonly IPublicClientApplication _msalPublicClientApp;

        /// <summary>
        /// Creates a new AuthenticationViewModel for logging users in and getting their info.
        /// </summary>
        public AuthenticationViewModel()
        {
            _msalPublicClientApp = PublicClientApplicationBuilder
                .Create(Repository.Constants.AccountClientId)
                .WithAuthority(AadAuthorityAudience.AzureAdMultipleOrgs)
                .WithDefaultRedirectUri()
                .Build();

            Task.Run(PrepareAsync);
        }

        private string _name;

        /// <summary>
        /// Gets or sets the user's name.
        /// </summary>
        public string Name
        {
            get => _name;
            set => Set(ref _name, value);
        }

        private string _email;

        /// <summary>
        /// Gets or sets the user's email.
        /// </summary>
        public string Email
        {
            get => _email;
            set => Set(ref _email, value);
        }

        private string _title;

        /// <summary>
        /// Gets or sets the user's standard title.
        /// </summary>
        public string Title
        {
            get => _title;
            set => Set(ref _title, value);
        }

        private string _domain;

        /// <summary>
        /// Gets or sets the user's AAD domain.
        /// </summary>
        public string Domain
        {
            get => _domain;
            set => Set(ref _domain, value);
        }

        private BitmapImage _photo;

        /// <summary>
        /// Gets or sets the user's photo.
        /// </summary>
        public BitmapImage Photo
        {
            get => _photo;
            set => Set(ref _photo, value);
        }

        private string _errorText;

        /// <summary>
        /// Gets or sets error text to show if the login operation fails.
        /// </summary>
        public string ErrorText
        {
            get => _errorText;
            set => Set(ref _errorText, value);
        }

        private bool _showWelcome;

        /// <summary>
        /// Gets or sets whether to show the starting welcome UI. 
        /// </summary>
        public bool ShowWelcome
        {
            get => _showWelcome;
            set => Set(ref _showWelcome, value);
        }

        private bool _showLoading;

        /// <summary>
        /// Gets or sets whether to show the logging in progress UI.
        /// </summary>
        public bool ShowLoading
        {
            get => _showLoading;
            set => Set(ref _showLoading, value);
        }

        private bool _showData;

        /// <summary>
        /// Gets or sets whether to show user data UI.
        /// </summary>
        public bool ShowData
        {
            get => _showData;
            set => Set(ref _showData, value);
        }

        private bool _showError;

        /// <summary>
        /// Gets or sets whether to show the error UI.
        /// </summary>
        public bool ShowError
        {
            get => _showError;
            set => Set(ref _showError, value);
        }

        /// <summary>
        /// Prepares the login sequence. 
        /// </summary>
        public async Task PrepareAsync()
        {
            if (ApplicationData.Current.RoamingSettings.Values.ContainsKey("IsLoggedIn") &&
                (bool)ApplicationData.Current.RoamingSettings.Values["IsLoggedIn"])
            {
                SetVisibleAsync(vm => vm.ShowLoading);
                await LoginAsync();
            }
            else
            {
                SetVisibleAsync(vm => vm.ShowWelcome);
            }
        }

        /// <summary>
        /// Logs the user in by requesting a token and using it to query the 
        /// Microsoft Graph API.
        /// </summary>
        public async Task LoginAsync()
        {
            try
            {
                SetVisibleAsync(vm => vm.ShowLoading);
                string token = await GetTokenAsync();
                if (token != null)
                {
                    ApplicationData.Current.RoamingSettings.Values["IsLoggedIn"] = true;
                    await SetUserInfoAsync(token);
                    await SetUserPhoto(token);
                    SetVisibleAsync(vm => vm.ShowData);
                }
                else
                {
                    SetVisibleAsync(vm => vm.ShowError);
                }
            }
            catch (Exception ex)
            {
                ErrorText = ex.Message;
                SetVisibleAsync(vm => vm.ShowError);
            }
        }

        /// <summary>
        /// Gets an auth token for the user, which can be used to call the Microsoft Graph API.
        /// </summary>
        private async Task<string> GetTokenAsync()
        {
            AuthenticationResult? msalAuthenticationResult = null;

            // Acquire a cached access token for Microsoft Graph if one is available from a prior
            // execution of this process.
            var accounts = await _msalPublicClientApp.GetAccountsAsync();
            if (accounts.Any())
            {
                try
                {
                    // Will return a cached access token if available, refreshing if necessary.
                    msalAuthenticationResult = await _msalPublicClientApp.AcquireTokenSilent(
                        new[] { "https://graph.microsoft.com/User.Read" },
                        accounts.First())
                        .ExecuteAsync();
                }
                catch (MsalUiRequiredException)
                {
                    // Nothing in cache for this account + scope, and interactive experience required.
                }
            }

            if (msalAuthenticationResult == null)
            {
                // This is likely the first authentication request in the application, so calling
                // this will launch the user's default browser and send them through a login flow.
                // After the flow is complete, the rest of this method will continue to execute.
                msalAuthenticationResult = await _msalPublicClientApp.AcquireTokenInteractive(
                    new[] { "https://graph.microsoft.com/User.Read" })
                    .ExecuteAsync();

                // TODO: [feat] when user cancel the authN flow, the UX will be as if the login had failed. This can be improved with a more friendly UI experience on top of this. 
            }

            return msalAuthenticationResult.AccessToken;
        }

        /// <summary>
        /// Gets and processes the user's info from the Microsoft Graph API.
        /// </summary>
        private async Task SetUserInfoAsync(string token)
        {
            var users = await Windows.System.User.FindAllAsync();
            var graph = new GraphServiceClient(new DelegateAuthenticationProvider(message =>
            {
                message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                return Task.CompletedTask;
            }));

            var me = await graph.Me.Request().GetAsync();

            App.Window.DispatcherQueue.TryEnqueue(
                DispatcherQueuePriority.Normal, async () =>
            {
                Name = me.DisplayName;
                Email = me.Mail;
                Title = me.JobTitle;
                Domain = (string)await users[0].GetPropertyAsync(Windows.System.KnownUserProperties.DomainName);
            });
        }

        /// <summary>
        /// Gets and processes the user's photo from the Microsoft Graph API. 
        /// </summary>
        private async Task SetUserPhoto(string token)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                string url = "https://graph.microsoft.com/beta/me/photo/$value";
                var result = await client.GetAsync(url);
                if (!result.IsSuccessStatusCode)
                {
                    return;
                }
                using (Stream stream = await result.Content.ReadAsStreamAsync())
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await stream.CopyToAsync(memoryStream);
                        memoryStream.Position = 0;
                        App.Window.DispatcherQueue.TryEnqueue(
                            DispatcherQueuePriority.Normal, async () =>
                        {
                            Photo = new BitmapImage();
                            await Photo.SetSourceAsync(memoryStream.AsRandomAccessStream());
                        });
                    }
                }
            }
        }

        /// <summary>
        /// Logs the user in.
        /// </summary>
        public async void LoginClick()
        {
            await LoginAsync();
        }

        /// <summary>
        /// Logs the user out.
        /// </summary>
        public async void LogoutClick()
        {
            if (ApplicationData.Current.RoamingSettings.Values.ContainsKey("IsLoggedIn") &&
                (bool)ApplicationData.Current.RoamingSettings.Values["IsLoggedIn"])
            {
                // All cached tokens will be removed.
                // The next token request will require the user to sign in.
                foreach (var account in (await _msalPublicClientApp.GetAccountsAsync()).ToList())
                {
                    await _msalPublicClientApp.RemoveAsync(account);
                }
                ApplicationData.Current.RoamingSettings.Values["IsLoggedIn"] = false;
                SetVisibleAsync(vm => vm.ShowWelcome);
            }
        }

        /// <summary>
        /// Shows one part of the login UI sequence and hides all the others.
        /// </summary>
        private void SetVisibleAsync(Expression<Func<AuthenticationViewModel, bool>> selector)
        {
            var prop = (PropertyInfo)((MemberExpression)selector.Body).Member;

            App.Window.DispatcherQueue.TryEnqueue(
                DispatcherQueuePriority.High, () =>
            {
                ShowWelcome = false;
                ShowLoading = false;
                ShowData = false;
                ShowError = false;
                prop.SetValue(this, true);
            });
        }
    }
}
