using BookMyHome.Domain.ValueObjects;

namespace BookMyHome.Domain.Exceptions
{
    public class AccommodationNotFoundException : DomainException
    {
        public AccommodationNotFoundException(Guid acccommodationId)
            : base($"Accommodation {acccommodationId} was not found.")
        { }
    }
}
