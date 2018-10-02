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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Contoso.Models;
using Microsoft.Toolkit.Uwp.Helpers;

namespace Contoso.App.ViewModels
{
    /// <summary>
    /// Encapsulates data for the Order detail page. 
    /// </summary>
    public class OrderDetailPageViewModel : BindableBase
    {
        /// <summary>
        /// Creates an OrderDetailPageViewModel that wraps the specified EnterpriseModels.Order.
        /// </summary>
        /// <param name="order">The order to wrap.</param>
        public OrderDetailPageViewModel(Order order)
        {
            _order = order;

            // Create an ObservableCollection to wrap Order.LineItems so we can track
            // product additions and deletions.
            _lineItems = order != null && order.LineItems != null ?
                new ObservableCollection<LineItem>(_order.LineItems) :
                new ObservableCollection<LineItem>();

            _lineItems.CollectionChanged += LineItems_Changed;

            NewLineItem = new LineItemWrapper();

            if (order.Customer == null)
            {
                Task.Run(() => LoadCustomer(_order.CustomerId));
            }
        }

        /// <summary>
        /// Creates a OrderDetailPageViewModel from an Order ID.
        /// </summary>
        /// <param name="orderId">The ID of the order to retrieve. </param>
        public async static Task<OrderDetailPageViewModel> CreateFromGuid(Guid orderId) =>
            new OrderDetailPageViewModel(await GetOrder(orderId));

        /// <summary>
        /// The EnterpriseModels.Order this object wraps. 
        /// </summary>
        private Order _order;

        /// <summary>
        /// Loads the customer with the specified ID. 
        /// </summary>
        /// <param name="customerId">The ID of the customer to load.</param>
        private async void LoadCustomer(Guid customerId)
        {
            var customer = await App.Repository.Customers.GetAsync(customerId);
            await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
            {
                Customer = customer;
            });
        }

        /// <summary>
        /// Returns the order with the specified ID.
        /// </summary>
        /// <param name="orderId">The ID of the order to retrieve.</param>
        /// <returns>The order, if it exists; otherwise, null. </returns>
        private static async Task<Order> GetOrder(Guid orderId) =>
            await App.Repository.Orders.GetAsync(orderId); 

        /// <summary>
        /// Gets a value that specifies whether the user can refresh the page.
        /// </summary>
        public bool CanRefresh => _order != null && !HasChanges && IsExistingOrder;

        /// <summary>
        /// Gets a value that specifies whether the user can revert changes. 
        /// </summary>
        public bool CanRevert => _order != null && HasChanges && IsExistingOrder;

