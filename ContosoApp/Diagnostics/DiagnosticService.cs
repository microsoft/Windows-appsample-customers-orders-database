using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace Contoso.App.Diagnostics
{
    public class DiagnosticService
    {
        private const string UserIdKey = "diag_user_id"; 

        private readonly List<Diagnostic> _events = new List<Diagnostic>();
        private readonly Stopwatch _appFocusTime = new Stopwatch();
        private readonly Stopwatch _appOpenTime = new Stopwatch();
        private readonly Stopwatch _pageOpenTime = new Stopwatch();
        private readonly Stopwatch _pageFocusTime = new Stopwatch(); 
        private readonly Guid _sessionId = Guid.NewGuid();
        private readonly Guid _userId;

        private Type _currentPage;

        public DiagnosticService()
        {
            if (ApplicationData.Current.LocalSettings.Values.TryGetValue(UserIdKey, out object userId))
            {
                _userId = (Guid)userId;
            }
            else
            {
                _userId = Guid.NewGuid();
                ApplicationData.Current.LocalSettings.Values.Add(UserIdKey, _userId); 
            }

            App.Current.EnteredBackground += OnAppEnteredBackground;
            Window.Current.Activated += OnWindowActivated;
        }

        private void OnWindowActivated(object sender, WindowActivatedEventArgs e)
        {
            TrackFocus(e.WindowActivationState); 
        }

        private async void OnAppEnteredBackground(object sender, EnteredBackgroundEventArgs e)
        {
            var deferral = e.GetDeferral();
            await TrackExit();
            deferral.Complete();
        }

        private async Task SendAsync()
        {
            var sending = new List<Diagnostic>(); 
            lock (_events)
            {
                sending.AddRange(_events);
                _events.Clear(); 
            }
            using (var client = new HttpClient())
            {
                string json = JsonConvert.SerializeObject(sending); 
                var response = await client.PostAsync("", new StringContent("", Encoding.UTF8, "application/json")); 
                if (!response.IsSuccessStatusCode)
                {

                }
            }
        }

        public void SendError(Exception exception)
        {

        }

        public void TrackPageView(Type type)
        {
            if (_currentPage != null)
            {
                Add(new PageView
                {
                    Name = _currentPage.Name,
                    FocusTime = _pageFocusTime.Elapsed,
                    TotalTime = _pageOpenTime.Elapsed
                }); 
            }

            _currentPage = type;
            _pageOpenTime.Reset();
            _pageOpenTime.Start(); 
        }

        public void SendEvent(object data)
        {
            Add(new Event
            {
                Data = data
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
        /// Tracks when the app launches. 
        public void TrackLaunch()
        {
            _appOpenTime.Start();
            _appFocusTime.Start(); 
        }

        public void TrackFeedback(Feedback feedback)
        {
            Add(feedback); 
        }

        /// <summary>
        /// Tracks when the user exits the app. The includes the total time 
        /// the app was open and time it was in focus versus deactivated.
        /// </summary>
        public async Task TrackExit()
        {
            Add(new PageView
            {
                Name = _currentPage.Name,
                FocusTime = _pageFocusTime.Elapsed,
                TotalTime = _pageOpenTime.Elapsed
            });

            Add(new Session
            {
                FocusTime = _appFocusTime.Elapsed,
                OpenTime = _appOpenTime.Elapsed
            });

            await SendAsync(); 
        }

        private void Add(Diagnostic diag)
        {
            diag.Timestamp = DateTime.UtcNow;
            diag.UserId = _userId;
            diag.SessionId = _sessionId;
            diag.Package = null; 

            lock (_events)
            {
                _events.Add(diag); 
            }
        }
    }

    public class Diagnostic
    {
        /// <summary>
        /// Gets or sets the date and time the feedback was submitted.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets info on the current app package, such as display name, 
        /// version, architecture, etc.
        /// </summary>
        public Package Package { get; set; }

        public Guid SessionId { get; set; }
        public Guid UserId { get; set; }
    }

    public class Event : Diagnostic
    {
        public object Data { get; set; }
    }

    public class PageView : Diagnostic
    {
        public string Name { get; set; }
        public TimeSpan TotalTime { get; set; }
        public TimeSpan FocusTime { get; set; }
    }

    public class Session : Diagnostic
    {
        public TimeSpan OpenTime { get; set; }
        public TimeSpan FocusTime { get; set; }
    }



    public class Feedback : Diagnostic
    {
        /// <summary>
        /// Gets or sets if the user voted the sample helpful or not. 
        /// Null if the user did not respond.
        /// </summary>
        public bool? IsHelpful { get; set; }

        /// <summary>
        /// Gets or sets whether the user voluntarily chose to provide 
        /// feedback or was prompted by the system. 
        /// </summary>
        public bool IsUserInitiated { get; set; }

        /// <summary>
        /// Gets or sets the user's verbatim comments.
        /// </summary>
        public string FeedbackText { get; set; }
    }
}
