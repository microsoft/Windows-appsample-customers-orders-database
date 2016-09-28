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
using System.Globalization;
using Windows.UI.Xaml.Data;


namespace ContosoApp.ValueConverters
{
    /// <summary>
    /// Converts EnterpriseModels.Term values to strings. 
    /// </summary>
    public class TermToStringConverter : IValueConverter
    {
        /// <summary>
        /// Converts a Term object to the string to display. 
        /// </summary>
        /// <param name="value">The EnterpriseModels.Term object.</param>
        /// <param name="targetType">The type to convert to. This should be the object type.</param>
        /// <param name="parameter">The format string.</param>
        /// <param name="language">Language and culture info. If this is null, we use the current culture.</param>
        /// <returns>The converted object.</returns>
        public object Convert(object value, Type targetType,
            object parameter, string language)
        {
            // Verify that the value is a Term
            var term = value as ContosoModels.Term?;
            if (targetType.Equals(typeof(System.Object)) && term != null)
            {

                // Retrieve the format string and use it to format the value.
                string formatString = parameter as string;
                if (!string.IsNullOrEmpty(formatString))
                {

                    CultureInfo culture = (!string.IsNullOrEmpty(language)) ? new CultureInfo(language) : CultureInfo.CurrentCulture;
                    return string.Format(culture, formatString, value);
                }

                // If no formatting information is provided, do a simple string conversion.
                return term.ToString();
            }
            else
            {
                throw new ArgumentException($"Unsuported type: {targetType.FullName}");
            }
        }

        /// <summary>
        /// Converts a string to a term status.
        /// </summary>
        /// <param name="value">The string.</param>
        /// <param name="targetType">The target type to convert to. This should be Term.</param>
        /// <param name="parameter">Not used.</param>
        /// <param name="language">Not used.</param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType,
                object parameter, string language)
        {
            return Enum.Parse(typeof(ContosoModels.Term), (string)value);
        }
    }


}

