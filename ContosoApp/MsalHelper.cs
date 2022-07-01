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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Extensions.Msal;

namespace Contoso.App
{
    /// <summary>
    /// Handles user authentication and getting user info from the Microsoft Graph API.
    /// </summary>
    public class MsalHelper
    {
        private static readonly IPublicClientApplication _msalPublicClientApp;

        static MsalHelper()
        {
            _msalPublicClientApp = PublicClientApplicationBuilder
                .Create(Repository.Constants.AccountClientId)
                .WithAuthority(AadAuthorityAudience.AzureAdMultipleOrgs)
                .WithDefaultRedirectUri()
                .Build();

            Task.Run(ConfigureCachingAsync);
        }

        private static async void ConfigureCachingAsync()
        {
            // Configuring the token cache
            var storageProperties =
                new StorageCreationPropertiesBuilder(Repository.Constants.CacheFileName, MsalCacheHelper.UserRootDirectory)
                .Build();

            // This hooks up the cache into MSAL
            var cacheHelper = await MsalCacheHelper.CreateAsync(storageProperties);
            cacheHelper.RegisterCache(_msalPublicClientApp.UserTokenCache);
        }

        public static async Task<IEnumerable<IAccount>> GetAccountsAsync() 
            => await _msalPublicClientApp.GetAccountsAsync();

        /// <summary>
        /// Gets an auth token for the user, which can be used to call the Microsoft Graph API.
        /// </summary>
        public static async Task<string> GetTokenAsync(IEnumerable<String> scopes)
        {
            AuthenticationResult msalAuthenticationResult = null;

            // Acquire a cached access token for Microsoft Graph if one is available from a prior
            // execution of this process.
            var accounts = await _msalPublicClientApp.GetAccountsAsync();
            if (accounts.Any())
            {
                try
                {
                    // Will return a cached access token if available, refreshing if necessary.
                    msalAuthenticationResult = await _msalPublicClientApp.AcquireTokenSilent(
                        scopes,
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
                    scopes)
                    .ExecuteAsync();

                // TODO: [feat] when user cancel the authN flow, the UX will be as if the login had failed. This can be improved with a more friendly UI experience on top of this. 
            }

            return msalAuthenticationResult.AccessToken;
        }

        public static async Task RemoveCachedTokens()
        {
            // All cached tokens will be removed.
            // The next token request will require the user to sign in.
            foreach (var account in (await MsalHelper.GetAccountsAsync()).ToList())
            {
                await _msalPublicClientApp.RemoveAsync(account);
            }
        }
    }
}
