using Domain.ValueObjects;
using Domain.Enums;
using Domain.Exceptions;

namespace Domain.Aggregates.Accommodations
{
    public class Booking
    {
        public Guid Id { get; private set; }
        public Guid GuestId { get; private set; }
        public Guid AccommodationId { get; private set; }
        public DateRange RentalPeriod { get; private set; } // RentalPeriod dato er "til -og med", så to bookinger på samme dag KAN IKKE ske, selvom gæsten fiser af tidligt. Det kunne give noget lost revenue...
        public decimal TotalPrice { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; } // People from all over the world book accommodations, thats why DateTimeOffset type
        public BookingStatus Status { get; private set; }
        public bool IsCompleted => Status == BookingStatus.Active && RentalPeriod.EndDate < DateOnly.FromDateTime(DateTime.Today); // The data decides if a booking is completed or not
        public decimal PricePerDayWhenBooked { get; private set; } // To store the historical data

        private Booking() { }
        public Booking(Guid id, Guid guestId, Guid accommodationId, DateRange rentalPeriod, decimal pricePerDay, DateOnly today)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Id cannot be empty", nameof(id));

            if (guestId == Guid.Empty)
                throw new ArgumentException("Guest Id cannot be empty", nameof(guestId));

            if (accommodationId == Guid.Empty)
                throw new ArgumentException("Accommodation Id cannot be empty", nameof(accommodationId));

            if (rentalPeriod is null)
                ArgumentNullException.ThrowIfNull(rentalPeriod);

            if (rentalPeriod.StartDate < today) 
                throw new BookingStartDateCannotBeInThePastException();
                

            Id = id;
            GuestId = guestId;
            AccommodationId = accommodationId;
            RentalPeriod = rentalPeriod;
            PricePerDayWhenBooked = pricePerDay;
            TotalPrice = pricePerDay * rentalPeriod.TotalDays;
            CreatedAt = DateTimeOffset.UtcNow;
            Status = BookingStatus.Active;
        }

        public void Reschedule(DateRange newPeriod, DateOnly today)
        {
            ArgumentNullException.ThrowIfNull(newPeriod);

            if (Status != BookingStatus.Active)
                throw new BookingNotActiveException(Id);

            if (newPeriod.StartDate < today)
                throw new BookingStartDateCannotBeInThePastException();

            RentalPeriod = newPeriod;
            TotalPrice = PricePerDayWhenBooked * RentalPeriod.TotalDays;
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
