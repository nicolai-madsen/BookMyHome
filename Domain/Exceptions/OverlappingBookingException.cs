using Domain.Errors;

namespace Domain.Exceptions
{
    public class OverlappingBookingException : DomainException
    {
        public OverlappingBookingException(Guid bookingId)
            : base(DomainErrorMessages.OverlappingBooking(bookingId))
        { }
    }
}
