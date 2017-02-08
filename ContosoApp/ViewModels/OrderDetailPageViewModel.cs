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
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace ContosoApp.ViewModels
{
    /// <summary>
    /// Encapsulates data for the Order detail page. 
    /// </summary>
    public class OrderDetailPageViewModel : INotifyPropertyChanged
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

            _lineItems.CollectionChanged += lineItems_Changed;

            NewLineItem = new LineItemWrapper();

            if (order.Customer == null)
            {
                Task.Run(() => loadCustomer(_order.CustomerId));
            }
        }

        /// <summary>
        /// Creates a OrderDetailPageViewModel from an Order ID.
        /// </summary>
        /// <param name="orderId">The ID of the order to retrieve. </param>
        /// <returns></returns>
        public async static Task<OrderDetailPageViewModel> CreateFromGuid(Guid orderId)
        {
            var order = await getOrder(orderId);
            return new OrderDetailPageViewModel(order);
        }



        /// <summary>
        /// The EnterpriseModels.Order this object wraps. 
        /// </summary>
        private Order _order;

        /// <summary>
        /// Loads the customer with the specified ID. 
        /// </summary>
        /// <param name="customerId">The ID of the customer to load.</param>
        private async void loadCustomer(Guid customerId)
        {

            var db = new ContosoDataSource();
            var customer = await db.Customers.GetAsync(customerId);

            await Utilities.CallOnUiThreadAsync(() =>
            {
                Customer = customer;
            });
        }

        /// <summary>
        /// Returns the order with the specified ID.
        /// </summary>
        /// <param name="orderId">The ID of the order to retrieve.</param>
        /// <returns>The order, if it exists; otherwise, null. </returns>
        private static async Task<Order> getOrder(Guid orderId)
        {
            var db = new ContosoDataSource();
            var order = await db.Orders.GetAsync(orderId);
            return order;
        }

        /// <summary>
        /// Gets a value that specifies whether the user can refresh the page.
        /// </summary>
        public bool CanRefresh
        {
            get
            {
                return _order != null && !HasChanges && IsExistingOrder;
            }
        }

        /// <summary>
        /// Gets a value that specifies whether the user can revert changes. 
        /// </summary>
        public bool CanRevert
        {
            get
            {
                return _order != null && HasChanges && IsExistingOrder;
            }
        }

        /// <summary>
        /// Gets or sets the order's ID.
        /// </summary>
        public Guid Id
        {
            get
            {
                return _order.Id;
            }
            set
            {
                if (_order.Id != value)
                {
                    _order.Id = value;
                    OnPropertyChanged(nameof(Id));
                    HasChanges = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the ID of the order's customer.
        /// </summary>
        public Guid CustomerId
        {
            get
            {
                return _order.CustomerId;
            }
            set
            {
                if (_order.CustomerId != value)
                {
                    _order.CustomerId = value;
                    OnPropertyChanged(nameof(CustomerId));
                    HasChanges = true;
                }
            }
        }


        /// <summary>
        /// Gets or sets a value that indicates whether the user has changed the order. 
        /// </summary>
        bool _hasChanges = false;
        public bool HasChanges
        {
            get
            {
                return _hasChanges;
            }
            set
            {
                if (value != _hasChanges)
                {
                    // Only record changes after the order has loaded. 
                    if (IsLoaded)
                    {
                        _hasChanges = value;
                        OnPropertyChanged(nameof(HasChanges));
                    }
                }
            }
        }

        /// <summary>
        /// Gets a value that specifies whether htis is an existing order.
        /// </summary>
        public bool IsExistingOrder => !IsNewOrder;

        public bool IsLoaded => _order != null && (IsNewOrder || _order.Customer != null);

        public bool IsNotLoaded => !IsLoaded;

        public bool IsNewOrder => _order.InvoiceNumber == 0; 

        /// <summary>
        /// Gets or sets the invoice number for this order. 
        /// </summary>
        public int InvoiceNumber
        {
            get
            {
                return _order.InvoiceNumber;
            }
            set
            {
                if (_order.InvoiceNumber != value)
                {
                    _order.InvoiceNumber = value;
                    OnPropertyChanged(nameof(InvoiceNumber));
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
            get
            {
                return _order.Customer;
            }
            set
            {
                if (_order.Customer != value)
                {
                    var isLoadingOperation = _order.Customer == null &&
                        value != null && !IsNewOrder;
                    _order.Customer = value;
                    OnPropertyChanged(nameof(Customer));
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
            get
            {
                return _lineItems;
            }
            set
            {
                if (_lineItems != value)
                {
                    if (value != null)
                    {
                        value.CollectionChanged += lineItems_Changed;
                    }

                    if (_lineItems != null)
                    {
                        _lineItems.CollectionChanged -= lineItems_Changed;
                    }
                    _lineItems = value;
                    OnPropertyChanged(nameof(LineItems));
                    HasChanges = true;
                }

            }
        }

        /// <summary>
        /// Notifies anyone listening to this object that a line item changed. 
        /// </summary>
        private void lineItems_Changed(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
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
            get
            {
                return _newLineItem;
            }
            set
            {
                if (value != _newLineItem)
                {

                    if (value != null)
                    {
                        value.PropertyChanged += _newLineItem_PropertyChanged;
                    }
                    if (_newLineItem != null)
                    {
                        _newLineItem.PropertyChanged -= _newLineItem_PropertyChanged;
                    }

                    _newLineItem = value;
                    OnPropertyChanged(nameof(NewLineItem));
                }
            }

        }

        private void _newLineItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(NewLineItem));
            OnPropertyChanged(nameof(HasNewLineItem));
        }

        /// <summary>
        /// Gets or sets whether there is a new line item in progress.
        /// </summary>
        public bool HasNewLineItem
        {
            get
            {
                return NewLineItem != null && NewLineItem.Product != null;
            }
        }

        /// <summary>
        /// Gets or sets the date this order was placed. 
        /// </summary>
        public DateTime DatePlaced
        {
            get
            {
                return _order.DatePlaced;
            }
            set
            {
                if (_order.DatePlaced != value)
                {
                    _order.DatePlaced = value;
                    OnPropertyChanged(nameof(DatePlaced));
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
            get
            {
                return _order.DateFilled;

            }
            set
            {
                if (value != _order.DateFilled)
                {
                    _order.DateFilled = value;
                    OnPropertyChanged(nameof(DateFilled));
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
            get
            {
                return _order.Address;
            }
            set
            {
                if (_order.Address != value)
                {
                    _order.Address = value;
                    OnPropertyChanged(nameof(Address));
                    HasChanges = true;
                }

            }
        }

        /// <summary>
        /// Gets or sets the payment status.
        /// </summary>
        public PaymentStatus PaymentStatus
        {
            get
            {
                return _order.PaymentStatus;
            }
            set
            {
                if (_order.PaymentStatus != value)
                {
                    _order.PaymentStatus = value;
                    OnPropertyChanged(nameof(PaymentStatus));
                    HasChanges = true;
                }

            }
        }

        /// <summary>
        /// Gets or sets the payment term.
        /// </summary>
        public Term Term
        {
            get
            {
                return _order.Term;
            }
            set
            {
                if (_order.Term != value)
                {
                    _order.Term = value;
                    OnPropertyChanged(nameof(Term));
                    HasChanges = true;
                }

            }
        }

        /// <summary>
        /// Gets or sets the order status.
        /// </summary>
        public OrderStatus Status
        {
            get
            {
                return _order.Status;
            }
            set
            {
                if (_order.Status != value)
                {
                    _order.Status = value;
                    OnPropertyChanged(nameof(Status));

                    // Update the DateFilled value.
                    DateFilled = _order.Status == OrderStatus.Filled ? (Nullable<DateTime>)DateTime.Now : null;
                    HasChanges = true;

                }

            }
        }

        /// <summary>
        /// Gets or sets the name of the order's customer. 
        /// </summary>
        public string CustomerName
        {
            get
            {
                return _order.CustomerName;
            }
            set
            {
                if (_order.CustomerName != value)
                {
                    _order.CustomerName = value;
                    OnPropertyChanged("CustomerName");
                }

            }
        }

        /// <summary>
        /// Gets a string representation of the order.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"{_order.InvoiceNumber}";

        /// <summary>
        /// Fired when a property value changes. 
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Notifies listeners that a property value changed. 
        /// </summary>
        /// <param name="propertyName">The name of the property that changed. </param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        /// <summary>
        /// Converts an Order to an OrderDetailPageViewModel.
        /// </summary>
        /// <param name="order">The EnterpriseModels.Order to convert.</param>
        public static implicit operator OrderDetailPageViewModel(Order order)
        {

            return new OrderDetailPageViewModel(order);
        }


        /// <summary>
        /// Converts an OrderDetailPageViewModel to an Order.
        /// </summary>
        /// <param name="order">The OrderDetailPageViewModel to convert.</param>
        public static implicit operator Order(OrderDetailPageViewModel order)
        {
            order._order.LineItems = order.LineItems.ToList<LineItem>();
            return order._order;
        }

        /// <summary>
        /// Gets the set of order status values so we can populate the order status combo box. 
        /// </summary>
        public List<string> OrderStatusValues => Enum.GetNames(typeof(OrderStatus)).ToList();

        /// <summary>
        /// Gets the set of payment status values so we can populate the payment status combo box.
        /// </summary>
        public List<string> PaymentStatusValues => Enum.GetNames(typeof(PaymentStatus)).ToList();

        /// <summary>
        /// Gets the set of payment term values, so we can populate the term status combo box. 
        /// </summary>
        public List<string> TermValues => Enum.GetNames(typeof(Term)).ToList();


        /// <summary>
        /// Saves the current order to the database. 
        /// </summary>
        public async Task SaveOrder()
        {
            Order result = null;
            try
            {
                var db = new ContosoModels.ContosoDataSource();
                result = await db.Orders.PostAsync(_order);
            }
            catch (Exception ex)
            {
                throw new OrderSavingException("Unable to save. There might have been a problem " +
                    "connecting to the database. Please try again.", ex);
            }

            if (result != null)
            {
                await Utilities.CallOnUiThreadAsync(() => HasChanges = false);
            }
            else
            {
                await Utilities.CallOnUiThreadAsync(() => new OrderSavingException(
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
                var dataSource = new ContosoModels.ContosoDataSource();
                var suggestions = await dataSource.Products.GetAsync(queryText);

                foreach (Product p in suggestions)
                {
                    ProductSuggestions.Add(p);
                }
            }
        }

    }

    public class LineItemWrapper : INotifyPropertyChanged
    {
        LineItem _item;

        public LineItemWrapper()
        {
            _item = new LineItem();
        }

        public LineItemWrapper(LineItem item)
        {
            _item = item;
        }

        public Product Product
        {
            get
            {
                return _item.Product;
            }
            set
            {
                if (_item.Product != value)
                {
                    _item.Product = value;
                    OnPropertyChanged();
                }
            }
        }

        public Guid ProductId
        {
            get
            {
                return _item.ProductId;
            }
            set
            {
                if (_item.ProductId != value)
                {
                    _item.ProductId = value;
                    OnPropertyChanged();
                }
            }
        }

        public int Quantity
        {
            get
            {
                return _item.Quantity;
            }
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
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) => 
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        /// <summary>
        /// Converts an LineItem to an LineItemWrapper.
        /// </summary>
        /// <param name="order">The LineItem to convert.</param>
        public static implicit operator LineItemWrapper(LineItem item) =>

             new LineItemWrapper(item);


        /// <summary>
        /// Converts an LineItemWrapper to an LineItem.
        /// </summary>
        /// <param name="item">The LineItemWrapper to convert.</param>
        public static implicit operator LineItem(LineItemWrapper item) =>
            item._item;
    }

    public class OrderSavingException : Exception
    {

        public OrderSavingException() : base("Error saving an order.")
        {
        }

        public OrderSavingException(string message) : base(message)
        {
        }

        public OrderSavingException(string message,
            Exception innerException) : base(message, innerException)
        {

        }
    }
}
