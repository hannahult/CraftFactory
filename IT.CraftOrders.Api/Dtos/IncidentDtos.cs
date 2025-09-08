namespace IT.CraftOrders.Api.Dtos
{

    public record IncidentCreateDto(Guid? OrderId, string Code, string Severity, string Message);
    public record IncidentDto(int IncidentId, Guid? OrderId, string Code, string Severity, string Message, DateTime CreatedUtc);

}
