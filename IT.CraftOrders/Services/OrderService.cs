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
    public class OrderService
    {
        private readonly CraftFactoryDbContext _db;
        public OrderService(CraftFactoryDbContext db)
        {
            _db = db;
        }

        public Task<List<Order>> GetLatestAsync(int take = 10)
        {
            return _db.Orders.Include(o => o.Customer)
                      .Include(o => o.OrderLines).ThenInclude(ol => ol.Product)
                      .OrderByDescending(o => o.CreatedUtc)
                      .Take(take)
                      .ToListAsync();
        }
        public Task<List<Order>> GetAllOrdersAsync()
        {
            return _db.Orders.OrderBy(o => o.CreatedUtc).ToListAsync();
        }
    }
}