        /// <summary>
        /// Gets or sets the order's ID.
        /// </summary>
        public Guid Id
        {
            get => _order.Id; 
            set
            {
                if (_order.Id != value)
                {
                    _order.Id = value;
                    OnPropertyChanged();
                    HasChanges = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the ID of the order's customer.
        /// </summary>
        public Guid CustomerId
        {
            get => _order.CustomerId;
            set
            {
                if (_order.CustomerId != value)
                {
                    _order.CustomerId = value;
                    OnPropertyChanged();
                    HasChanges = true;
                }
            }
        }

        bool _hasChanges = false;

        /// <summary>
        /// Gets or sets a value that indicates whether the user has changed the order. 
        /// </summary>
        public bool HasChanges
        {
            get => _hasChanges; 
            set
            {
                if (value != _hasChanges)
                {
                    // Only record changes after the order has loaded. 
                    if (IsLoaded)
                    {
                        _hasChanges = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Gets a value that indicates whether this is an existing order.
        /// </summary>
        public bool IsExistingOrder => !IsNewOrder;

        /// <summary>
        /// Gets a value that indicates whether there is a backing order.
        /// </summary>
        public bool IsLoaded => _order != null && (IsNewOrder || _order.Customer != null);

        /// <summary>
        /// Gets a value that indicates whether there is not a backing order.
        /// </summary>
        public bool IsNotLoaded => !IsLoaded;

        /// <summary>
        /// Gets a value that indicates whether this is a newly-created order.
        /// </summary>
        public bool IsNewOrder => _order.InvoiceNumber == 0; 

        /// <summary>
        /// Gets or sets the invoice number for this order. 
        /// </summary>
        public int InvoiceNumber
        {
            get => _order.InvoiceNumber;
            set
            {
                if (_order.InvoiceNumber != value)
                {
                    _order.InvoiceNumber = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsNewOrder));
                    OnPropertyChanged(nameof(IsLoaded));
                    OnPropertyChanged(nameof(IsNotLoaded));
                    OnPropertyChanged(nameof(IsNewOrder));
                    OnPropertyChanged(nameof(IsExistingOrder));
                    HasChanges = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the customer for this order. This value is null
        /// unless you manually retrieve the customer (using CustomerId) and 
        /// set it. 
        /// </summary>
        public Customer Customer
        {
            get => _order.Customer;
            set
            {
                if (_order.Customer != value)
                {
                    var isLoadingOperation = _order.Customer == null &&
                        value != null && !IsNewOrder;
                    _order.Customer = value;
                    OnPropertyChanged();
                    if (isLoadingOperation)
                    {
                        OnPropertyChanged(nameof(IsLoaded));
                        OnPropertyChanged(nameof(IsNotLoaded));
                    }
                    else
                    {
                        HasChanges = true;
                    }
                }
            }
        }

        private ObservableCollection<LineItem> _lineItems = new ObservableCollection<LineItem>();
        
        /// <summary>
        /// Gets the line items in this invoice. 
        /// </summary>
        public virtual ObservableCollection<LineItem> LineItems
        {
            get => _lineItems; 
            set
            {
                if (_lineItems != value)
                {
                    if (value != null)
                    {
                        value.CollectionChanged += LineItems_Changed;
                    }

                    if (_lineItems != null)
                    {
                        _lineItems.CollectionChanged -= LineItems_Changed;
                    }
                    _lineItems = value;
                    OnPropertyChanged();
                    HasChanges = true;
                }
            }
        }

        /// <summary>
        /// Notifies anyone listening to this object that a line item changed. 
        /// </summary>
        private void LineItems_Changed(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (LineItems != null)
            {
                _order.LineItems = LineItems.ToList<LineItem>();
            }

            OnPropertyChanged(nameof(LineItems));
            OnPropertyChanged(nameof(Subtotal));
            OnPropertyChanged(nameof(Tax));
            OnPropertyChanged(nameof(GrandTotal));
            HasChanges = true;
        }

        private LineItemWrapper _newLineItem;

        /// <summary>
        /// Gets or sets the line item that the user is currently working on.
        /// </summary>
        public LineItemWrapper NewLineItem
        {
            get => _newLineItem; 
            set
            {
                if (value != _newLineItem)
                {
                    if (value != null)
                    {
                        value.PropertyChanged += NewLineItem_PropertyChanged;
                    }

                    if (_newLineItem != null)
                    {
                        _newLineItem.PropertyChanged -= NewLineItem_PropertyChanged;
                    }

                    _newLineItem = value;
                    UpdateNewLineItemBindings();
                }
            }
        }

        private void NewLineItem_PropertyChanged(object sender, PropertyChangedEventArgs e) => UpdateNewLineItemBindings();

        private void UpdateNewLineItemBindings()
        {
            OnPropertyChanged(nameof(NewLineItem));
            OnPropertyChanged(nameof(HasNewLineItem));
            OnPropertyChanged(nameof(NewLineItemProductListPriceFormatted));
        }

        /// <summary>
        /// Gets or sets whether there is a new line item in progress.
        /// </summary>
        public bool HasNewLineItem => NewLineItem != null && NewLineItem.Product != null;

        /// <summary>
        /// Gets the product list price of the new line item, formatted for display.
        /// </summary>
        public string NewLineItemProductListPriceFormatted => (NewLineItem?.Product?.ListPrice ?? 0).ToString("c");

        /// <summary>
        /// Gets or sets the date this order was placed. 
        /// </summary>
        public DateTime DatePlaced
        {
            get => _order.DatePlaced;
            set
            {
                if (_order.DatePlaced != value)
                {
                    _order.DatePlaced = value;
                    OnPropertyChanged();
                    HasChanges = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the date this order was filled. 
        /// This value is automatically updated when the 
        /// OrderStatus changes. 
        /// </summary>
        public DateTime? DateFilled
        {
            get => _order.DateFilled;
            set
            {
                if (value != _order.DateFilled)
                {
                    _order.DateFilled = value;
                    OnPropertyChanged();
                    HasChanges = true;
                }
            }
        }

        /// <summary>
        /// Gets the subtotal. This value is calculated automatically. 
        /// </summary>
        public decimal Subtotal => _order.Subtotal;

        /// <summary>
        /// Gets the tax. This value is calculated automatically. 
        /// </summary>
        public decimal Tax => _order.Tax;

        /// <summary>
        /// Gets the total. This value is calculated automatically. 
        /// </summary>
        public decimal GrandTotal => _order.GrandTotal;

        /// <summary>
        /// Gets or sets the shipping address, which may be different
        /// from the customer's primary address. 
        /// </summary>
        public string Address
        {
            get => _order.Address; 
            set
            {
                if (_order.Address != value)
                {
                    _order.Address = value;
                    OnPropertyChanged();
                    HasChanges = true;
                }
            }
        }

        /// <summary>
        /// Gets the set of payment status values so we can populate the payment status combo box.
        /// </summary>
        public List<string> PaymentStatusValues => Enum.GetNames(typeof(PaymentStatus)).ToList();

        /// <summary>
        /// Sets the PaymentStatus property by parsing a string representation of the enum value.
        /// </summary>
        public void SetPaymentStatus(object value) =>
            PaymentStatus = (PaymentStatus)Enum.Parse(typeof(PaymentStatus), value as string);

        /// <summary>
        /// Gets or sets the payment status.
        /// </summary>
        public PaymentStatus PaymentStatus
        {
            get => _order.PaymentStatus;
            set
            {
                if (_order.PaymentStatus != value)
                {
                    _order.PaymentStatus = value;
                    OnPropertyChanged();
                    HasChanges = true;
                }
            }
        }

        /// <summary>
        /// Gets the set of payment term values, so we can populate the term combo box. 
        /// </summary>
        public List<string> TermValues => Enum.GetNames(typeof(Term)).ToList();

        /// <summary>
        /// Sets the Term property by parsing a string representation of the enum value.
        /// </summary>
        public Term SetTerm(object value) => Term = (Term)Enum.Parse(typeof(Term), value as string);

        /// <summary>
        /// Gets or sets the payment term.
        /// </summary>
        public Term Term
        {
            get => _order.Term;
            set
            {
                if (_order.Term != value)
                {
                    _order.Term = value;
                    OnPropertyChanged();
                    HasChanges = true;
                }
            }
        }

        /// <summary>
        /// Gets the set of order status values so we can populate the order status combo box. 
        /// </summary>
        public List<string> OrderStatusValues => Enum.GetNames(typeof(OrderStatus)).ToList();

        /// <summary>
        /// Sets the Status property by parsing a string representation of the enum value.
        /// </summary>
        public OrderStatus SetStatus(object value) =>
            Status = (OrderStatus)Enum.Parse(typeof(OrderStatus), value as string);

        /// <summary>
        /// Gets or sets the order status.
        /// </summary>
        public OrderStatus Status
        {
            get => _order.Status;
            set
            {
                if (_order.Status != value)
                {
                    _order.Status = value;
                    OnPropertyChanged();

                    // Update the DateFilled value.
                    DateFilled = _order.Status == OrderStatus.Filled ? (DateTime?)DateTime.Now : null;
                    HasChanges = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the name of the order's customer. 
        /// </summary>
        public string CustomerName
        {
            get => _order.CustomerName;
            set
            {
                if (_order.CustomerName != value)
                {
                    _order.CustomerName = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets a string representation of the order.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"{_order.InvoiceNumber}";

        /// <summary>
        /// Converts an Order to an OrderDetailPageViewModel.
        /// </summary>
        /// <param name="order">The EnterpriseModels.Order to convert.</param>
        public static implicit operator OrderDetailPageViewModel(Order order) => new OrderDetailPageViewModel(order);

        /// <summary>
        /// Converts an OrderDetailPageViewModel to an Order.
        /// </summary>
        /// <param name="order">The OrderDetailPageViewModel to convert.</param>
        public static implicit operator Order(OrderDetailPageViewModel order)
        {
            order._order.LineItems = order.LineItems.ToList();
            return order._order;
        }

        /// <summary>
        /// Saves the current order to the database. 
        /// </summary>
        public async Task SaveOrderAsync()
        {
            Order result = null;
            try
            {
                result = await App.Repository.Orders.UpsertAsync(_order);
            }
            catch (Exception ex)
            {
                throw new OrderSavingException("Unable to save. There might have been a problem " +
                    "connecting to the database. Please try again.", ex);
            }

            if (result != null)
            {
                await DispatcherHelper.ExecuteOnUIThreadAsync(() => HasChanges = false);
            }
            else
            {
                await DispatcherHelper.ExecuteOnUIThreadAsync(() => new OrderSavingException(
                    "Unable to save. There might have been a problem " +
                    "connecting to the database. Please try again."));
            }
        }

        /// <summary>
        /// Stores the product suggestions. 
        /// </summary>
        public ObservableCollection<Product> ProductSuggestions { get; } =
            new ObservableCollection<Product>();

        /// <summary>
        /// Queries the database and updates the list of new product suggestions. 
        /// </summary>
        /// <param name="queryText">The query to submit.</param>
        public async void UpdateProductSuggestions(string queryText)
        {
            ProductSuggestions.Clear();

            if (!string.IsNullOrEmpty(queryText))
            {
                var suggestions = await App.Repository.Products.GetAsync(queryText);

                foreach (Product p in suggestions)
                {
                    ProductSuggestions.Add(p);
                }
            }
        }
    }

    /// <summary>
    /// Wraps LineItem objects in order to provide change notification for data binding purposes.
    /// </summary>
    public class LineItemWrapper : INotifyPropertyChanged
    {
        LineItem _item;

        /// <summary>
        /// Initializes a new instance of the LineItemWrapper class using a new line item.
        /// </summary>
        public LineItemWrapper() => _item = new LineItem();

        /// <summary>
        /// Initializes a new instance of the LineItemWrapper class using the specified line item.
        /// </summary>
        public LineItemWrapper(LineItem item) => _item = item;

        /// <summary>
        /// Gets or sets the product for the line item.
        /// </summary>
        public Product Product
        {
            get => _item.Product;
            set
            {
                if (_item.Product != value)
                {
                    _item.Product = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the product ID for the line item.
        /// </summary>
        public Guid ProductId
        {
            get => _item.ProductId;
            set
            {
                if (_item.ProductId != value)
                {
                    _item.ProductId = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the product quantity for the line item.
        /// </summary>
        public int Quantity
        {
            get => _item.Quantity;
            set
            {
                if (_item.Quantity != value)
                {
                    _item.Quantity = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) => 
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        /// <summary>
        /// Converts an LineItem to an LineItemWrapper.
        /// </summary>
        /// <param name="order">The LineItem to convert.</param>
        public static implicit operator LineItemWrapper(LineItem item) => new LineItemWrapper(item);

        /// <summary>
        /// Converts an LineItemWrapper to an LineItem.
        /// </summary>
        /// <param name="item">The LineItemWrapper to convert.</param>
        public static implicit operator LineItem(LineItemWrapper item) => item._item;
    }

    public class OrderSavingException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the OrderSavingException class with a default error message.
        /// </summary>
        public OrderSavingException() : base("Error saving an order.")
        {
        }

        /// <summary>
        /// Initializes a new instance of the OrderSavingException class with the specified error message.
        /// </summary>
        public OrderSavingException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the OrderSavingException class with 
        /// the specified error message and inner exception.
        /// </summary>
        public OrderSavingException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
