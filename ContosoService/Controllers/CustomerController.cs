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
    /// Contains methods for interacting with customer data.
    /// </summary>
    [Route("api/[controller]")]
    public class CustomerController : Controller
    {
        private ContosoContext _db = new ContosoContext();

        /// <summary>
        /// Gets all customers. 
        /// </summary>
        [HttpGet]
        public async Task<IEnumerable<Customer>> Get() => await _db.Customers.ToArrayAsync(); 

        /// <summary>
        /// Gets the customer with the given id.
        /// </summary>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<Customer> Get(Guid id) => await _db.Customers.FirstOrDefaultAsync(x => x.Id == id);

        /// <summary>
        /// Gets all customers with a data field matching the start of the given string.
        /// </summary>
        [HttpGet("search")]
        public async Task<IEnumerable<Customer>> Search(string value)
        {
            string[] parameters = value.Split(' ');
            return await _db.Customers
                .Where(x => 
                    parameters.Any(y =>
                        x.FirstName.StartsWith(y) || 
                        x.LastName.StartsWith(y) || 
                        x.Email.StartsWith(y) || 
                        x.Phone.StartsWith(y) || 
                        x.Address.StartsWith(y)))
                .OrderByDescending(x => 
                    parameters.Count(y =>
                        x.FirstName.StartsWith(y) || 
                        x.LastName.StartsWith(y) ||  
                        x.Email.StartsWith(y) || 
                        x.Phone.StartsWith(y) || 
                        x.Address.StartsWith(y)))
                .ToArrayAsync(); 
        }

        /// <summary>
        /// Adds a new customer or updates an existing one.
        /// </summary>
        [HttpPost]
        public async Task<Customer> Post([FromBody]Customer customer)
        {
            var current = await _db.Customers.FirstOrDefaultAsync(x => x.Id == customer.Id);
            if (null == current)
            {
                _db.Customers.Add(customer);
            }
            else
            {
                _db.Entry(current).CurrentValues.SetValues(customer);
            }
            await _db.SaveChangesAsync();
            return customer; 
        }

        /// <summary>
        /// Deletes a customer and all data associated with them.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task Delete(Guid id)
        {
            var customer = await _db.Customers.FirstOrDefaultAsync(x => x.Id == id);
            if (null != customer)
            {
                var orders = await _db.Orders.Where(x => x.CustomerId == id).ToListAsync();
                _db.Orders.RemoveRange(orders);
                _db.Customers.Remove(customer);
                await _db.SaveChangesAsync();
            }
        }
    }
}
