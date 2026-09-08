using Domain.ValueObjects;
using Domain.Enums;
using Domain.Errors;
using Domain.Exceptions;

namespace Domain.Aggregates.Accommodations
{
    public class Booking
    {
        public Guid Id { get; private set; }
        public Guid GuestId { get; private set; }
        public Accommodation Accommodation { get; private set; }
        public DateRange RentalPeriod { get; private set; } // RentalPeriod dato er "til -og med", så to bookinger på samme dag KAN IKKE ske, selvom gæsten fiser af tidligt. Det kunne give noget lost revenue...
        public decimal TotalPrice { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; } // People from all over the world book accommodations, thats why DateTimeOffset type
        public BookingStatus Status { get; private set; }
        public bool IsCompleted => Status == BookingStatus.Active && RentalPeriod.EndDate < DateOnly.FromDateTime(DateTime.Today); // The data decides if a booking is completed or not
        public decimal PricePerDayWhenBooked { get; private set; } // To store the historical data

        private Booking() { }
        public Booking(Guid id, Guid guestId, DateRange rentalPeriod, decimal pricePerDay)
        {
            if (id == Guid.Empty)
                throw new ArgumentException(nameof(id));

            if (guestId == Guid.Empty)
                throw new ArgumentException(nameof(guestId));

            if (rentalPeriod is null)
                throw new ArgumentException(nameof(rentalPeriod));

            if (rentalPeriod.StartDate < DateOnly.FromDateTime(DateTime.Today))
                throw new ArgumentException(DomainErrorMessages.StartTimeMustBeInTheFuture, nameof(rentalPeriod.StartDate));

            if (pricePerDay <= 0) 
                throw new InvalidOperationException("Price per day cannot be less than -or equal to 0");
                

            Id = id;
            GuestId = guestId;
            RentalPeriod = rentalPeriod;
            PricePerDayWhenBooked = pricePerDay;
            TotalPrice = pricePerDay * rentalPeriod.TotalDays;
            CreatedAt = DateTimeOffset.UtcNow;
            Status = BookingStatus.Active;
        }

        public void Reschedule(DateRange dateRange)
        {
            
        }

        public void CancelByGuest() 
        {
            if (Status != BookingStatus.Active)
                throw new BookingNotActiveException(Id); // Booking, obviously, has to be active to be cancelled

            Status = BookingStatus.CancelledByGuest;
        }

        public void CancelByHost()
        {
            if (Status != BookingStatus.Active)
                throw new BookingNotActiveException(Id); // Booking, obviously, has to be active to be cancelled

            Status = BookingStatus.CancelledByHost;
        }
    }
}
