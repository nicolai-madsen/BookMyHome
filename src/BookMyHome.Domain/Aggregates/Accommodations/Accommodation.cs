using BookMyHome.Domain.Exceptions;
using BookMyHome.Domain.ValueObjects;
using BookMyHome.Domain.Enums;

namespace BookMyHome.Domain.Aggregates.Accommodations
{
    public class Accommodation
    {
        public Guid Id { get; private set; }
        public Guid HostId { get; private set; }
        private readonly List<Booking> _bookings = new(); // field NOT property. This hold the list, so it can't be changed from outside the root
        public IReadOnlyCollection<Booking> Bookings => _bookings.AsReadOnly(); // Exposed list, that the rest of the program can use
        public Address Address { get; private set; }
        public DateRange AvailablePeriod { get; private set; }
        public decimal PricePerDay { get; private set; }
        public DateTimeOffset? DeletedAt { get; private set; }
        public bool IsDeleted => DeletedAt is not null;

        // Aggregate version for optimistic concurrency. Incremented by every method that changes the aggregate, including its bookings.
        public int Version { get; private set; } 

        private Accommodation() { }

        public Accommodation(Guid id, Guid hostId, Address address, DateRange availablePeriod, decimal pricePerDay)
        {
            if (id == Guid.Empty) throw new ArgumentException("Id cannot be empty;", nameof(id));

            if (hostId == Guid.Empty) throw new ArgumentException("Host Id cannot be empty;", nameof(hostId));

            ArgumentNullException.ThrowIfNull(address);
            ArgumentNullException.ThrowIfNull(availablePeriod);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pricePerDay);

            Id = id;
            HostId = hostId;
            Address = address;
            AvailablePeriod = availablePeriod;
            PricePerDay = pricePerDay;
        }

        public Booking CreateBooking(Guid guestId, DateRange period, DateOnly today)
        {
          
            if (guestId == HostId)
                throw new HostCannotBookOwnAccommodationException(Id, guestId);

            if (!this.AvailablePeriod.Contains(period))
                throw new BookingIsOutsideAvailablePeriodException(period, this.AvailablePeriod);

            foreach(var existing in _bookings)
            {
                if (existing.Status == BookingStatus.Active && existing.RentalPeriod.OverlapsWith(period))
                    throw new OverlappingBookingException(existing.Id);
            }
              
            var booking = new Booking(Guid.NewGuid(), guestId, Id, period, PricePerDay, today);
            _bookings.Add(booking);
            Version++;
            return booking;
        }

        public Booking RescheduleBooking(Guid bookingId, DateRange newPeriod, DateOnly today)
        {
            var targetBooking = _bookings.FirstOrDefault(b => b.Id == bookingId);

            if (targetBooking is null)
                throw new BookingNotFoundException(bookingId);

            if (!this.AvailablePeriod.Contains(newPeriod))
                throw new BookingIsOutsideAvailablePeriodException(newPeriod, this.AvailablePeriod);

            var overlap = _bookings.FirstOrDefault(b => b.Id != bookingId && b.Status == BookingStatus.Active && b.RentalPeriod.OverlapsWith(newPeriod));
            if (overlap is not null)
                throw new OverlappingBookingException(overlap.Id);

            targetBooking.Reschedule(newPeriod, today);
            Version++;
            return targetBooking;
        }

        
        public Booking CancelBooking(Guid bookingId, Guid userId)
        {
            var targetBooking = _bookings.FirstOrDefault(b => b.Id == bookingId)
                ?? throw new BookingNotFoundException(bookingId); 

            var cancelledBy = userId == targetBooking.GuestId ? CancellationParty.Guest
                : userId == HostId ? CancellationParty.Host
                : throw new UnauthorizedDomainActionException(userId, "cancel booking", bookingId);

            targetBooking.Cancel(cancelledBy);
            Version++;
            return targetBooking;
        }

        public void UpdateDetails(Guid userId, Address address, DateRange availablePeriod, decimal pricePerDay, DateOnly today) 
        {
            EnsureIsHost(userId, "update accommodation");

            ArgumentNullException.ThrowIfNull(address);
            ArgumentNullException.ThrowIfNull(availablePeriod);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pricePerDay);

            // Find booking in the new available period for the accommodation
            var stranded = UpComingOrOngoingBookings(today)
                .FirstOrDefault(b => !availablePeriod.Contains(b.RentalPeriod));

            // If there is any bookings, throw the exception
            if (stranded is not null)
                throw new ActiveBookingOutsideAvailablePeriodException(stranded.Id, availablePeriod);

            Address = address;
            AvailablePeriod = availablePeriod;
            PricePerDay = pricePerDay;
            Version++;
        }

        public void Delete(Guid userId, DateTimeOffset now)
        {
            EnsureIsHost(userId, "delete accommodation");

            var today = DateOnly.FromDateTime(now.DateTime);

            if (UpComingOrOngoingBookings(today).Any())
                throw new AccommodationHasActiveBookingsException(Id);

            DeletedAt = now;
        }

        private IEnumerable<Booking> UpComingOrOngoingBookings(DateOnly today) =>
            _bookings.Where(b => b.Status == BookingStatus.Active && b.RentalPeriod.EndDate >= today);

        private void EnsureIsHost(Guid userId, string action)
        {
            if (userId != HostId)
                throw new UnauthorizedDomainActionException(userId, action, Id);
            // No exception? Good to go!
        }
    }
}