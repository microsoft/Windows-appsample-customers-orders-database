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
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Contoso.Models;
using Microsoft.Toolkit.Uwp.Helpers;

namespace Contoso.App.ViewModels
{
    /// <summary>
    /// Encapsulates data for the Customer detail page. 
    /// </summary>
    public class CustomerDetailPageViewModel : BindableBase
    {
        /// <summary>
        /// Creates a CustomerDetailPageViewModel that wraps the specified customer.
        /// </summary>
        public CustomerDetailPageViewModel() => Task.Run(LoadCustomerOrdersAsync);

        private ObservableCollection<Order> _orders = new ObservableCollection<Order>(); 

        /// <summary>
        /// The collection of the customer's orders.
        /// </summary>
        public ObservableCollection<Order> Orders
        {
            get => _orders;
            set => Set(ref _orders, value);
        }

        private bool _isLoading; 

        /// <summary>
        /// Indicates whether to show the loading icon. 
        /// </summary>
        public bool IsLoading
        {
            get => _isLoading;
            set => Set(ref _isLoading, value); 
        }

        private bool _isNewCustomer; 

        /// <summary>
        /// Indicates whether this is a new customer.
        /// </summary>
        public bool IsNewCustomer
        {
            get => _isNewCustomer;
            set => Set(ref _isNewCustomer, value);
        }

        private CustomerViewModel _customer;

        /// <summary>
        /// Gets and sets the current customer values.
        /// </summary>
        public CustomerViewModel Customer
        {
            get => _customer;
            set
            {
                if (Set(ref _customer, value))
                {
                    if (string.IsNullOrWhiteSpace(Customer.FirstName))
                    {
                        IsInEdit = true;
                    }
                }
            }
        }

        private bool _isInEdit = false;

        /// <summary>
        /// Gets and sets the current edit mode.
        /// </summary>
        public bool IsInEdit
        {
            get => _isInEdit;
            set => Set(ref _isInEdit, value);                           
        }

        public bool IsNotInEdit => !IsInEdit;

        /// <summary>
        /// Saves customer data that has been edited.
        /// </summary>
        public async Task Save()
        {
            IsInEdit = false;
            await App.Repository.Customers.UpsertAsync(_customer.Model);
        }

        public event EventHandler EditsCanceled;

        /// <summary>
        /// Cancels any in progress edits.
        /// </summary>
        public async Task CancelEditsAsync()
        {
            await RefreshCustomerAsync();
            IsInEdit = false;
            Customer.IsModified = false;
            EditsCanceled?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Enables edit mode.
        /// </summary>
        public void StartEdit() => IsInEdit = true; 

        public async Task RefreshCustomerAsync()
        {
            RefreshOrders();
            Customer cust = await App.Repository.Customers.GetAsync(Customer.Model.Id);
            Customer.Model = cust;
        }

        /// <summary>
        /// Resets the customer detail fields to the current values.
        /// </summary>
        public void RefreshOrders() => Task.Run(LoadCustomerOrdersAsync);

        public async Task LoadCustomerOrdersAsync()
        {
            await DispatcherHelper.ExecuteOnUIThreadAsync(() => IsLoading = true);
            var orders = await App.Repository.Orders.GetForCustomerAsync(_customer.Model.Id); 
            await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
            {
                Orders.Clear();
                foreach (var o in orders)
                {
                    Orders.Add(o);
                }
                IsLoading = false;
            });
        }
    }
}
