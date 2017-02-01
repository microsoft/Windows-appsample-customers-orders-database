////  ---------------------------------------------------------------------------------
////  Copyright (c) Microsoft Corporation.  All rights reserved.
//// 
////  The MIT License (MIT)
//// 
////  Permission is hereby granted, free of charge, to any person obtaining a copy
////  of this software and associated documentation files (the "Software"), to deal
////  in the Software without restriction, including without limitation the rights
////  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
////  copies of the Software, and to permit persons to whom the Software is
////  furnished to do so, subject to the following conditions:
//// 
////  The above copyright notice and this permission notice shall be included in
////  all copies or substantial portions of the Software.
//// 
////  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
////  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
////  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
////  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
////  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
////  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
////  THE SOFTWARE.
////  ---------------------------------------------------------------------------------

//using ContosoModels;
//using ContosoApp.Commands;
//using PropertyChanged;
//using System;
//using System.Collections.ObjectModel;
//using System.Text.RegularExpressions;

//namespace ContosoApp.ViewModels
//{
//    [ImplementPropertyChanged]
//    public class CustomerViewModel : BindableBase
//    {
//        public CustomerViewModel() : this(new Customer())
//        { }

//        public CustomerViewModel(Customer model)
//        {
//            SaveCommand = new RelayCommand(OnSave);

//            Model = model;

//            FirstName = _firstNameOriginal = model.FirstName;
//            LastName = _lastNameOriginal = model.LastName;
//            Company = _companyOriginal = model.Company;
//            Email = _emailOriginal = model.Email;
//            Phone = _phoneOriginal = model.Phone;
//            Address = _addressOriginal = model.Address;
//            Id = model.Id;

//            if (string.IsNullOrWhiteSpace(model.ToString()))
//            {
//                IsNewCustomer = true;
//            }
//            else
//            {
//                GetOrdersList();
//            }
//        }

//        private string _errorText = null;
//        public string ErrorText
//        {
//            get { return _errorText; }

//            set
//            {
//                SetProperty(ref _errorText, value);
//                CanSave = string.IsNullOrWhiteSpace(value);
//            }
//        }

//        public bool HasChanges { get; set;} = false;

//        public bool IsNewCustomer { get; private set; } = false;

//        public bool CanSave { get; private set; } = true;

//        public bool IsLoading { get; private set; } = false;

//        public ObservableCollection<Order> Orders { get; private set; } = 
//            new ObservableCollection<Order>();

//        public Customer Model { get; private set; }

//        private string _firstNameOriginal;
//        private string _firstName;
//        public string FirstName
//        {
//            get { return _firstName; }

//            set
//            {
//                // Don't check on initial load.
//                if (FirstName == null)
//                {
//                    SetProperty(ref _firstName, value);
//                }
//                // Make sure text is entered for name.
//                else if (string.IsNullOrWhiteSpace(value))
//                {
//                    ErrorText = "First name is required. ";
//                }
//                else if (SetProperty(ref _firstName, value) == true)
//                {
//                    OnPropertyChanged(nameof(Name));
//                    ErrorText = null;
//                }
//            }
//        }

//        private string _lastNameOriginal;
//        private string _lastName;
//        public string LastName
//        {
//            get { return _lastName; }

//            set
//            {
//                // Don't check on initial load.
//                if (LastName == null)
//                {
//                    SetProperty(ref _lastName, value);
//                }
//                // Make sure text is entered for name.
//                else if (string.IsNullOrWhiteSpace(value))
//                {
//                    ErrorText = "Last name is required. ";
//                }
//                else if (SetProperty(ref _lastName, value) == true)
//                {
//                    OnPropertyChanged(nameof(Name));
//                    ErrorText = null;
//                }
//            }
//        }

//        public string Name
//        {
//            get
//            {
//                return $"{FirstName} {LastName}";
//            }
//        }

//        private string _companyOriginal;
//        private string _company;
//        public string Company
//        {
//            get { return _company; }

//            set
//            {
//                SetProperty(ref _company, value);
//            }
//        }

//        private string _emailOriginal;
//        private string _email;
//        public string Email
//        {
//            get { return _email; }

//            set
//            {
//                // Don't check on initial load.
//                if (Email == null)
//                {
//                    SetProperty(ref _email, value);
//                }
//                // Check if email address string is valid, convert to lowercase 
//                // for easier duplicate checking.
//                else if (IsValidEmail(value))
//                {
//                    value = value.ToLower();
//                    if (SetProperty(ref _email, value) == true)
//                    {
//                        ErrorText = null;
//                    }
//                }
//                else
//                {
//                    ErrorText = ("Email format is invalid. ");
//                }
//            }
//        }

//        private string _phoneOriginal;
//        private string _phone;
//        public string Phone
//        {
//            get { return _phone; }

//            set
//            {
//                // Don't check on initial load.
//                if (Phone == null)
//                {
//                    SetProperty(ref _phone, value);
//                }
//                else if (IsValidPhoneNumber(value))
//                {
//                    if (SetProperty(ref _phone, value) == true)
//                    {
//                        ErrorText = null;
//                    }
//                }
//                else
//                {
//                    ErrorText = ("Phone number format is invalid. ");
//                }
//            }
//        }

//        private string _addressOriginal;
//        private string _address;
//        public string Address
//        {
//            get { return _address; }

//            set
//            {
//                SetProperty(ref _address, value);
//            }
//        }

//        private Guid _id;
//        public Guid Id
//        {
//            get { return _id; }

//            set
//            {
//                SetProperty(ref _id, value);
//            }
//        }
//        public int OrderCount
//        {
//            get
//            {
//                if (Orders != null)
//                {
//                    return Orders.Count;
//                }
//                else
//                {
//                    return 0;
//                }
//            }
//        }

//        public override string ToString() => $"{Name} ({Email})";

//        private async void GetOrdersList()
//        {
//            IsLoading = true;

//            var db = new ContosoDataSource();
//            var orders = await db.Orders.GetAsync(Model);

//            await Utilities.CallOnUiThreadAsync(() =>
//            {
//                if (orders != null)
//                {
//                    foreach(Order o in orders)
//                    {
//                        Orders.Add(o);
//                    }
//                }
//                else
//                {
//                    // There was a problem retreiving customers. Let the user know.
//                    ErrorText = "Couldn't retrieve orders.";
//                }

//                IsLoading = false;
//            });
//        }

//        public RelayCommand SaveCommand { get; private set; }
//        private async void OnSave()
//        {
//            if (CanSave == true)
//            {
//                Model.FirstName = _firstNameOriginal = FirstName;
//                Model.LastName = _lastNameOriginal = LastName;
//                Model.Company = _companyOriginal = Company;
//                Model.Email = _emailOriginal = Email;
//                Model.Phone = _phoneOriginal = Phone;
//                Model.Address = _addressOriginal = Address;

//                var db = new ContosoDataSource();
//                await db.Customers.PostAsync(Model);

//                HasChanges = true;
//                if (IsNewCustomer == true)
//                {
//                    IsNewCustomer = false;
//                }
//            }
//        }

//        public void Restore()
//        {
//            FirstName = _firstNameOriginal;
//            LastName = _lastNameOriginal;
//            Company = _companyOriginal;
//            Email = _emailOriginal;
//            Phone = _phoneOriginal;
//            Address = _addressOriginal;
//            ErrorText = null;
//        }


//        public bool IsValidEmail(string email) =>
//            new Regex(@"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" +
//                @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
//                @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$").IsMatch(email);

//        public bool IsValidPhoneNumber(string phone) =>
//            new Regex(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$").IsMatch(phone);
//    }
//}