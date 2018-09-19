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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Contoso.Models;
using Microsoft.Toolkit.Uwp.Helpers;

namespace Contoso.App.ViewModels
{
    /// <summary>
    /// Encapsulates data for the OrderListPage. The page UI
    /// binds to the properties defined here.
    /// </summary>
    public class OrderListPageViewModel : BindableBase
    {
        private List<Order> _masterOrdersList { get; } = new List<Order>();

        public OrderListPageViewModel() => IsLoading = false;

        /// <summary>
        /// Gets the orders to display.
        /// </summary>
        public ObservableCollection<Order> Orders { get; private set; } = new ObservableCollection<Order>();

        private bool _isLoading;

        /// <summary>
        /// Gets or sets a value that specifies whether orders are being loaded.
        /// </summary>
        public bool IsLoading
        {
            get => _isLoading;
            set => Set(ref _isLoading, value); 
        }

        private Order _selectedOrder;

        /// <summary>
        /// Gets or sets the selected order.
        /// </summary>
        public Order SelectedOrder
        {
            get => _selectedOrder;
            set
            {
                if (Set(ref _selectedOrder, value))
                {
                    // Clear out the existing customer.
                    SelectedCustomer = null;
                    if (_selectedOrder != null)
                    {
                        Task.Run(() => LoadCustomer(_selectedOrder.CustomerId));
                    }
                    OnPropertyChanged(nameof(SelectedOrderGrandTotalFormatted));
                }
            }
        }

        public string SelectedOrderGrandTotalFormatted => (SelectedOrder?.GrandTotal ?? 0).ToString("c");

        private Customer _selectedCustomer;

        /// <summary>
        /// Gets or sets the selected customer.
        /// </summary>
        public Customer SelectedCustomer
        {
            get => _selectedCustomer; 
            set => Set(ref _selectedCustomer, value);
        }

        /// <summary>
        /// Stores the order suggestions.
        /// </summary>
        public ObservableCollection<Order> OrderSuggestions { get; } = new ObservableCollection<Order>();

        /// <summary>
        /// Loads the specified customer and sets the
        /// SelectedCustomer property.
        /// </summary>
        /// <param name="customerId">The customer to load.</param>
        private async void LoadCustomer(Guid customerId)
        {
            var customer = await App.Repository.Customers.GetAsync(customerId);
            await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
            {
                SelectedCustomer = customer;
            });
        }

        /// <summary>
        /// Retrieves orders from the data source.
        /// </summary>
        public async void LoadOrders()
        {
            await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
            {
                IsLoading = true;
                Orders.Clear();
                _masterOrdersList.Clear();
            });

            var orders = await Task.Run(App.Repository.Orders.GetAsync);
            await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
            {
                if (orders != null)
                {
                    foreach (Order o in orders)
                    {
                        Orders.Add(o);
                        _masterOrdersList.Add(o);
                    }
                }
                IsLoading = false;
            });
        }

        /// <summary>
        /// Submits a query to the data source.
        /// </summary>
        /// <param name="query"></param>
        public async void QueryOrders(string query)
        {
            IsLoading = true;
            Orders.Clear();
            if (!string.IsNullOrEmpty(query))
            {
                var results = await App.Repository.Orders.GetAsync(query);
                await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
                {
                    foreach (Order o in results)
                    {
                        Orders.Add(o);
                    }
                    IsLoading = false;
                });
            }
        }

        /// <summary>
        /// Queries the database and updates the list of new order suggestions.
        /// </summary>
        /// <param name="queryText">The query to submit.</param>
        public void UpdateOrderSuggestions(string queryText)
        {
            OrderSuggestions.Clear();
            if (!string.IsNullOrEmpty(queryText))
            {
                string[] parameters = queryText.Split(new char[] { ' ' },
                    StringSplitOptions.RemoveEmptyEntries);

                var resultList = _masterOrdersList
                    .Where(order => parameters
                        .Any(parameter =>
                            order.Address.StartsWith(parameter) ||
                            order.CustomerName.Contains(parameter) ||
                            order.InvoiceNumber.ToString().StartsWith(parameter)))
                    .OrderByDescending(order => parameters
                        .Count(parameter =>
                            order.Address.StartsWith(parameter) ||
                            order.CustomerName.Contains(parameter) ||
                            order.InvoiceNumber.ToString().StartsWith(parameter)));

                foreach (Order order in resultList)
                {
                    OrderSuggestions.Add(order);
                }
            }
        }

        /// <summary>
        /// Deletes the specified order from the database.
        /// </summary>
        /// <param name="orderToDelete">The order to delete.</param>
        public async Task DeleteOrder(Order orderToDelete) =>
            await App.Repository.Orders.DeleteAsync(orderToDelete.Id);
    }

    /// <summary>
    /// Occurs when there's an error deleting an order.
    /// </summary>
    public class OrderDeletionException : Exception
    {
        public OrderDeletionException() : base("Error deleting an order.")
        { }

        public OrderDeletionException(string message) : base(message)
        { }

        public OrderDeletionException(string message,
            Exception innerException) : base(message, innerException)
        { }
    }
}