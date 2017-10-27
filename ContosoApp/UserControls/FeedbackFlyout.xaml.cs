using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Contoso.App.UserControls
{
    public sealed partial class FeedbackFlyout : Flyout
    {
        public FeedbackFlyout()
        {
            InitializeComponent();
            SetTimerAsync();
        }

        private const string FeedbackRequestedKey = "feedback_requested";

        private Feedback _currentFeedback = new Feedback();

        private int _delay;
        public int Delay
        {
            get => _delay;
            set => SetProperty(ref _delay, value);
        }

        private string _url;
        public string Url
        {
            get => _url;
            set => SetProperty(ref _url, value);
        }

        private void Flyout_Opened(object sender, object e)
        {
            //var item = (sender as Flyout);
            //var itemDataContext = item.DataContext;

            //FrameworkElement parent = (item.FindName("MenuFlyoutContainer") as MenuFlyout);

            //var fx = VisualTreeHelper.GetParent(sender as DependencyObject); 

            _currentFeedback = new Feedback
            {
                Timestamp = DateTime.UtcNow,
                Package = Package.Current
            };
        }

        private void Flyout_Closed(object sender, object e)
        {
            // Send any data we have collected on a separate thread.

            Task.Run(() => SendAsync(new Feedback
            {
                FeedbackText = _currentFeedback.FeedbackText,
                IsHelpful = _currentFeedback.IsHelpful,
                IsUserInitiated = _currentFeedback.IsUserInitiated,
                Timestamp = DateTime.UtcNow,
            }));

            // Reset the form and cache a flag so the flyout doesn't appear again 
            // unless the user explicitly asks.

            FeedbackTextBox.Text = "";
            _currentFeedback = new Feedback();
            ApplicationData.Current.RoamingSettings.Values[FeedbackRequestedKey] = true;

            IsHelpfulPhase.Visibility = Visibility.Visible;
            VerbatimPhase.Visibility = Visibility.Collapsed;
            FinishedPhase.Visibility = Visibility.Collapsed;
        }

        private void IsHelpfulButton_Click(object sender, RoutedEventArgs e)
        {
            _currentFeedback.IsHelpful = true;
            IsHelpfulPhase.Visibility = Visibility.Collapsed;
            VerbatimPhase.Visibility = Visibility.Visible;
        }

        private void NotHelpfulButton_Click(object sender, RoutedEventArgs e)
        {
            _currentFeedback.IsHelpful = false;
            IsHelpfulPhase.Visibility = Visibility.Collapsed;
            VerbatimPhase.Visibility = Visibility.Visible;
        }

        private void FeedbackButton_Click(object sender, RoutedEventArgs e)
        {
            _currentFeedback.IsUserInitiated = true;
        }

        private void SubmitVerbatimButton_Click(object sender, RoutedEventArgs e)
        {
            _currentFeedback.FeedbackText = FeedbackTextBox.Text;
            VerbatimPhase.Visibility = Visibility.Collapsed;
            FinishedPhase.Visibility = Visibility.Visible;
        }

        private void SetTimerAsync()
        {
            // If we didn't already ask for feedback, set a timer to pop the dialog 
            // after the user has spent some time using the app.

            if (!ApplicationData.Current.RoamingSettings.Values.ContainsKey(FeedbackRequestedKey))
            {
                var timer = ThreadPoolTimer.CreateTimer(async (x) =>
                {
                    // When the task fires, double-check to see if the user already gave feedback 
                    // voluntarily so we don't annoy them to submit twice.

                    if (!ApplicationData.Current.RoamingSettings.Values.ContainsKey(FeedbackRequestedKey))
                    {
                        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            
                        });
                    }
                }, TimeSpan.FromMilliseconds(Delay));
            }
        }

        private async Task SendAsync(Feedback data)
        {
            using (var client = new HttpClient())
            {
                string json = JsonConvert.SerializeObject(data, new JsonSerializerSettings
                {
                    Error = (s, e) => e.ErrorContext.Handled = true
                });
                await client.PostAsync(Url, new StringContent(json, Encoding.UTF8, "application/json"));
            }
        }

        private bool SetProperty<T>(ref T field, T value, [CallerMemberName]string propertyName = null)
        {
            if (Equals(field, value))
            {
                return false;
            }
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private class Feedback
        {
            /// <summary>
            /// Gets or sets the date and time the feedback was submitted.
            /// </summary>
            public DateTime Timestamp { get; set; }

            /// <summary>
            /// Gets or sets if the user voted the sample helpful or not. 
            /// Null if the user dismissed the flyout and did not respond.
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

            /// <summary>
            /// Gets or sets info on the current app package, such as display name, 
            /// version, architecture, etc.
            /// </summary>
            public Package Package { get; set; }
        }
    }
}
