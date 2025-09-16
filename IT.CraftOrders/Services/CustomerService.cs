using IT.CraftOrders.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IT.CraftOrders.Services
{
    public class CustomerService
    {
        private readonly Data.CraftFactoryDbContext _db;

        public CustomerService(Data.CraftFactoryDbContext db)
        {
            _db = db;
        }

        public Task<Customer?> GetCustomerByIdAsync(int id)
        {
            return _db.Customers.FirstOrDefaultAsync(c => c.CustomerId == id);
        }
    }
}
