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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace ContosoApp.ViewModels
{
    /// <summary>
    /// Encapsulates data for the OrderListPage. The page UI
    /// binds to the properties defined here.
    /// </summary>
    public class OrderListPageViewModel : BindableBase
    {
        public OrderListPageViewModel()
        {
            IsLoading = false;
        }

        /// <summary>
        /// Gets the orders to display.
        /// </summary>
        public ObservableCollection<Order> Orders { get; private set; } = new ObservableCollection<Order>();

        /// <summary>
        /// Keeps an unfiltered view of the orders list.
        /// </summary>
        private List<Order> masterOrdersList { get; } = new List<Order>();

        /// <summary>
        /// Indicates whether orders are being loaded.
        /// </summary>
        private bool _isLoading;

        /// <summary>
        /// Gets or sets a value that specifies whether orders are being loaded.
        /// </summary>
        public bool IsLoading
        {
            get
            {
                return _isLoading;
            }
            set
            {
                SetProperty(ref _isLoading, value);
            }
        }

        /// <summary>
        /// Backing field for the SelectedOrder property.
        /// </summary>
        private Order _selectedOrder;

        /// <summary>
        /// Gets or sets the selected order.
        /// </summary>
        public Order SelectedOrder
        {
            get
            {
                return _selectedOrder;
            }
            set
            {
                if (SetProperty(ref _selectedOrder, value))
                {
                    // Clear out the existing customer.
                    SelectedCustomer = null;
                    if (_selectedOrder != null)
                    {
                        Task.Run(() => loadCustomer(_selectedOrder.CustomerId));
                    }
                }
            }
        }

        /// <summary>
        /// Gets the SelectedOrder value as type Object, which is required for using the
        /// strongly-typed x:Bind with ListView.SelectedItem, which is of type Object.
        /// </summary>
        public object SelectedOrderAsObject
        {
            get { return SelectedOrder; }
            set { SelectedOrder = value as Order; }
        }


        /// <summary>
        /// Loads the specified customer and sets the
        /// SelectedCustomer property.
        /// </summary>
        /// <param name="customerId">The customer to load.</param>
        private async void loadCustomer(Guid customerId)
        {
            var db = new ContosoModels.ContosoDataSource();
            var customer = await db.Customers.GetAsync(customerId);

            await Utilities.CallOnUiThreadAsync(() =>
            {
                SelectedCustomer = customer;
            });
        }

        /// <summary>
        /// The selected customer.
        /// </summary>
        private Customer _selectedCustomer;

        /// <summary>
        /// Gets or sets the selected customer.
        /// </summary>
        public Customer SelectedCustomer
        {
            get
            {
                return _selectedCustomer;
            }
            set
            {
                SetProperty(ref _selectedCustomer, value);
            }
        }

        /// <summary>
        /// Retrieves orders from the data source.
        /// </summary>
        public async void LoadOrders()
        {
            await Utilities.CallOnUiThreadAsync(() =>
            {
                IsLoading = true;
                Orders.Clear();
                masterOrdersList.Clear();
            });

            var db = new ContosoModels.ContosoDataSource();
            var orders = await db.Orders.GetAsync();

            await Utilities.CallOnUiThreadAsync(() =>
            {
                if (orders != null)
                {
                    foreach (Order o in orders)
                    {
                        Orders.Add(o);
                        masterOrdersList.Add(o);
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
                var dataSource = new ContosoModels.ContosoDataSource();
                var results = await dataSource.Orders.GetAsync(query);

                await Utilities.CallOnUiThreadAsync(() =>
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
        /// Stores the order suggestions.
        /// </summary>
        public ObservableCollection<Order> OrderSuggestions { get; } = new ObservableCollection<Order>();

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

                var resultList = masterOrdersList
                    .Where(x => parameters
                        .Any(y =>
                            x.Address.StartsWith(y) ||
                            x.CustomerName.Contains(y) ||
                            x.InvoiceNumber.ToString().StartsWith(y)))
                    .OrderByDescending(x => parameters
                        .Count(y =>
                            x.Address.StartsWith(y) ||
                            x.CustomerName.Contains(y) ||
                            x.InvoiceNumber.ToString().StartsWith(y)));

                foreach (Order o in resultList)
                {
                    OrderSuggestions.Add(o);
                }
            }
        }

        /// <summary>
        /// Deletes the specified order from the database.
        /// </summary>
        /// <param name="orderToDelete">The order to delete.</param>
        public async Task DeleteOrder(Order orderToDelete)
        {
            var db = new ContosoModels.ContosoDataSource();
            var response = await db.Orders.DeleteAsync(orderToDelete.Id);

            if (!response.IsSuccessStatusCode)
            {
                Orders.Remove(orderToDelete);
                SelectedOrder = null;
            }
            else
            {
                throw new OrderDeletionException(response.ReasonPhrase);
            }
        }

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