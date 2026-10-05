namespace BookMyHome.Application.Accommodations.CreateAccommodation
{
    public sealed record CreateAccommodationUseCaseCommand(
        Guid HostId,  // HUUUUUSK TIL Opgave 11: den skal komme fra JWT 'sub', ikke fra client!!
        string StreetName, string StreetNumber, string City, string ZipCode, string Country,
        DateOnly AvailableFrom, DateOnly AvailableTo,
        decimal PricePerDay
    );
}
