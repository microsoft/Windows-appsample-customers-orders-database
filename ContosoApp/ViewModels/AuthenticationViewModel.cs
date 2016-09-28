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

using Microsoft.Graph;
using PropertyChanged;
using System;
using System.IO;
using System.Linq.Expressions;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Security.Authentication.Web.Core;
using Windows.Security.Credentials;
using Windows.Storage;
using Windows.System;
using Windows.UI.ApplicationSettings;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace ContosoApp.ViewModels
{
    /// <summary>
    /// Handles user authentication and getting user info from the Microsoft Graph API.
    /// </summary>
    [ImplementPropertyChanged]
    public class AuthenticationViewModel
    {
        /// <summary>
        /// The Azure Active Directory (AAD) client id.
        /// </summary>
        private const string AccountClientId = "<TODO: Insert Azure client Id>";

        /// <summary>
        /// Gets or sets the user's name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the user's email.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the user's standard title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the user's AAD domain.
        /// </summary>
        public string Domain { get; set; }

        /// <summary>
        /// Gets or sets the user's photo.
        /// </summary>
        public BitmapImage Photo { get; set; }

        /// <summary>
        /// Gets or sets error text to show if the login operation fails.
        /// </summary>
        public string ErrorText { get; set; }

        /// <summary>
        /// Gets or sets whether to show the starting welcome UI. 
        /// </summary>
        public bool ShowWelcome { get; set; }

        /// <summary>
        /// Gets or sets whether to show the logging in progress UI.
        /// </summary>
        public bool ShowLoading { get; set; }

        /// <summary>
        /// Gets or sets whether to show user data UI.
        /// </summary>
        public bool ShowData { get; set; }

        /// <summary>
        /// Gets or sets whether to show the error UI.
        /// </summary>
        public bool ShowError { get; set; }

        /// <summary>
        /// Creates a new AuthenticationViewModel for logging users in and getting their info.
        /// </summary>
        public AuthenticationViewModel()
        {
            Task.Run(PrepareAsync);
            AccountsSettingsPane.GetForCurrentView().AccountCommandsRequested += BuildAccountsPane;
        }

        /// <summary>
        /// Prepares the login sequence. 
        /// </summary>
        public async Task PrepareAsync()
        {
            if (ApplicationData.Current.RoamingSettings.Values.ContainsKey("IsLoggedIn") &&
                (bool)ApplicationData.Current.RoamingSettings.Values["IsLoggedIn"])
            {
                await SetVisibleAsync(x => x.ShowLoading);
                await LoginAsync();
            }
            else
            {
                await SetVisibleAsync(x => x.ShowWelcome);
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
                await SetVisibleAsync(x => x.ShowLoading);
                string token = await GetTokenAsync();
                if (token != null)
                {
                    ApplicationData.Current.RoamingSettings.Values["IsLoggedIn"] = true;
                    await SetUserInfoAsync(token);
                    await SetUserPhoto(token);
                    await SetVisibleAsync(x => x.ShowData);
                }
                else
                {
                    await SetVisibleAsync(x => x.ShowError);
                }
            }
            catch (Exception ex)
            {
                ErrorText = ex.Message;
                await SetVisibleAsync(x => x.ShowError);
            }
        }

        /// <summary>
        /// Gets an auth token for the user, which can be used to call the Microsoft Graph API.
        /// </summary>
        private async Task<string> GetTokenAsync()
        {
            var provider = await GetAadProviderAsync();
            var request = new WebTokenRequest(provider, "User.Read", AccountClientId);
            request.Properties.Add("resource", "https://graph.microsoft.com");
            var result = await WebAuthenticationCoreManager.GetTokenSilentlyAsync(request);
            if (result.ResponseStatus != WebTokenRequestStatus.Success)
            {
                result = await WebAuthenticationCoreManager.RequestTokenAsync(request);
            }
            return result.ResponseStatus == WebTokenRequestStatus.Success ?
                result.ResponseData[0].Token : null;
        }

        /// <summary>
        /// Gets and processes the user's info from the Microsoft Graph API.
        /// </summary>
        private async Task SetUserInfoAsync(string token)
        {
            var users = await Windows.System.User.FindAllAsync();
            var graph = new GraphServiceClient(new DelegateAuthenticationProvider((x) =>
            {
                x.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                return Task.CompletedTask;
            }));

            var me = await graph.Me.Request().GetAsync();

            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal, async () =>
            {
                Name = me.DisplayName;
                Email = me.Mail;
                Title = me.JobTitle;
                Domain = (string)await users[0].GetPropertyAsync(KnownUserProperties.DomainName);
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
                        await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                            CoreDispatcherPriority.Normal, async () =>
                        {
                            Photo = new BitmapImage();
                            await Photo.SetSourceAsync(memoryStream.AsRandomAccessStream());
                        });
                    }
                }
            }
        }

        /// <summary>
        /// Initializes the AccountsSettingsPane with AAD login.
        /// </summary>
        private async void BuildAccountsPane(AccountsSettingsPane sender,
            AccountsSettingsPaneCommandsRequestedEventArgs args)
        {
            var deferral = args.GetDeferral();
            var command = new WebAccountProviderCommand(await GetAadProviderAsync(), async (x) =>
                await LoginAsync());
            args.WebAccountProviderCommands.Add(command);
            deferral.Complete();
        }

        /// <summary>
        /// Gets the Microsoft ADD login provider.
        /// </summary>
        public async Task<WebAccountProvider> GetAadProviderAsync() =>
            await WebAuthenticationCoreManager.FindAccountProviderAsync(
                "https://login.microsoft.com", "organizations");


        /// <summary>
        /// Logs the user in.
        /// </summary>
        public async void LoginClick()
        {
            if (ApplicationData.Current.RoamingSettings.Values.ContainsKey("IsLoggedIn") &&
                (bool)ApplicationData.Current.RoamingSettings.Values["IsLoggedIn"])
            {
                await LoginAsync();
            }
            else
            {
                AccountsSettingsPane.Show();
            }
        }

        /// <summary>
        /// Logs the user out.
        /// </summary>
        public async void LogoutClick()
        {
            if (ApplicationData.Current.RoamingSettings.Values.ContainsKey("IsLoggedIn") &&
                (bool)ApplicationData.Current.RoamingSettings.Values["IsLoggedIn"])
            {
                ContentDialog SignoutDialog = new ContentDialog()
                {
                    Title = "Sign out",
                    Content = "Sign out?",
                    PrimaryButtonText = "Sign out",
                    SecondaryButtonText = "Cancel"

                };
                await SignoutDialog.ShowAsync();
            }
        }

        /// <summary>
        /// Shows one part of the login UI sequence and hides all the others.
        /// </summary>
        private async Task SetVisibleAsync(Expression<Func<AuthenticationViewModel, bool>> selector)
        {
            var prop = (PropertyInfo)((MemberExpression)selector.Body).Member;
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal, () =>
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
