using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IT.CraftOrders.Models
{
    public class OrderLine
    {
        public int OrderLineId { get; set; }  
        public Guid OrderId { get; set; }     
        public int ProductId { get; set; }   

        public string Sku { get; set; }      
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; } 

        public Order Order { get; set; }
        public Product Product { get; set; }
    }
}
