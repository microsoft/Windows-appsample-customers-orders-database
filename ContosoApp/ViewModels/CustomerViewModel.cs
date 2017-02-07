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
    public class CustomerViewModel : ValidateViewModelBase
    {
        public CustomerViewModel(Customer model)
        {
            Model = model ?? new Customer();
        }

        internal Customer Model { get; set; }

        internal bool IsModified { get; set; }

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

        public string Phone
        {
            get { return Model.Phone; }
            set
            {
                if (value != Model.Phone)
                {
                    var validPhone = new Regex(@"^\(?([0-9]{3})\)?[-. ]?" +
                        @"([0-9]{3})[-. ]?([0-9]{4})$");
                    if (validPhone.IsMatch(value))
                    {
                        RemoveErrors(nameof(Phone));
                        Model.Phone = value;
                    }
                    else
                    {
                        AddError(nameof(Phone), "Phone number is not valid.");
                    }
                    IsModified = true; 
                }
            }
        }

        public string Email
        {
            get { return Model.Email; }
            set
            {
                if (value != Model.Email)
                {
                    Model.Email = value;
                    var validEmail = new Regex(@"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" +
                        @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
                        @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$");

                    if (validEmail.IsMatch(value))
                    {
                        RemoveErrors(nameof(Email));
                        Model.Email = value;
                    }
                    else
                    {
                        AddError(nameof(Email), "Email is not valid.");
                    }
                    IsModified = true; 
                }
            }
        }
    }
}