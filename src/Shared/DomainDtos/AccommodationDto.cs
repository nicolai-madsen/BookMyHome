namespace Shared.DomainDtos
{
    public record AccommodationDto(
        Guid Id,
        /*
        Guid HostId,
        string StreetName,
        string StreetNumber,
        string City,
        string ZipCode,
        string Country,
        DateOnly AvailablePeriod_Start,
        DateOnly AvailablePeriod_End, 
        */
        decimal PricePerDay
        );
}
