using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ContosoModels;
using Microsoft.EntityFrameworkCore;

namespace ContosoService.Controllers
{
    namespace ContosoService.Controllers
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
            public async Task<IEnumerable<Customer>> Get()
            {
                return await _repository.GetCustomersAsync(); 
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
                var result = _repository.GetCustomerAsync(id);
                if (result == null)
                {
                    return NotFound();
                }
                else
                {
                    return Ok(result); 
                }
            }

            /// <summary>
            /// Gets all customers with a data field matching the start of the given string.
            /// </summary>
            [HttpGet("search")]
            public async Task<IActionResult> Search(string value)
            {
                return Ok(await _repository.SearchCustomersAsync(value)); 
            }

            /// <summary>
            /// Adds a new customer or updates an existing one.
            /// </summary>
            [HttpPost]
            public async Task<IActionResult> Post([FromBody]Customer customer)
            {
                return Ok(await _repository.UpsertCustomerAsync(customer)); 
            }

            /// <summary>
            /// Deletes a customer and all data associated with them.
            /// </summary>
            [HttpDelete("{id}")]
            public async Task Delete(Guid id)
            {
                await _repository.DeleteCustomerAsync(id); 
            }
        }
    }
}
