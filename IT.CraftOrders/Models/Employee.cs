using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IT.CraftOrders.Models
{
    public class Employee
    {
        public int EmployeeId { get; set; }       
        public string Email { get; set; }    
        public string PasswordHash { get; set; }
        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginUtc { get; set; }
    }
}
