namespace BookMyHome.Shared.UseCaseDtos
{
    public sealed record CreateAccommodationRequest(
        Guid HostId,

        string StreetName,
        string StreetNumber,
        string City,
        string ZipCode,
        string Country,

        DateOnly AvailableFrom,
        DateOnly AvailableTo,

        decimal PricePerDay
    );
}
