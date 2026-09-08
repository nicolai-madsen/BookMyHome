using Domain.ValueObjects;

namespace Domain.Aggregates.Accommodations
{
    public class Accommodation
    {
        public Guid Id { get; private set; }
        public Guid HostId { get; private set; } // Not sure if they're supposed to know about each other both ways? 
        public Address Address { get; private set; }
        public DateRange AvailablePeriod { get; private set; }
        public decimal PricePerDay { get; private set; }
    }
}
