using ContosoModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContosoService.Controllers
{
    /// <summary>
    /// Contains methods for interacting with order data.
    /// </summary>
    [Route("api/[controller]")]
    public class OrderController : Controller
    {
        private readonly ContosoContext _db; 

        public OrderController(ContosoContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Gets all orders.
        /// </summary>
        [HttpGet]
        public async Task<IEnumerable<Order>> Get() => await _db.Orders.Include(x =>
            x.LineItems).ThenInclude(x => x.Product).ToArrayAsync();

        /// <summary>
        /// Gets the with the given id.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<Order> Get(Guid id) => await _db.Orders.Include(x => x.LineItems)
            .ThenInclude(x => x.Product).FirstOrDefaultAsync(x => x.Id == id);

        /// <summary>
        /// Gets all the orders for a given customer. 
        /// </summary>
        [HttpGet("customer/{id}")]
        public async Task<IEnumerable<Order>> GetCustomerOrders(Guid id) => await _db.Orders.Where(x =>
            x.CustomerId == id).Include(x => x.LineItems).ThenInclude(x => x.Product).ToArrayAsync();

        /// <summary>
        /// Gets all orders with a data field matching the start of the given string.
        /// </summary>
        [HttpGet("search")]
        public async Task<IEnumerable<Order>> Search(string value)
        {
            string[] parameters = value.Split(' ');
            return await _db.Orders
                .Include(x => x.Customer)
                .Include(x => x.LineItems)
                .ThenInclude(x => x.Product)
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
