using Domain.Errors;

namespace Domain.Exceptions
{
    public class BookingNotActiveException : DomainException
    {
        public BookingNotActiveException(Guid bookingId)
            : base(DomainErrorMessages.OverlappingBooking(bookingId))
        { }
    }
}
