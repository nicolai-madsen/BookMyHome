namespace BookMyHome.Shared.UseCaseDtos
{
    public sealed record UpdateAccommodationRequest(
        Guid UserId,   // HUSK!!! Opgave 11: fjernes, kommer fra JWT
        string StreetName,
        string StreetNumber,
        string City,
        string ZipCode,
        string Country,
        DateOnly AvailableFrom,
        DateOnly AvailableTo,
        decimal PricePerDay);
}