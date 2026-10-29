using BookMyHome.Domain.Aggregates.Accommodations;
using BookMyHome.Domain.Exceptions;
using BookMyHome.Domain.Interfaces;
using BookMyHome.Domain.Interfaces.Repositories;

namespace BookMyHome.Application.Accommodations.Bookings.GetAccommodationBookings
{
    public class GetAccommodationBookingsUseCase : IGetAccommodationBookingsUseCase
    {
        private readonly IAccommodationRepository _accommodations;
        private readonly IUserRepository _users;

        public GetAccommodationBookingsUseCase(IAccommodationRepository repository, IUserRepository users)
        {
            _accommodations = repository;
            _users = users;
        }
        public async Task<IReadOnlyCollection<Booking>> ExecuteAsync(GetAccommodationBookingsQuery query, CancellationToken ct)
        {
            var accommodation = await _accommodations.GetByIdAsync(query.AccommodationId);

            if (accommodation is null)
                throw new AccommodationNotFoundException(query.AccommodationId);

            return accommodation.GetBookingsVisibleTo(query.UserId);
        }
    }
}
