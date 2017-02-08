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
using System.Linq;
using System.Threading.Tasks;

namespace ContosoApp.ViewModels
{
    [ImplementPropertyChanged]
    /// <summary>
    /// Encapsulates data for the CustomerListPage. The page UI
    /// binds to the properties defined here. 
    /// </summary>
    public class CustomerListPageViewModel : BindableBase
    {
        /// <summary>
        /// Creates a new CustomerListPageViewModel.
        /// </summary>
        public CustomerListPageViewModel()
        {
            Task.Run(GetCustomerListAsync);
            SyncCommand = new RelayCommand(OnSync);
        }
        
        /// <summary>
        /// The collection of customers in the list. 
        /// </summary>
        public ObservableCollection<CustomerViewModel> Customers { get; set; } = 
            new ObservableCollection<CustomerViewModel>(); 

        private CustomerViewModel _selectedCustomer;
        /// <summary>
        /// Gets or sets the selected customer, or null if no customer is selected. 
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

        private bool _isLoading = false;
        /// <summary>
        /// Gets or sets whether to show the data loading progress wheel. 
        /// </summary>
        public bool IsLoading
        {
            get { return _isLoading; }

            set
            {
                SetProperty(ref _isLoading, value);
            }
        }

        /// <summary>
        /// Gets the complete list of customers from the database.
        /// </summary>
        public async Task GetCustomerListAsync()
        {
            await Utilities.CallOnUiThreadAsync(() => IsLoading = true); 

            var db = new ContosoDataSource();
            var customers = await db.Customers.GetAsync();
            if (customers == null)
            {
                return; 
            }
            await Utilities.CallOnUiThreadAsync(() =>
            {
                Customers.Clear(); 
                foreach (var c in customers)
                {
                    Customers.Add(new CustomerViewModel(c) { Validate = true }); 
                }
                IsLoading = false;
            });
        }

        public RelayCommand SyncCommand { get; private set; }

        /// <summary>
        /// Queries the database for a current list of customers.
        /// </summary>
        private void OnSync()
        {
            Task.Run(async () =>
            {
                IsLoading = true; 
                var db = new ContosoDataSource(); 
                foreach (var modifiedCustomer in Customers
                    .Where(x => x.IsModified).Select(x => x.Model))
                {
                    await db.Customers.PostAsync(modifiedCustomer); 
                }
                await GetCustomerListAsync();
                IsLoading = false; 
            }); 
        }
    }
}
