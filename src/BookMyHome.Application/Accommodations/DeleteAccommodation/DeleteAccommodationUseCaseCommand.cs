namespace BookMyHome.Application.Accommodations.DeleteAccommodation
{
    public sealed record DeleteAccommodationUseCaseCommand(
        Guid AccommodationId,
        Guid UserId   // TODO Opgave 11: fra JWT 'sub'
    );
}