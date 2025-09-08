
using IT.CraftOrders.Data;
using Microsoft.EntityFrameworkCore;

namespace IT.CraftOrders.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddDbContext<CraftFactoryDbContext>(o =>
                o.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            builder.Services.AddEndpointsApiExplorer();

            var app = builder.Build();

            app.UseHttpsRedirection();

            var api = app.MapGroup("/api/v1");

            // PRODUCTS
            api.MapGet("/products", async (CraftFactoryDbContext db) =>
            {
                var items = await db.Products
                    .Where(p => p.IsActive)
                    .OrderBy(p => p.Name)
                    .Select(p => new ProductDto(p.ProductId, p.Sku, p.Name, p.Price))
                    .ToListAsync();
                return Results.Ok(items);
            });

            // ORDERS
            api.MapPost("/orders", async (OrderCreateDto req, CraftFactoryDbContext db) =>
            {
                if (req.Lines is null || req.Lines.Count == 0) return Results.BadRequest("At least one line required.");

                var prodIds = req.Lines.Select(l => l.ProductId).ToHashSet();
                var products = await db.Products.Where(p => prodIds.Contains(p.ProductId) && p.IsActive).ToListAsync();
                if (products.Count != prodIds.Count) return Results.BadRequest("One or more products not found/active.");
                if (req.Lines.Any(l => l.Quantity <= 0)) return Results.BadRequest("Quantity must be > 0.");

                var order = new Order { CustomerId = req.CustomerId, Status = "New", CreatedUtc = DateTime.UtcNow, UpdatedUtc = DateTime.UtcNow };
                foreach (var l in req.Lines)
                {
                    var p = products.First(x => x.ProductId == l.ProductId);
                    order.OrderLines.Add(new OrderLine { ProductId = p.ProductId, Sku = p.Sku, Quantity = l.Quantity, UnitPrice = p.Price });
                }
                db.Orders.Add(order);
                await db.SaveChangesAsync();

                var dto = new OrderSummaryDto(order.OrderId, order.Status, order.CreatedUtc);
                return Results.Created($"/api/v1/orders/{order.OrderId}", dto);
            });

            api.MapGet("/orders", async (string? status, int take, CraftFactoryDbContext db) =>
            {
                take = take is > 0 and <= 100 ? take : 10;
                var q = db.Orders.AsQueryable();
                if (!string.IsNullOrWhiteSpace(status)) q = q.Where(o => o.Status == status);
                var items = await q.OrderByDescending(o => o.CreatedUtc)
                    .Take(take)
                    .Select(o => new OrderSummaryDto(o.OrderId, o.Status, o.CreatedUtc))
                    .ToListAsync();
                return Results.Ok(items);
            });

            api.MapGet("/orders/{id:guid}", async (Guid id, CraftFactoryDbContext db) =>
            {
                var o = await db.Orders
                    .Include(x => x.Customer)
                    .Include(x => x.OrderLines).ThenInclude(ol => ol.Product)
                    .FirstOrDefaultAsync(x => x.OrderId == id);
                if (o is null) return Results.NotFound();

                var dto = new OrderDetailsDto(
                    o.OrderId, o.Status, o.CreatedUtc, o.Customer?.Name ?? "Unknown",
                    o.OrderLines.Select(ol => new OrderLineDto(ol.ProductId, ol.Sku, ol.Product?.Name ?? "", ol.Quantity, ol.UnitPrice)).ToList()
                );
                return Results.Ok(dto);
            });

            api.MapPut("/orders/{id:guid}/status", async (Guid id, UpdateOrderStatusDto req, CraftFactoryDbContext db) =>
            {
                var allowed = new[] { "New", "InProgress", "Completed", "Failed", "Cancelled" };
                if (!allowed.Contains(req.Status)) return Results.BadRequest("Invalid status.");

                var o = await db.Orders.FirstOrDefaultAsync(x => x.OrderId == id);
                if (o is null) return Results.NotFound();

                o.Status = req.Status;
                o.UpdatedUtc = DateTime.UtcNow;
                await db.SaveChangesAsync();
                return Results.NoContent();
            });

            // INCIDENTS
            api.MapPost("/incidents", async (IncidentCreateDto req, CraftFactoryDbContext db) =>
            {
                var incident = new Incident { OrderId = req.OrderId, Code = req.Code, Severity = req.Severity, Message = req.Message, CreatedUtc = DateTime.UtcNow };
                db.Incidents.Add(incident);
                await db.SaveChangesAsync();
                return Results.Created($"/api/v1/incidents/{incident.IncidentId}",
                    new IncidentDto(incident.IncidentId, incident.OrderId, incident.Code, incident.Severity, incident.Message, incident.CreatedUtc));
            });

            api.MapGet("/health", () => Results.Ok(new { status = "ok" }));

            app.Run();
        }
    }
}
