using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integration.DataBridge.Dtos
{
    public record OrderSummaryDto(Guid OrderId, string Status, DateTime CreatedUtc);

    public class UpdateOrderStatusDto
    {
        public string Status { get; set; } = "New";

    }
}
