using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IT.CraftOrders.Models
{
    public class Incident
    {
        public int IncidentId { get; set; }   
        public Guid? OrderId { get; set; }   
        public string Code { get; set; }     
        public string Severity { get; set; } 
        public string Message { get; set; }
        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

        public Order Order { get; set; }
    }
}
