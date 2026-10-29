using BookMyHome.Domain.Exceptions;
using BookMyHome.Domain.Interfaces;
using BookMyHome.Domain.Interfaces.Repositories;

namespace BookMyHome.Application.Accommodations.Bookings.CancelBooking
{
    public class CancelBookingUseCase : ICancelBookingUseCase
    {
        private readonly IAccommodationRepository _repository;
        private IUnitOfWork _unitOfWork;
        private readonly TimeProvider timeProvider;

        public CancelBookingUseCase(IAccommodationRepository repository, IUnitOfWork unitOfWork, TimeProvider timeProvider)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            this.timeProvider = timeProvider;
        }

        public async Task ExecuteAsync(CancelBookingUseCaseCommand command, CancellationToken ct)
        {
            var accommodation = await _repository.GetByIdAsync(command.AccommodationId) ?? throw new AccommodationNotFoundException(command.AccommodationId);

            var today = DateOnly.FromDateTime(timeProvider.GetLocalNow().DateTime);
            accommodation.CancelBooking(command.BookingId, command.UserId);

            await _unitOfWork.SaveChangesAsync(ct);
        }
    }
}
