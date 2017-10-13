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

using Contoso.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Contoso.Repository.Rest
{
    /// <summary>
    /// Contains methods for interacting with the orders backend using REST. 
    /// </summary>
    public class RestOrderRepository : IOrderRepository
    {
        private readonly HttpHelper _http;

        public RestOrderRepository(string baseUrl)
        {
            _http = new HttpHelper(baseUrl);
        }

        public async Task<IEnumerable<Order>> GetAsync() =>
            await _http.GetAsync<IEnumerable<Order>>("order");

        public async Task<Order> GetAsync(Guid id) =>
            await _http.GetAsync<Order>($"order/{id}");

        public async Task<IEnumerable<Order>> GetForCustomerAsync(Guid customerId) =>
            await _http.GetAsync<IEnumerable<Order>>($"order/customer/{customerId}");

        public async Task<IEnumerable<Order>> GetAsync(string search) =>
            await _http.GetAsync<IEnumerable<Order>>($"order/search?value={search}");

        public async Task<Order> UpsertAsync(Order order) =>
            await _http.PostAsync<Order, Order>("order", order);

        public async Task DeleteAsync(Guid orderId) =>
            await _http.DeleteAsync("order", orderId);
    }
}
