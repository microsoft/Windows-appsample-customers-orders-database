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
using Microsoft.Toolkit.Uwp;
using Windows.System;

namespace Contoso.App.ViewModels
{
    /// <summary>
    /// Encapsulates data for the OrderListPage. The page UI
    /// binds to the properties defined here.
    /// </summary>
    public class OrderListPageViewModel : BindableBase
    {
        private DispatcherQueue dispatcherQueue = DispatcherQueue.GetForCurrentThread();

        /// <summary>
        /// Initializes a new instance of the OrderListPageViewModel class.
        /// </summary>
        public OrderListPageViewModel() => IsLoading = false;

        /// <summary>
        /// Gets the unfiltered collection of all orders. 
        /// </summary>
        private List<Order> MasterOrdersList { get; } = new List<Order>();

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

        /// <summary>
        /// Gets a formatted version of the selected order's grand total value.
        /// </summary>
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
        /// Loads the specified customer and sets the
        /// SelectedCustomer property.
        /// </summary>
        /// <param name="customerId">The customer to load.</param>
        private async void LoadCustomer(Guid customerId)
        {
            var customer = await App.Repository.Customers.GetAsync(customerId);
            await dispatcherQueue.EnqueueAsync(() =>
            {
                SelectedCustomer = customer;
            });
        }

        /// <summary>
        /// Retrieves orders from the data source.
        /// </summary>
        public async void LoadOrders()
        {
            await dispatcherQueue.EnqueueAsync(() =>
            {
                IsLoading = true;
                Orders.Clear();
                MasterOrdersList.Clear();
            });

            var orders = await Task.Run(App.Repository.Orders.GetAsync);

            await dispatcherQueue.EnqueueAsync(() =>
            {
                foreach (var order in orders)
                {
                    Orders.Add(order);
                    MasterOrdersList.Add(order);
                }

                IsLoading = false;
            });
        }

        /// <summary>
        /// Submits a query to the data source.
        /// </summary>
        public async void QueryOrders(string query)
        {
            if (!string.IsNullOrWhiteSpace(query))
            {
                IsLoading = true;
                Orders.Clear();
                var results = await App.Repository.Orders.GetAsync(query);
                await dispatcherQueue.EnqueueAsync(() =>
                {
                    foreach (Order o in results)
                    {
                        Orders.Add(o);
                    }
                });
                IsLoading = false;
            }
        }

        /// <summary>
        /// Deletes the specified order from the database.
        /// </summary>
        public async Task DeleteOrder(Order orderToDelete) =>
            await App.Repository.Orders.DeleteAsync(orderToDelete.Id);

        /// <summary>
        /// Stores the order suggestions.
        /// </summary>
        public ObservableCollection<Order> OrderSuggestions { get; } = new ObservableCollection<Order>();

        /// <summary>
        /// Queries the database and updates the list of new order suggestions.
        /// </summary>
        public void UpdateOrderSuggestions(string queryText)
        {
            OrderSuggestions.Clear();
            if (!string.IsNullOrEmpty(queryText))
            {
                string[] parameters = queryText.Split(new char[] { ' ' },
                    StringSplitOptions.RemoveEmptyEntries);

                var resultList = MasterOrdersList
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
    }
}