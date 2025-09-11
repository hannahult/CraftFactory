using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OT.RobotArmSim.Models
{
    public class StatusMessage
    {
        public Guid OrderId { get; set; }
        public string State { get; set; } = ""; 
        public string? Msg { get; set; }
    }
}
