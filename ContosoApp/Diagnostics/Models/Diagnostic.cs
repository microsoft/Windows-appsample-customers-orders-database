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

namespace Contoso.App.Diagnostics.Models
{
    /// <summary>
    /// Contains diagnostic data about app usage.
    /// </summary>
    public abstract class Diagnostic
    {
        /// <summary>
        /// The type of diagnostic. 
        /// </summary>
        public string Type => GetType().Name;

        /// <summary>
        /// The UTC timestamp the diagnostic was generated at.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets the session Id. A session is a use of an app 
        /// from launch to suspend/exit. 
        /// </summary>
        public Guid SessionId { get; set; }

        /// <summary>
        /// Gets the user Id. The Id persists between sessions, 
        /// so long as the user does not uninstall the app.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Indicates if the package is in development mode (e.g., launched 
        /// in Visual Studio or from the Store). 
        /// </summary>
        public bool IsPackageDevelopmentMode { get; set; }

        /// <summary>
        /// The package version.
        /// </summary>
        public string PackageVersion { get; set; }

        /// <summary>
        /// The date the package was installed.
        /// </summary>
        public DateTime PackageInstalledDate { get; set; }

        /// <summary>
        /// The name of the package.
        /// </summary>
        public string PackageName { get; set; }

        /// <summary>
        /// The full name of the package.
        /// </summary>
        public string PackageFullName { get; set; }

        /// <summary>
        /// The friendly display name of the package.
        /// </summary>
        public string PackageDisplayName { get; set; }
    }
}
