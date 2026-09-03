using Domain.ValueObjects;

namespace Domain.Aggregates.Accommodation
{
    public class Booking
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public DateRange RentalPeriod { get; private set; }
        public decimal TotalPrice { get; private set; }
        public DateOnly CreatedAtDate { get; private set; }

        public Booking() { }
        public Booking(Guid id, Guid userId, DateRange rentalPeriod, decimal totalPrice)
        {
            if (id == Guid.Empty)
                throw new ArgumentException(nameof(id));

            if (userId == Guid.Empty)
                throw new ArgumentException(nameof(id));

            if (rentalPeriod is null)
                throw new ArgumentException(nameof(rentalPeriod));

            if (totalPrice <= 0) 
                throw new InvalidOperationException("Total price must be above or equal to 0");
                

            Id = id;
            UserId = userId;
            RentalPeriod = rentalPeriod;
            TotalPrice = totalPrice;
            CreatedAtDate = DateOnly.FromDateTime(DateTime.Now); 
        }
    }
}
