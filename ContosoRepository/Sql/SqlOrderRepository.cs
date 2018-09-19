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
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Contoso.Repository.Sql
{
    /// <summary>
    /// Contains methods for interacting with the orders backend using 
    /// SQL via Entity Framework Core 2.0.
    /// </summary>
    public class SqlOrderRepository : IOrderRepository
    {
        private readonly ContosoContext _db; 

        public SqlOrderRepository(ContosoContext db) => _db = db;

        public async Task<IEnumerable<Order>> GetAsync() =>
            await _db.Orders
                .Include(order => order.LineItems)
                .ThenInclude(lineItem => lineItem.Product)
                .AsNoTracking()
                .ToListAsync();

        public async Task<Order> GetAsync(Guid id) =>
            await _db.Orders
                .Include(order => order.LineItems)
                .ThenInclude(lineItem => lineItem.Product)
                .AsNoTracking()
                .FirstOrDefaultAsync(order => order.Id == id);

        public async Task<IEnumerable<Order>> GetForCustomerAsync(Guid id) =>
            await _db.Orders
                .Where(order => order.CustomerId == id)
                .Include(order => order.LineItems)
                .ThenInclude(lineItem => lineItem.Product)
                .AsNoTracking()
                .ToListAsync();

        public async Task<IEnumerable<Order>> GetAsync(string value)
        {
            string[] parameters = value.Split(' ');
            return await _db.Orders
                .Include(order => order.Customer)
                .Include(order => order.LineItems)
                .ThenInclude(lineItem => lineItem.Product)
                .Where(order => parameters
                    .Any(parameter =>
                        order.Address.StartsWith(parameter) ||
                        order.Customer.FirstName.StartsWith(parameter) ||
                        order.Customer.LastName.StartsWith(parameter) ||
                        order.InvoiceNumber.ToString().StartsWith(parameter)))
                .OrderByDescending(order => parameters
                    .Count(parameter =>
                        order.Address.StartsWith(parameter) ||
                        order.Customer.FirstName.StartsWith(parameter) ||
                        order.Customer.LastName.StartsWith(parameter) ||
                        order.InvoiceNumber.ToString().StartsWith(parameter)))
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Order> UpsertAsync(Order order)
        {
            var existing = await _db.Orders.FirstOrDefaultAsync(_order => _order.Id == order.Id);
            if (null == existing)
            {
                order.InvoiceNumber = _db.Orders.Max(_order => _order.InvoiceNumber) + 1;
                _db.Orders.Add(order);
            }
            else
            {
                _db.Entry(existing).CurrentValues.SetValues(order);
            }
            await _db.SaveChangesAsync();
            return order;
        }

        public async Task DeleteAsync(Guid orderId)
        {
            var match = await _db.Orders.FindAsync(orderId);
            if (match != null)
            {
                _db.Orders.Remove(match);
            }
            await _db.SaveChangesAsync();
        }
    }
}
