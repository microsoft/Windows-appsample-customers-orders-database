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
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Contoso.App.UserControls
{
    /// <summary>
    /// Flyout dialog for collecting user feedback and satisfaction with the app.
    /// </summary>
    public sealed partial class FeedbackFlyout : Flyout
    {
        private Feedback _currentFeedback = new Feedback();

        public FeedbackFlyout()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets or sets whether the app prompted the user for feedback 
        /// or if they decided to provide it on their own.
        /// </summary>
        public bool IsUserInitiated { get; set; }

        /// <summary>
        /// Called when the flyout opens. Resets the current feedback. 
        /// </summary>
        private void Flyout_Opened(object sender, object e)
        {
            _currentFeedback = new Feedback(); 
        }

        /// <summary>
        /// Called when the flyout is closed. Sends any feedback that was collected.
        /// </summary>
        private void Flyout_Closed(object sender, object e)
        {
            App.Diagnostics.TrackFeedback(_currentFeedback);

            // Reset the form and cache a flag so the flyout doesn't appear again 
            // unless the user explicitly asks.

            FeedbackTextBox.Text = "";
            _currentFeedback = new Feedback();

            IsHelpfulPhase.Visibility = Visibility.Visible;
            VerbatimPhase.Visibility = Visibility.Collapsed;
            FinishedPhase.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Sets current feedback as "Helpful," then transitions to the verbatim form.
        /// </summary>
        private void IsHelpfulButton_Click(object sender, RoutedEventArgs e)
        {
            _currentFeedback.IsHelpful = true;
            IsHelpfulPhase.Visibility = Visibility.Collapsed;
            VerbatimPhase.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Sets current feedback as IsHelpful = then transitions to the verbatim form.
        /// </summary>
        private void NotHelpfulButton_Click(object sender, RoutedEventArgs e)
        {
            _currentFeedback.IsHelpful = false;
            IsHelpfulPhase.Visibility = Visibility.Collapsed;
            VerbatimPhase.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Sets the current feedback verbatim text, then transitions to the finished text.
        /// </summary>
        private void SubmitVerbatimButton_Click(object sender, RoutedEventArgs e)
        {
            _currentFeedback.FeedbackText = FeedbackTextBox.Text;
            VerbatimPhase.Visibility = Visibility.Collapsed;
            FinishedPhase.Visibility = Visibility.Visible;
        }
    }
}
