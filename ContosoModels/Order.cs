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
using System.Linq;

namespace ContosoModels
{
    /// <summary>
    /// Represents a customer order.
    /// </summary>
    public class Order : DbObject
    {
        /// <summary>
        /// Creates a new order.
        /// </summary>
        public Order()
        { }

        /// <summary>
        /// Creates a new order for the given customer.
        /// </summary>
        public Order(Customer customer) : this()
        {
            Customer = customer;
            CustomerName = $"{customer.FirstName} {customer.LastName}";
            Address = customer.Address;
        }

        /// <summary>
        /// Gets or sets the id of the customer placing the order.
        /// </summary>
        public Guid CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the customer placing the order.
        /// </summary>
        public Customer Customer { get; set; }

        /// <summary>
        /// Gets or sets the name of the customer placing the order.
        /// </summary>
        public string CustomerName { get; set; }

        /// <summary>
        /// Gets or sets the invoice number.
        /// </summary>
        public int InvoiceNumber { get; set; } = 0;

        /// <summary>
        /// Gets or sets the order shipping address.
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// Gets or sets the items in the order.
        /// </summary>
        public virtual List<LineItem> LineItems { get; set; } = new List<LineItem>();

        /// <summary>
        /// Gets or sets when the order was placed.
        /// </summary>
        public DateTime DatePlaced { get; set; } = DateTime.Now;

        /// <summary>
        /// Gets or sets when the order was filled.
        /// </summary>
        public DateTime? DateFilled { get; set; } = null;

        /// <summary>
        /// Gets or sets the order's status.
        /// </summary>
        public OrderStatus Status { get; set; } = OrderStatus.Open;

        /// <summary>
        /// Gets or sets the order's payment status.
        /// </summary>
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;

        /// <summary>
        /// Gets or sets the order's term.
        /// </summary>
        public Term Term { get; set; }

        /// <summary>
        /// Gets the order's subtotal.
        /// </summary>
        public decimal Subtotal => LineItems.Sum(x => x.Product.ListPrice * x.Quantity);

        /// <summary>
        /// Gets the order's tax.
        /// </summary>
        public decimal Tax => Subtotal * .095m;

        /// <summary>
        /// Gets the order's grand total.
        /// </summary>
        public decimal GrandTotal => Subtotal + Tax; 

        /// <summary>
        /// Returns the invoice number.
        /// </summary>
        public override string ToString() => $"{InvoiceNumber}";
    }

    /// <summary>
    /// Represents the term for an order.
    /// </summary>
    public enum Term
    {
        Net1, 
        Net5,
        Net15, 
        Net30
    }

    /// <summary>
    /// Represents the payment status for an order.
    /// </summary>
    public enum PaymentStatus
    {
        Unpaid,
        Paid 
    }

    /// <summary>
    /// Represents the status of an order.
    /// </summary>
    public enum OrderStatus
    {
        Open,
        Filled, 
        Cancelled
    }
}
