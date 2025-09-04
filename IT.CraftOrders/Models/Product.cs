using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IT.CraftOrders.Models
{
    public class Product
    {
        public int ProductId { get; set; }   
        public string Sku { get; set; }    
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<OrderLine> OrderLines { get; set; } = new List<OrderLine>();
    }
}
