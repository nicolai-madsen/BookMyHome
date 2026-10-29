using BookMyHome.Domain.Aggregates.Accommodations;
using BookMyHome.Domain.Exceptions;
using BookMyHome.Domain.Interfaces.Repositories;

namespace BookMyHome.Application.Accommodations.Bookings.GetBookingById
{
    public class GetBookingByIdUseCase : IGetBookingByIdUseCase
    {
        private readonly IAccommodationRepository _repository;
        public GetBookingByIdUseCase(IAccommodationRepository accommodationRepository)
        {
            _repository = accommodationRepository;
        }

        public async Task<Booking> ExecuteAsync(GetBookingByIdQuery query, CancellationToken ct)
        {
            var accommodation = await _repository.GetByIdAsync(query.AccommodationId) ?? throw new AccommodationNotFoundException(query.AccommodationId);

            return accommodation.GetBooking(query.BookingId);
        }
    }
}
