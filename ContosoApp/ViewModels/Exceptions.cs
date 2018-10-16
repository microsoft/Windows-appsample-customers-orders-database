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

namespace Contoso.App.ViewModels
{
    /// <summary>
    /// Represents an exception that occurs when there's an error saving an order.
    /// </summary>
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

    /// <summary>
    /// Represents an exception that occurs when there's an error deleting an order.
    /// </summary>
    public class OrderDeletionException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the OrderDeletionException class with a default error message.
        /// </summary>
        public OrderDeletionException() : base("Error deleting an order.")
        {
        }

        /// <summary>
        /// Initializes a new instance of the OrderDeletionException class with the specified error message.
        /// </summary>
        public OrderDeletionException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the OrderDeletionException class with 
        /// the specified error message and inner exception.
        /// </summary>
        public OrderDeletionException(string message,
            Exception innerException) : base(message, innerException)
        {
        }
    }
}
