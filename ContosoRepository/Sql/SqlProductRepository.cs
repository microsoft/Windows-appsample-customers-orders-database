using ContosoModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContosoRepository
{
    /// <summary>
    /// Contains methods for interacting with product data.
    /// </summary>
    public class SqlProductRepository : IProductRepository
    {
        private readonly ContosoContext _db;

        public SqlProductRepository(ContosoContext db)
        {
            _db = db; 
        }

        /// <summary>
        /// Gets all products in the database.
        /// </summary>
        public async Task<IEnumerable<Product>> Get()
        {
            return await _db.Products.ToListAsync();
        }

        /// <summary>
        /// Gets the product with the given id.
        /// </summary>
        public async Task<Product> Get(Guid id)
        {
            return await _db.Products.FirstOrDefaultAsync(x => x.Id == id);
        }

        /// <summary>
        /// Gets all products with a data field matching the start of the given string.
        /// </summary>
        public async Task<IEnumerable<Product>> Search(string value)
        {
            return await _db.Products.Where(x =>
                x.Name.StartsWith(value) ||
                x.Color.StartsWith(value) ||
                x.DaysToManufacture.ToString().StartsWith(value) ||
                x.StandardCost.ToString().StartsWith(value) ||
                x.ListPrice.ToString().StartsWith(value) ||
                x.Weight.ToString().StartsWith(value) ||
                x.Description.StartsWith(value))
            .ToListAsync();
        }
    }
}
