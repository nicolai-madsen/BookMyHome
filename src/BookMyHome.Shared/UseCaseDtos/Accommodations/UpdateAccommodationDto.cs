namespace BookMyHome.Shared.UseCaseDtos.Accommodations
{
    public sealed record UpdateAccommodationRequest(
        string StreetName,
        string StreetNumber,
        string City,
        string ZipCode,
        string Country,
        DateOnly AvailableFrom,
        DateOnly AvailableTo,
        decimal PricePerDay);
}