using BookMyHome.Domain.Exceptions;
using BookMyHome.Domain.Interfaces;
using BookMyHome.Domain.Interfaces.Repositories;
using BookMyHome.Domain.ValueObjects;

namespace BookMyHome.Application.Accommodations.RescheduleBooking
{
    public sealed class RescheduleBookingUseCase : IRescheduleBookingUseCase 
    {
        private readonly IAccommodationRepository _accommodations;
        private readonly IUserRepository _users;
        private readonly IUnitOfWork _unitOfWork;
        private readonly TimeProvider _timeProvider;

        public RescheduleBookingUseCase(IAccommodationRepository accommodations, IUserRepository users, IUnitOfWork unitOfWork, TimeProvider timeProvider)
        {
            _accommodations = accommodations;
            _users = users;
            _unitOfWork = unitOfWork;
            _timeProvider = timeProvider;
        }

        public async Task ExecuteAsync(RescheduleBookingUseCaseCommand command, CancellationToken ct = default)
        {
            var guest = await _users.GetByIdAsync(command.GuestId);

            if (guest is null)
                throw new UserNotFoundException(command.GuestId);



            var accommodation = await _accommodations.GetByIdAsync(command.AccommodationId);

            if (accommodation is null)
                throw new AccommodationNotFoundException(command.AccommodationId);

            var today = DateOnly.FromDateTime(_timeProvider.GetLocalNow().DateTime);

            var booking = accommodation.RescheduleBooking(command.BookingId, new DateRange(command.NewStartDate, command.NewEndDate), today);

            await _unitOfWork.SaveChangesAsync(ct);
        }
    }
}
