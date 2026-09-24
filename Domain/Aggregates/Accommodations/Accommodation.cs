using Domain.Exceptions;
using Domain.ValueObjects;
using Domain.Enums;

namespace Domain.Aggregates.Accommodations
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

        private Accommodation() { }

        public Accommodation(Guid id, Guid hostId, Address address, DateRange availablePeriod, decimal pricePerDay)
        {
            if (id == Guid.Empty) throw new ArgumentException("Id cannot be empty;", nameof(id));

            if (hostId == Guid.Empty) throw new ArgumentException("Host Id cannot be empty;", nameof(hostId));

            ArgumentNullException.ThrowIfNull(address);

            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pricePerDay);

            Id = id;
            HostId = hostId;
            Address = address;
            AvailablePeriod = availablePeriod;
            PricePerDay = pricePerDay;
        }

        public Booking AddBooking(Guid guestId, DateRange period, DateOnly today)
        {
            foreach (var existing in _bookings)
            {
                if (existing.Status == BookingStatus.Active && existing.RentalPeriod.OverlapsWith(period))
                    throw new OverlappingBookingException(existing.Id);
            }

            var booking = new Booking(Guid.NewGuid(), guestId, Id, period, PricePerDay, today);
            _bookings.Add(booking);
            return booking;
        }

        public Booking RescheduleBooking(Guid bookingId, DateRange newPeriod, DateOnly today)
        {
            var target = _bookings.FirstOrDefault(b => b.Id == bookingId);

            if (target is null)
                throw new BookingNotFoundException(bookingId);


            var overlap = _bookings.FirstOrDefault(b => b.Id != bookingId && b.Status == BookingStatus.Active && b.RentalPeriod.OverlapsWith(newPeriod));
            if (overlap is not null)
                throw new OverlappingBookingException(overlap.Id);

            target.Reschedule(newPeriod, today);
            return target;
        }
    }
}