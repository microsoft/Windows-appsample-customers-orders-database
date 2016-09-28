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
using ContosoApp.Commands;
using PropertyChanged;
using System.Collections.ObjectModel;

namespace ContosoApp.ViewModels
{
    [ImplementPropertyChanged]
    /// <summary>
    /// Encapsulates data for the CustomerListPage. The page UI
    /// binds to the properties defined here. 
    /// </summary>
    public class CustomerListPageViewModel : BindableBase
    {
        public CustomerListPageViewModel()
        {
            GetCustomerList();

            SaveCommand = new RelayCommand<CustomerViewModel>(OnSave);
            CancelEditsCommand = new RelayCommand(OnCancelEdits);
            StartEditCommand = new RelayCommand(OnStartEdit);
            RefreshCommand = new RelayCommand(OnRefresh);
        }

        /// <summary>
        /// Gets the customers to display.
        /// </summary>
        public ObservableCollection<CustomerViewModel> Customers { get; set; } = 
            new ObservableCollection<CustomerViewModel>();

        /// <summary>
        /// Backing field for the SelectedCustomer property.
        /// </summary>
        private CustomerViewModel _selectedCustomer;

        /// <summary>
        /// Gets or sets the selected customer.
        /// </summary>
        public CustomerViewModel SelectedCustomer
        {
            get { return _selectedCustomer; }
            set
            {
                SetProperty(ref _selectedCustomer, value);
            }
        }

        private string _errorText = null;

        /// <summary>
        /// Gets or sets the error text.
        /// </summary>
        public string ErrorText
        {
            get { return _errorText; }

            set
            {
                SetProperty(ref _errorText, value);
            }
        }

        /// <summary>
        /// Returns the customer that is being edited. 
        /// </summary>
        public CustomerViewModel EditDataContext
        {
            get
            {
                if (IsInEdit == true && SelectedCustomer != null)
                {
                    SelectedCustomer.Restore();
                    return SelectedCustomer;
                }
                else
                {
                    return null;
                }
            }
        }

        private bool _isInEdit = false;
        /// <summary>
        /// Gets and sets the current edit mode.
        /// </summary>
        public bool IsInEdit
        {
            get { return _isInEdit; }

            set
            {
                if (SetProperty(ref _isInEdit, value) == true)
                {
                    OnPropertyChanged(nameof(EditDataContext));
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the detials pane is open
        /// </summary>
        public bool IsPaneOpen { get; set; } = false;

        /// <summary>
        /// Gets or sets whether to show the data loading progress wheel. 
        /// </summary>
        public bool IsLoading { get; private set; } = false;

        /// <summary>
        /// Gets the complete list of customers from the database.
        /// </summary>
        private async void GetCustomerList()
        {
            IsLoading = true;

            var db = new ContosoDataSource();
            var customers = await db.Customers.GetAsync();

            await Utilities.CallOnUiThreadAsync(() =>
            {
                if (customers != null)
                {
                    ErrorText = null;
                    Customers.Clear();
                    foreach (var c in customers)
                    {
                        Customers.Add(new CustomerViewModel(c));
                    }
                }
                else
                {
                    // There was a problem retreiving customers. Let the user know.
                    ErrorText = "Couldn't retrieve customers. Check your connection and " + 
                        "click Refresh to try again.";
                }

                IsLoading = false;
            });
        }

        /// <summary>
        /// Queries the database and updates the list of customers.
        /// </summary>
        public void UpdateCustomerInList(CustomerViewModel customer)
        {
            if (customer != null)
            {
                int index = Customers.IndexOf(customer);
                Customers.RemoveAt(index);
                Customers.Insert(index, customer);
                SelectedCustomer = customer;
            }
        }

        public RelayCommand<CustomerViewModel> SaveCommand { get; private set; }

        /// <summary>
        /// Saves the specified customer data to the database.
        /// </summary>
        private void OnSave(CustomerViewModel customer)
        {
            ErrorText = customer.ErrorText;
            if (customer.CanSave == true)
            {
                customer.SaveCommand.Execute(null);
                UpdateCustomerInList(customer);
                IsInEdit = false;
            }
        }

        public RelayCommand CancelEditsCommand { get; private set; }
        /// <summary>
        /// Cancels the changes in progress.
        /// </summary>
        private void OnCancelEdits()
        {
            IsInEdit = false;
            SelectedCustomer.Restore();
        }

        public RelayCommand StartEditCommand { get; private set; }
        /// <summary>
        /// Opens the edit pane and enables edit mode.
        /// </summary>
        private void OnStartEdit()
        {
            IsInEdit = true;
            IsPaneOpen = true;
        }

        public RelayCommand RefreshCommand { get; private set; }
        /// <summary>
        /// Queries the database for a current list of customers.
        /// </summary>
        private void OnRefresh()
        {
            IsInEdit = false;
            GetCustomerList();
        }
    }
}
