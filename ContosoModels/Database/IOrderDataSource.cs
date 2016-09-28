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
using System.Net.Http;
using System.Threading.Tasks;

namespace ContosoModels
{
    public interface IOrderDataSource
    {
        /// <summary>
        /// Gets all orders. 
        /// </summary>
        Task<IEnumerable<Order>> GetAsync();

        /// <summary>
        /// Gets all the given customer's orders. 
        /// </summary>
        Task<IEnumerable<Order>> GetAsync(Customer customer);

        /// <summary>
        /// Gets the order with the given id.
        /// </summary>
        Task<Order> GetAsync(Guid orderId);

        /// <summary>
        /// Gets all order with a data field matching the start of the given string. 
        /// </summary>
        Task<IEnumerable<Order>> GetAsync(string search);

        /// <summary>
        /// Adds a new order if the order does not exist, updates the 
        /// existing order otherwise.
        /// </summary>
        Task<Order> PostAsync(Order order);

        /// <summary>
        /// Deletes an order.
        /// </summary>
        Task<HttpResponseMessage> DeleteAsync(Guid orderId);

    }
}
