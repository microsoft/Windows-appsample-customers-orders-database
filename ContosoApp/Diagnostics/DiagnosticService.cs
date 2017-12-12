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

using Contoso.App.Diagnostics.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace Contoso.App.Diagnostics
{
    /// <summary>
    /// Contains methods for tracking diagnostic app information, such as page views and
    /// errors. We use this information to help make our code samples better. No information 
    /// is collected that can personally identify you. To opt-out of diagnostic data collection,
    /// comment out this code or using the toggle switch on the app's Settings page. 
    /// For a a complete copy of Microsoft's privacy statement, see: 
    /// https://privacy.microsoft.com/privacystatement
    /// <para />
    /// Note: Do not copy/paste this code into your own app; it will send data to our diganostic 
    /// services instead of yours. If you are looking for a premade instrumentation solution, 
    /// consider HockeyApp or Google Analytics, both of which have a UWP SDK. 
    /// </summary>
    public class DiagnosticService
    {
        private const string FeedbackProvidedKey = "feedback_provided";
        private const string UserIdKey = "diag_user_id";
        private const string IsDiagnosticsEnabledKey = "diag_enabled";
        private const string EndpointUrl = "https://sample-diagnostics-api.azurewebsites.net/";

        private readonly List<Diagnostic> _events = new List<Diagnostic>();
        private readonly Stopwatch _appFocusTime = new Stopwatch();
        private readonly Stopwatch _appOpenTime = new Stopwatch();
        private readonly Stopwatch _pageOpenTime = new Stopwatch();
        private readonly Stopwatch _pageFocusTime = new Stopwatch();
        private readonly Guid _sessionId = Guid.NewGuid();
        private readonly Guid _userId;

        private Type _currentPage;

        /// <summary>
        /// Creates a new diagnostic service.
        /// </summary>
        public DiagnosticService()
        {
            // Check if a user Id is stored from when the user previously launched the app. 
            // If so, load it so we can track multiple sessions. Otherwise, assign a new user Id. 

            if (ApplicationData.Current.LocalSettings.Values.TryGetValue(UserIdKey, out object userId))
            {
                _userId = (Guid)userId;
            }
            else
            {
                _userId = Guid.NewGuid();
                ApplicationData.Current.LocalSettings.Values.Add(UserIdKey, _userId);
            }

            // Register tracking events that are triggered automatically by the system, 
            // like unhandled exceptions or app losing/gaining focus.

            App.Current.EnteredBackground += OnAppEnteredBackground;
            App.Current.UnhandledException += (s, e) => TrackError(e.Exception);
            Window.Current.Activated += (s, e) => TrackFocus(e.WindowActivationState);

            // Start all timers. 

            _appFocusTime.Start();
            _appOpenTime.Start();
            _pageOpenTime.Start();
            _pageFocusTime.Start();
        }

        private bool? _isEnabled;
        /// <summary>
        /// Gets or sets if the service sends data.
        /// </summary>
        public bool IsEnabled
        {
            get
            {
                if (_isEnabled == null)
                {
                    if (ApplicationData.Current.LocalSettings.Values.TryGetValue(
                        IsDiagnosticsEnabledKey, out object isEnabled))
                    {
                        _isEnabled = (bool)isEnabled; 
                    }
                    else
                    {
                        _isEnabled = true;
                    }
                }
                return (bool)_isEnabled; 
            }
            set
            {
                ApplicationData.Current.LocalSettings.Values[IsDiagnosticsEnabledKey] = value;
                _isEnabled = value; 
            }
        }

        /// <summary>
        /// Gets if the app should auto-prompt the user for feedback.
        /// Returns false if the user already submitted feedback; otherwise, true.
        /// </summary>
        public bool FeedbackProvided
        {
            get => ApplicationData.Current.LocalSettings.Values.ContainsKey(FeedbackProvidedKey);
        }

        /// <summary>
        /// Called when the app enters the background to track app exit diagnostics. 
        /// Note: This method does not fire if the app is launched from Visual Studio 
        /// and stopped using using the stop button (SHIFT + F5). There is no straightforward
        /// workaround for this case, so some diagnostic data for this scenario is simply lost. 
        /// </summary>
        private void OnAppEnteredBackground(object sender, EnteredBackgroundEventArgs e)
        {
            var deferral = e.GetDeferral();
            TrackExit();
            deferral.Complete();
        }

        /// <summary>
        /// Tracks an app exception.
        /// </summary>
        public void TrackError(Exception exception)
        {
            Send(new Error
            {
                Data = exception
            });
        }

        /// <summary>
        /// Tracks when the user navigates to a page.
        /// </summary>
        public void TrackNavigatingToPage(Type previousPage, Type destinationPage)
        {
            _pageOpenTime.Reset();
            _pageFocusTime.Reset();
            _pageOpenTime.Start();
            _pageFocusTime.Start();
            _currentPage = destinationPage;

            Send(new PageNavigatedTo
            {
                DestinationPageName = destinationPage?.Name,
                PreviousPageName = previousPage?.Name
            });
        }

        /// <summary>
        /// Tracks when the user navigates from a page.
        /// </summary>
        public void TrackNavigatingFromPage(Type page)
        {
            Send(new PageNavigatedFrom
            {
                Name = page?.Name,
                FocusTime = _pageFocusTime.Elapsed,
                TotalTime = _pageOpenTime.Elapsed
            });
        }

        /// <summary>
        /// Tracks an app launch.
        /// </summary>
        public void TrackLaunch(TimeSpan loadTime)
        {
            Send(new Launch
            {
                LoadTime = loadTime
            });
        }

        /// <summary>
        /// Tracks changes in app focus. Used to evaluate whether users spend 
        /// a concentrated amount of time in the app and then exit, or if they 
        /// toggle between different apps (such as Visual Studio or a browser). 
        /// </summary>
        public void TrackFocus(CoreWindowActivationState activationState)
        {
            if (activationState == CoreWindowActivationState.Deactivated)
            {
                _appFocusTime.Stop();
                _pageFocusTime.Stop();
            }
            else
            {
                _appFocusTime.Start();
                _pageFocusTime.Start();
            }
        }

        /// <summary>
        /// Tracks user feedback.
        /// </summary>
        public void TrackFeedback(Feedback feedback)
        {
            ApplicationData.Current.LocalSettings.Values[FeedbackProvidedKey] = true; 
            Send(feedback);
        }

        /// <summary>
        /// Tracks when the user exits the app. The includes the total time 
        /// the app was open and time it was in focus versus deactivated.
        /// </summary>
        public void TrackExit()
        {
            Send(new PageNavigatedFrom
            {
                Name = _currentPage.Name,
                FocusTime = _pageFocusTime.Elapsed,
                TotalTime = _pageOpenTime.Elapsed
            }, true);

            Send(new Session
            {
                FocusTime = _appFocusTime.Elapsed,
                OpenTime = _appOpenTime.Elapsed
            }, true);
        }

        /// <summary>
        /// Adds core diagnostic information (package info, timestamp, etc), then adds
        /// the diagnostic to the upload queue. 
        /// </summary>
        /// <param name="sendSynchronous">
        /// Indicates whether to queue the upload the data on a separate thread (false, default)
        /// or send synchronously on the current thread (true). Use synchronous upload when 
        /// the app is entering the background and code on other threads may not complete. 
        /// </param>
        private void Send(Diagnostic diag, bool sendSynchronous = false)
        {
            if (!IsEnabled)
            {
                return;
            }

            diag.Timestamp = DateTime.UtcNow;
            diag.UserId = _userId;
            diag.SessionId = _sessionId;

            var package = Package.Current;
            diag.PackageInstalledDate = package.InstalledDate.UtcDateTime;
            diag.IsPackageDevelopmentMode = package.IsDevelopmentMode;
            diag.PackageDisplayName = package.DisplayName;
            diag.PackageFullName = package.Id.FullName;
            diag.PackageName = package.Id.Name;

            var ver = package.Id.Version;
            diag.PackageVersion = $"{ver.Major}.{ver.Minor}.{ver.Build}.{ver.Revision}";

            if (sendSynchronous)
            {
                UploadAsync(diag).GetAwaiter().GetResult();
            }
            else
            {
                Task.Run(() => UploadAsync(diag));
            }
        }

        /// <summary>
        /// Uploads feedback to the cloud.
        /// </summary>
        private async Task UploadAsync(Diagnostic diagnostic)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    string json = JsonConvert.SerializeObject(diagnostic);
                    var response = await client.PostAsync(EndpointUrl + "api",
                        new StringContent(json, Encoding.UTF8, "application/json"));
                }
                catch (Exception)
                {
                    // Swallowing exceptions is normally bad, but in this case
                    // we don't want to surface diagnostic fails to the user. 
                }
            }
        }
    }
}