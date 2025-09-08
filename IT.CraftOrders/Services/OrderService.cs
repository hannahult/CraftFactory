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

        public async Task<Order> CreateAsync(int customerId, IEnumerable<(int productId, int qty)> lines)
        {
            var products = await _db.Products
                .Where(p => lines.Select(l => l.productId).Contains(p.ProductId))
                .ToListAsync();

            var order = new Order
            {
                CustomerId = customerId,
                Status = "New",
                CreatedUtc = DateTime.UtcNow,
                UpdatedUtc = DateTime.UtcNow
            };

            foreach (var (productId, qty) in lines)
            {
                var p = products.First(x => x.ProductId == productId);
                order.OrderLines.Add(new OrderLine
                {
                    ProductId = p.ProductId,
                    Sku = p.Sku,
                    Quantity = qty,
                    UnitPrice = p.Price
                });
            }

            _db.Orders.Add(order);
            await _db.SaveChangesAsync();
            return order;
        }

        public async Task UpdateStatusAsync(Guid orderId, string status)
        {
            var o = await _db.Orders.FirstOrDefaultAsync(x => x.OrderId == orderId)
                    ?? throw new InvalidOperationException("Order not found");
            o.Status = status;
            o.UpdatedUtc = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
    }
}
