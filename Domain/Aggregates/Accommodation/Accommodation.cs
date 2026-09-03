using Domain.ValueObjects;

namespace Domain.Aggregates.Accommodation
{
    public class Accommodation
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; } // Not sure if they're supposed to know about each other both ways? 
        public Address Address { get; private set; }
        public DateOnly RentalPeriod { get; private set; }
    }
}
