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
    public class IncidentService
    {
        private readonly CraftFactoryDbContext _db;

        public IncidentService(CraftFactoryDbContext db)
        {  
            _db = db; 
        }

        public async Task CreateAsync(Guid? orderId, string code, string severity, string message)
        {
            var incident = new Incident
            {
                OrderId = orderId,
                Code = code,
                Severity = severity,
                Message = message,
                CreatedUtc = DateTime.UtcNow
            };
            _db.Incidents.Add(incident);
            await _db.SaveChangesAsync();
        }
    }
}
