using IT.CraftOrders.Data;
using IT.CraftOrders.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IT.CraftOrders.Services
{
    public class ProductService
    {
        private readonly CraftFactoryDbContext _db;
        public ProductService(CraftFactoryDbContext db)
        {
            _db = db;
        }

        public Task<List<Product>> GetAllProductsAsync()
        {
            return _db.Products.OrderBy(p => p.Name).ToListAsync();
        }
    }
}
