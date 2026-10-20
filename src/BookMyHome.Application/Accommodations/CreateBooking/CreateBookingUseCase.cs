using BookMyHome.Domain.Aggregates.Accommodations;
using BookMyHome.Domain.Exceptions;
using BookMyHome.Domain.Interfaces;
using BookMyHome.Domain.Interfaces.Repositories;
using BookMyHome.Domain.ValueObjects;

namespace BookMyHome.Application.Accommodations.CreateBooking
{
    public class CreateBookingUseCase : ICreateBookingUseCase
    {
        private readonly IAccommodationRepository _accommodationRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        private readonly TimeProvider _time;
        public CreateBookingUseCase(IAccommodationRepository accommodationRepository, IUserRepository userRepository, IUnitOfWork unitOfWork, TimeProvider time)
        {
            _accommodationRepository = accommodationRepository;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _time = time;
        }
        public async Task<Guid> ExecuteAsync(CreateBookingCommand command, CancellationToken ct = default)
        {
            var guest = await _userRepository.GetByIdAsync(command.GuestId);

            if (guest is null)
                throw new UserNotFoundException(command.GuestId);

            var accommodation = await _accommodationRepository.GetByIdAsync(command.AccommodationId);

            if (accommodation is null)
                throw new AccommodationNotFoundException(command.AccommodationId);

            var period = new DateRange(command.StartDate, command.EndDate);

            var today = DateOnly.FromDateTime(_time.GetLocalNow().DateTime);

            var booking = accommodation.CreateBooking(guest.Id, period, today);

            await _unitOfWork.SaveChangesAsync(ct);
            return booking.Id;
        }
    }
}
