using Domain.ValueObjects;

namespace Domain.Exceptions
{
    public class BookingIsOutsideAvailablePeriodException : DomainException
    {
        public BookingIsOutsideAvailablePeriodException(DateRange requested, DateRange available)
            : base($"Booking on: {requested}, is outside the accommodation's available period: {available}")
        { }
    }
}
