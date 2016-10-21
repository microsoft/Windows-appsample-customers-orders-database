//-----------------------------------------------------------------------------------
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

using Windows.System.Profile;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

namespace ContosoApp.StateTriggers
{
    /// <summary>
    /// Custom trigger for Mobile device UI states.
    /// This trigger is active when the app runs on a mobile device and the
    /// UserInteractionMode is touch, which indicates that the app is showing
    /// on the device screen. When UserInteractionMode is mouse, the app is
    /// using Continuum to show on a larger screen.
    /// https://blogs.windows.com/buildingapps/2015/12/07/optimizing-apps-for-continuum-for-phone/#Yubo3bUdIM4H6Vle.97
    /// </summary>
    public class MobileScreenTrigger : StateTriggerBase
    {
        private UserInteractionMode _interactionMode;


        public MobileScreenTrigger()
        {
            Window.Current.SizeChanged += Window_SizeChanged;
        }

        private void Window_Activated(object sender, Windows.UI.Core.WindowActivatedEventArgs e)
        {
            UpdateTrigger();
        }

        private void Window_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            UpdateTrigger();
        }

        /// <summary>
        /// The target device family.
        /// </summary>
        public UserInteractionMode InteractionMode
        {
            get { return _interactionMode; }
            set { _interactionMode = value; UpdateTrigger(); }
        }

        private void UpdateTrigger()
        {
            // Get the current device family and interaction mode.
            var _currentDeviceFamily = AnalyticsInfo.VersionInfo.DeviceFamily;
            var _currentInteractionMode = UIViewSettings.GetForCurrentView().UserInteractionMode;

            // The trigger will be activated if the current device family is Windows.Mobile
            // and the UserInteractionMode matches the interaction mode value in XAML.
            SetActive(InteractionMode == _currentInteractionMode && _currentDeviceFamily == "Windows.Mobile");
        }
    }
}
