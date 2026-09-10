using Domain.Errors;

namespace Domain.Exceptions
{
    public class BookingMustBeInTheFutureException : DomainException
    {
        public BookingMustBeInTheFutureException(Guid bookingId)
            : base(DomainErrorMessages.StartTimeMustBeInTheFuture)
        { }
    }
}
