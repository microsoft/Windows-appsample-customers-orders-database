using ContosoModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContosoRepository
{
    /// <summary>
    /// Contains methods for interacting with customer data.
    /// </summary>
    public class SqlCustomerRepository : ICustomerRepository
    {
        private readonly ContosoContext _db; 

        public SqlCustomerRepository(ContosoContext db)
        {
            _db = db; 
        }

        /// <summary>
        /// Gets all customers. 
        /// </summary>
        public async Task<IEnumerable<Customer>> GetCustomersAsync()
        {
            return await _db.Customers.ToListAsync();
        }

        /// <summary>
        /// Gets the customer with the given id.
        /// </summary>
        public async Task<Customer> GetCustomerAsync(Guid id)
        {
            return await _db.Customers.FirstOrDefaultAsync(x => x.Id == id);
        }

        /// <summary>
        /// Gets all customers with a data field matching the start of the given string.
        /// </summary>
        public async Task<IEnumerable<Customer>> SearchCustomersAsync(string value)
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
        public async Task<Customer> UpsertCustomerAsync(Customer customer)
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
        public async Task DeleteCustomerAsync(Guid id)
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
