using BookMyHome.Domain.ValueObjects;

namespace BookMyHome.Domain.Exceptions
{
    public sealed class ActiveBookingOutsideAvailablePeriodException : DomainException
    {
        public ActiveBookingOutsideAvailablePeriodException(Guid bookingId, DateRange newPeriod)
            : base($"Cannot change available period to {newPeriod}: active booking {bookingId} would fall outside it.") { }
    }
}