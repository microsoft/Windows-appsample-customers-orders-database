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

using ContosoModels;
using System.Text.RegularExpressions;
using Telerik.Core;

namespace ContosoApp.ViewModels
{
    /// <summary>
    /// Wrapper for the Customer model in the master/details Customers page.
    /// </summary>
    public class CustomerViewModel : ValidateViewModelBase
    {
        /// <summary>
        /// Creates a new customer model.
        /// </summary>
        public CustomerViewModel(Customer model)
        {
            Model = model ?? new Customer();
        }

        /// <summary>
        /// The underlying customer model. Internal so it is 
        /// not visible to the RadDataGrid. 
        /// </summary>
        internal Customer Model { get; set; }

        /// <summary>
        /// Gets or sets whether the underlying model has been modified. 
        /// This is used when sync'ing with the server to reduce load
        /// and only upload the models that changed.
        /// </summary>
        internal bool IsModified { get; set; }

        /// <summary>
        /// Gets or sets whether to validate model data. 
        /// </summary>
        internal bool Validate { get; set; }

        /// <summary>
        /// Gets or sets the customer's first name.
        /// </summary>
        public string FirstName
        {
            get { return Model.FirstName; }
            set
            {
                if (value != Model.FirstName)
                {
                    Model.FirstName = value;
                    IsModified = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the customer's last name.
        /// </summary>
        public string LastName
        {
            get { return Model.LastName; }
            set
            {
                if (value != Model.LastName)
                {
                    Model.LastName = value;
                    IsModified = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the customer's address.
        /// </summary>
        public string Address
        {
            get { return Model.Address; }
            set
            {
                if (value != Model.Address)
                {
                    Model.Address = value;
                    IsModified = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the customer's company.
        /// </summary>
        public string Company
        {
            get { return Model.Company; }
            set
            {
                if (value != Model.Company)
                {
                    Model.Company = value;
                    IsModified = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the customer's phone number. If data validation is enabled, 
        /// displays an error if an invalid number is entered. 
        /// </summary>
        public string Phone
        {
            get { return Model.Phone; }
            set
            {
                if (value == Model.Phone)
                {
                    return;
                }
                if (Validate)
                {
                    var validPhoneRegex = new Regex(@"^\(?([0-9]{3})\)?[-. ]?" +
                        @"([0-9]{3})[-. ]?([0-9]{4})$");
                    if (validPhoneRegex.IsMatch(value))
                    {
                        RemoveErrors(nameof(Phone));
                    }
                    else
                    {
                        AddError(nameof(Phone), "Phone number is not valid.");
                    }
                }
                Model.Phone = value;
                IsModified = true;
            }
        }

        /// <summary>
        /// Gets or sets the customer's email. If data validation is enabled, 
        /// displays an error if an invalid email is entered. 
        /// </summary>
        public string Email
        {
            get { return Model.Email; }
            set
            {
                if (value == Model.Email)
                {
                    return;
                }
                var validEmail = new Regex(@"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" +
                    @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
                    @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$");
                if (validEmail.IsMatch(value))
                {
                    RemoveErrors(nameof(Email));
                }
                else
                {
                    AddError(nameof(Email), "Email is not valid.");
                }
                Model.Email = value;
                IsModified = true;
            }
        }
    }
}