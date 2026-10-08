namespace BookMyHome.Application.Accommodations.UpdateAccommodation
{
    public sealed record UpdateAccommodationUseCaseCommand(
        Guid AccommodationId,
        Guid UserId,   // TODO Opgave 11: fra JWT 'sub'
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