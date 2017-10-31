using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;

namespace Contoso.App.Diagnostics
{
    public class Diagnostic
    {

    }

    public class DiagnosticService
    {
        private Stopwatch _focusTime = new Stopwatch();
        private Stopwatch _openTime = new Stopwatch();
        private Stopwatch _pageTime = new Stopwatch(); 
        private Concurr

        public void SendError()
        {

        }

        public void TrackPageView(string name)
        {
            // When we track a page view
                    
        }

        public void SendEvent(object data)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="activationState"></param>
        public void TrackFocus(CoreWindowActivationState activationState)
        {
            if (activationState == CoreWindowActivationState.Deactivated)
            {
                _focusTime.Stop(); 
            }
            else
            {
                _focusTime.Start(); 
            }
        }

        public void TrackLaunch()
        {
            _openTime.Start(); 
        }

        public void TrackFeedback()
        {

        }

        public void TrackExit()
        {
            // Record the total amount of time the user spent with the app only,
            // plus the amount of time it was in focus. 
        }

        public void TrackFocusTime()
        {

        }

        public void SendData()
        {

        }
    }
}
