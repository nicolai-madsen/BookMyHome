using Domain.Aggregates.Accommodations;
using Shared.DomainDtos;

namespace Application.Mapping
{
    public static class BookingMappingExtensions
    {
        public static BookingDto ToDto(this Booking booking) => new(
            booking.Id,
            booking.GuestId,
            booking.RentalPeriod.StartDate,
            booking.RentalPeriod.EndDate,
            booking.TotalPrice,
            booking.Status.ToString());

        public static IEnumerable<BookingDto> ToDtos(this IEnumerable<Booking> bookings) => bookings.Select(b => b.ToDto());
    }
}
