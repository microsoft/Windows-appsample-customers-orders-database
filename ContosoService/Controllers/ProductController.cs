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

using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ContosoModels;
using System.Data.Entity;

namespace ContosoService.Controllers
{
    /// <summary>
    /// Contains methods for interacting with product data.
    /// </summary>
    [Route("api/[controller]")]
    public class ProductController : Controller
    {
        private ContosoContext _db = new ContosoContext();

        /// <summary>
        /// Gets all products in the database.
        /// </summary>
        [HttpGet]
        public async Task<IEnumerable<Product>> Get() => await _db.Products.ToArrayAsync();

        /// <summary>
        /// Gets the product with the given id.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<Product> Get(Guid id) => await _db.Products.FirstOrDefaultAsync(x => x.Id == id);


        /// <summary>
        /// Gets all products with a data field matching the start of the given string.
        /// </summary>
        [HttpGet("search")]
        public async Task<IEnumerable<Product>> Search(string value) =>
            await _db.Products.Where(x =>
                x.Name.StartsWith(value) ||
                x.Color.StartsWith(value) ||
                x.DaysToManufacture.ToString().StartsWith(value) ||
                x.StandardCost.ToString().StartsWith(value) ||
                x.ListPrice.ToString().StartsWith(value) ||
                x.Weight.ToString().StartsWith(value) ||
                x.Description.StartsWith(value))
            .ToArrayAsync();
    }
}
