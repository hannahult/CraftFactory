using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IT.CraftOrders.Models
{
    public class Order
    {
        public Guid OrderId { get; set; }
        int CustomerId { get; set; }
        public string Status { get; set; } = "New";
        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedUtc { get; set; } = DateTime.UtcNow;
        public Customer Customer { get; set; }
        public ICollection<OrderLine> OrderLines { get; set; } = new List<OrderLine>();
        public ICollection<Incident> Incidents { get; set; } = new List<Incident>();
    }
}
