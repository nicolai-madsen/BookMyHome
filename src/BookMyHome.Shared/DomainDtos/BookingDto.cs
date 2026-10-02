namespace Shared.DomainDtos
{
    public record BookingDto(
        Guid Id,
        Guid GuestId,
        DateOnly StartDate,
        DateOnly EndDate,
        decimal TotalPrice,
        string Status);
}
