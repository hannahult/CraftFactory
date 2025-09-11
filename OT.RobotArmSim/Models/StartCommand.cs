using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OT.RobotArmSim.Models
{
    public class StartCommand
    {
        public Guid OrderId { get; set; }
        public string Sku { get; set; } = "";
        public int Qty { get; set; }
    }
}
