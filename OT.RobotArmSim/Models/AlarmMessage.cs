using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OT.RobotArmSim.Models
{
    public class AlarmMessage
    {
        public Guid? OrderId { get; set; }
        public string Code { get; set; } = "";     
        public string Severity { get; set; } = "";  
        public string? Msg { get; set; }
    }
}
