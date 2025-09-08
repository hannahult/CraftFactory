namespace IT.CraftOrders.Api.Dtos
{
    public record OrderLineCreateDto(int ProductId, int Quantity);
    public record OrderCreateDto(int CustomerId, List<OrderLineCreateDto> Lines);

    public record OrderSummaryDto(Guid OrderId, string Status, DateTime CreatedUtc);

    public record OrderLineDto(int ProductId, string Sku, string Name, int Quantity, decimal UnitPrice);
    public record OrderDetailsDto(Guid OrderId, string Status, DateTime CreatedUtc, string CustomerName, List<OrderLineDto> Lines);

    public record UpdateOrderStatusDto(string Status);
}
