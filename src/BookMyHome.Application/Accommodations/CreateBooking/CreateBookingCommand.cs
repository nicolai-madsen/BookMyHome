namespace BookMyHome.Application.Accommodations.CreateBooking
{
    public sealed record CreateBookingCommand(
        Guid GuestId,
        Guid AccommodationId,
        
        DateOnly Start,
        DateOnly End,
        
        decimal PricePerDay);
}
