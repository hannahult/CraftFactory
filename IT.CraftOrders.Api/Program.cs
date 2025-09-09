
using IT.CraftOrders.Api.Dtos;
using IT.CraftOrders.Data;
using IT.CraftOrders.Models;
using IT.CraftOrders.Services;
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

            builder.Services.AddScoped<ProductService>();
            builder.Services.AddScoped<OrderService>();

            var app = builder.Build();

            app.UseHttpsRedirection();

            var api = app.MapGroup("/api/v1");

            // PRODUCTS
            //get all products
            api.MapGet("/products", async (ProductService products) =>
            {
                var list = await products.GetAllProductsAsync();
                var dtos = list.Select(p => new ProductDto(p.ProductId, p.Sku, p.Name, p.Price));
                return Results.Ok(dtos);
            });

            // ORDERS
            //create new order
            api.MapPost("/orders", async (OrderCreateDto req, OrderService orders) =>
            {
                if (req.Lines is null || req.Lines.Count == 0)
                    return Results.BadRequest("At least one line required.");

                if (req.Lines.Any(l => l.Quantity <= 0))
                    return Results.BadRequest("Quantity must be > 0.");

                // Service sköter produktvalidering via DB (du kan utöka där)
                var lines = req.Lines.Select(l => (l.ProductId, l.Quantity));
                var order = await orders.CreateAsync(req.CustomerId, lines);

                var dto = new OrderSummaryDto(order.OrderId, order.Status, order.CreatedUtc);
                return Results.Created($"/api/v1/orders/{order.OrderId}", dto);
            });

            //get orders
            api.MapGet("/orders", async (string? status, int? take, CraftFactoryDbContext db) =>
            {
                var q = db.Orders.AsQueryable();
                if (!string.IsNullOrWhiteSpace(status)) q = q.Where(o => o.Status == status);

                var list = await q.OrderByDescending(o => o.CreatedUtc)
                                  .Take(take is > 0 and <= 100 ? take.Value : 10)
                                  .Select(o => new OrderSummaryDto(o.OrderId, o.Status, o.CreatedUtc))
                                  .ToListAsync();
                return Results.Ok(list);
            });

            //get order by id
            api.MapGet("/orders/{id:guid}", async (Guid id, CraftFactoryDbContext db) =>
            {
                var o = await db.Orders
                    .Include(x => x.Customer)
                    .Include(x => x.OrderLines).ThenInclude(ol => ol.Product)
                    .FirstOrDefaultAsync(x => x.OrderId == id);

                if (o is null) return Results.NotFound();

                var dto = new OrderDetailsDto(
                    o.OrderId, o.Status, o.CreatedUtc, o.Customer?.Name ?? "Unknown",
                    o.OrderLines.Select(ol => new OrderLineDto(
                        ol.ProductId, ol.Sku, ol.Product?.Name ?? "", ol.Quantity, ol.UnitPrice
                    )).ToList()
                );
                return Results.Ok(dto);
            });

            //update order status
            api.MapPut("/orders/{id:guid}/status", async (Guid id, UpdateOrderStatusDto req, OrderService orders) =>
            {
                var allowed = new[] { "New", "InProgress", "Completed", "Failed", "Cancelled" };
                if (!allowed.Contains(req.Status)) return Results.BadRequest("Invalid status.");

                try
                {
                    await orders.UpdateStatusAsync(id, req.Status);
                    return Results.NoContent();
                }
                catch (InvalidOperationException)
                {
                    return Results.NotFound();
                }
            });

            // INCIDENTS
            //log new incident
            api.MapPost("/incidents", async (IncidentCreateDto req, CraftFactoryDbContext db) =>
            {
                var incident = new Incident
                {
                    OrderId = req.OrderId,
                    Code = req.Code,
                    Severity = req.Severity,
                    Message = req.Message,
                    CreatedUtc = DateTime.UtcNow
                };
                db.Incidents.Add(incident);
                await db.SaveChangesAsync();

                return Results.Created($"/api/v1/incidents/{incident.IncidentId}",
                    new IncidentDto(incident.IncidentId, incident.OrderId, incident.Code, incident.Severity, incident.Message, incident.CreatedUtc));
            });

            app.Run();
        }
    }
}
