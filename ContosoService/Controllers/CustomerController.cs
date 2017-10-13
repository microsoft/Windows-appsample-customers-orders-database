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
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Contoso.Models;

namespace Contoso.Service.Controllers
{
    namespace Contoso.Service.Controllers
    {
        /// <summary>
        /// Contains methods for interacting with customer data.
        /// </summary>
        [Route("api/[controller]")]
        public class CustomerController : Controller
        {
            private ICustomerRepository _repository; 

            public CustomerController(ICustomerRepository repository)
            {
                _repository = repository; 
            }

            /// <summary>
            /// Gets all customers. 
            /// </summary>
            [HttpGet]
            public async Task<IActionResult> Get()
            {
                return Ok(await _repository.GetAsync()); 
            }

            /// <summary>
            /// Gets the customer with the given id.
            /// </summary>
            [HttpGet("{id}")]
            public async Task<IActionResult> Get(Guid id)
            {
                if (id == Guid.Empty)
                {
                    return BadRequest(); 
                }
                var customer = await _repository.GetAsync(id);
                if (customer == null)
                {
                    return NotFound();
                }
                else
                {
                    return Ok(customer); 
                }
            }

            /// <summary>
            /// Gets all customers with a data field matching the start of the given string.
            /// </summary>
            [HttpGet("search")]
            public async Task<IActionResult> Search(string value)
            {
                return Ok(await _repository.GetAsync(value)); 
            }

            /// <summary>
            /// Adds a new customer or updates an existing one.
            /// </summary>
            [HttpPost]
            public async Task<IActionResult> Post([FromBody]Customer customer)
            {
                return Ok(await _repository.UpsertAsync(customer)); 
            }

            /// <summary>
            /// Deletes a customer and all data associated with them.
            /// </summary>
            [HttpDelete("{id}")]
            public async Task<IActionResult> Delete(Guid id)
            {
                await _repository.DeleteAsync(id);
                return Ok(); 
            }
        }
    }
}
