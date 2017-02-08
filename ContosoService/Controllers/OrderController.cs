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
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ContosoModels;
using ContosoDatabase;

namespace ContosoService.Controllers
{    
    /// <summary>
    /// Contains methods for interacting with order data.
    /// </summary>
    [Route("api/[controller]")]
    public class OrderController : Controller
    {
        private ContosoContext _db = new ContosoContext();

        /// <summary>
        /// Gets all orders.
        /// </summary>
        [HttpGet]
        public async Task<IEnumerable<Order>> Get() => await _db.Orders.Include(x => 
            x.LineItems.Select(y => y.Product)).ToArrayAsync(); 

        /// <summary>
        /// Gets the with the given id.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<Order> Get(Guid id) => await _db.Orders.Include(x => x.LineItems.Select(y => 
            y.Product)).FirstOrDefaultAsync(x => x.Id == id); 

        /// <summary>
        /// Gets all the orders for a given customer. 
        /// </summary>
        [HttpGet("customer/{id}")]
        public async Task<IEnumerable<Order>> GetCustomerOrders(Guid id) => await _db.Orders.Where(x => 
            x.CustomerId == id).Include(x => x.LineItems.Select(y => y.Product)).ToArrayAsync();

        /// <summary>
        /// Gets all orders with a data field matching the start of the given string.
        /// </summary>
        [HttpGet("search")]
        public async Task<IEnumerable<Order>> Search(string value)
        {
            string[] parameters = value.Split(' ');
            return await _db.Orders
                .Include(x => x.Customer)
                .Include(x => x.LineItems
                    .Select(y => y.Product))
                .Where(x => parameters
                    .Any(y =>
                        x.Address.StartsWith(y) ||
                        x.Customer.FirstName.StartsWith(y) ||
                        x.Customer.LastName.StartsWith(y) || 
                        x.InvoiceNumber.ToString().StartsWith(y)))
                .OrderByDescending(x => parameters
                    .Count(y =>
                        x.Address.StartsWith(y) ||
                        x.Customer.FirstName.StartsWith(y) ||
                        x.Customer.LastName.StartsWith(y) || 
                        x.InvoiceNumber.ToString().StartsWith(y)))
                .ToArrayAsync(); 
        }


        /// <summary>
        /// Creates a new order or updates an existing one.
        /// </summary>
        [HttpPost]
        public async Task<Order> Post([FromBody]Order order)
        {
            var existing = await _db.Orders.FirstOrDefaultAsync(x => x.Id == order.Id); 
            // Create new order
            if (null == existing)
            {
                order.InvoiceNumber = _db.Orders.Max(x => x.InvoiceNumber) + 1; 
                _db.Orders.Add(order); 
            }
            // Update existing order
            else
            {
                _db.Entry(existing).CurrentValues.SetValues(order);
            }
            await _db.SaveChangesAsync();
            return order; 
        }


        /// <summary>
        /// Deletes an order.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task Delete(Order order)
        {
            var match = await _db.Orders.FirstOrDefaultAsync(x => x.Id == order.Id); 
            if (match != null)
            {
                _db.Orders.Remove(match); 
            }
            await _db.SaveChangesAsync(); 
        }
    }
}
