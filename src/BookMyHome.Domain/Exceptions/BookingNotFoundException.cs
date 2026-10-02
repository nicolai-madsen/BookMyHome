namespace Domain.Exceptions
{
    public class BookingNotFoundException : DomainException
    {
        public BookingNotFoundException(Guid bookingId)
            : base($"Booking with ID {bookingId} not found")
        { }
    }
}
