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
        private readonly IOrderRepository _repository; 

        public OrderController(IOrderRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Gets all orders.
        /// </summary>
        [HttpGet]
        public async Task<IEnumerable<Order>> Get()
        {
            return await _repository.GetOrdersAsync(); 
        }

        /// <summary>
        /// Gets the with the given id.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<Order> Get(Guid id)
        {
            return await _repository.GetOrderAsync(id); 
        }

        /// <summary>
        /// Gets all the orders for a given customer. 
        /// </summary>
        [HttpGet("customer/{id}")]
        public async Task<IEnumerable<Order>> GetCustomerOrders(Guid id)
        {
            return await _repository.GetCustomerOrdersAsync(id); 
        }

        /// <summary>
        /// Gets all orders with a data field matching the start of the given string.
        /// </summary>
        [HttpGet("search")]
        public async Task<IEnumerable<Order>> Search(string value)
        {
            return await _repository.SearchOrdersAsync(value); 
        }


        /// <summary>
        /// Creates a new order or updates an existing one.
        /// </summary>
        [HttpPost]
        public async Task<Order> Post([FromBody]Order order)
        {
            return await _repository.UpsertOrderAsync(order); 
        }

        /// <summary>
        /// Deletes an order.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task Delete(Order order)
        {
            await _repository.DeleteOrderAsync(order.Id); 
        }
    }
}
