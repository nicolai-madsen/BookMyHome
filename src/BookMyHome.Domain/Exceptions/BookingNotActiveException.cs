namespace Domain.Exceptions
{
    public class BookingNotActiveException : DomainException
    {
        public BookingNotActiveException(Guid bookingId)
            : base("Booking isn't active and can't be cancelled")
        { }
    }
}
