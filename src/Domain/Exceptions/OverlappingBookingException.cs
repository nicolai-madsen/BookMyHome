namespace Domain.Exceptions
{
    public class OverlappingBookingException : DomainException
    {
        public Guid BookingId { get; }
        public OverlappingBookingException(Guid bookingId)
            
            : base($"Start time of the booking: {bookingId} overlaps with an already existing booking")
        { 
            BookingId = bookingId;
        }
    }
}
