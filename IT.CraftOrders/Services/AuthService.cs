using IT.CraftOrders.Data;
using IT.CraftOrders.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IT.CraftOrders.Services
{
    public class AuthService
    {
        private readonly CraftFactoryDbContext _db;
        private readonly IncidentService _incidentService;
        public AuthService(CraftFactoryDbContext db, IncidentService incidentService)
        {
            _db = db;
            _incidentService = incidentService;
        }

        public async Task<Employee?> LoginAsync(string email, string password)
        {
            var user = _db.Employees.FirstOrDefault(u => u.Email == email);
            if (user == null) return null;
            bool verified = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);

            // Log failed login attempt
            if (!verified)
            {
                await _incidentService.CreateAsync(null,"LOGIN_FAIL","Warning","Attempted login failed");
                return null;
            }

            user.LastLoginUtc = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return user;
        }
    }
}
