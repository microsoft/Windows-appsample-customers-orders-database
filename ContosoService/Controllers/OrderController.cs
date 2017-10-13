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
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Contoso.Service.Controllers
{
    /// <summary>
    /// Contains methods for interacting with order data.
    /// </summary>
    [Route("api/[controller]")]
    public class OrderController : Controller
    {
        private readonly IOrderRepository _repository; 

        public OrderController(IOrderRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Gets all orders.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok(await _repository.GetAsync()); 
        }

        /// <summary>
        /// Gets the with the given id.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest(); 
            }
            var order = await _repository.GetAsync(id); 
            if (order == null)
            {
                return NotFound(); 
            }
            return Ok(order); 
        }

        /// <summary>
        /// Gets all the orders for a given customer. 
        /// </summary>
        [HttpGet("customer/{id}")]
        public async Task<IActionResult> GetCustomerOrders(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest(); 
            }
            var orders = await _repository.GetForCustomerAsync(id);
            if (orders == null)
            {
                return NotFound(); 
            }
            return Ok(orders); 
        }

        /// <summary>
        /// Gets all orders with a data field matching the start of the given string.
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> Search(string value)
        {
            if (String.IsNullOrWhiteSpace(value))
            {
                return BadRequest(); 
            }
            var orders = await _repository.GetAsync(value); 
            if (orders == null)
            {
                return NotFound(); 
            }
            return Ok(orders); 
        }


        /// <summary>
        /// Creates a new order or updates an existing one.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]Order order)
        {
            return Ok(await _repository.UpsertAsync(order)); 
        }

        /// <summary>
        /// Deletes an order.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Order order)
        {
            await _repository.DeleteAsync(order.Id);
            return Ok(); 
        }
    }
}
