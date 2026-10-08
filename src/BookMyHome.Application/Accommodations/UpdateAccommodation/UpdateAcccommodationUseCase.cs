using BookMyHome.Domain.Exceptions;
using BookMyHome.Domain.Interfaces;
using BookMyHome.Domain.Interfaces.Repositories;
using BookMyHome.Domain.ValueObjects;

namespace BookMyHome.Application.Accommodations.UpdateAccommodation
{
    public sealed class UpdateAccommodationUseCase(IAccommodationRepository accommodations, IUnitOfWork unitOfWork, TimeProvider time) : IUpdateAccommodationUseCase
    {
        public async Task ExecuteAsync(UpdateAccommodationUseCaseCommand command, CancellationToken ct = default)
        {
            var accommodation = await accommodations.GetByIdAsync(command.AccommodationId)
                ?? throw new AccommodationNotFoundException(command.AccommodationId);

            var address = new Address(command.StreetName, command.StreetNumber,
                                      command.City, command.ZipCode, command.Country);
            var period = new DateRange(command.AvailableFrom, command.AvailableTo);
            var today = DateOnly.FromDateTime(time.GetLocalNow().DateTime);

            accommodation.UpdateDetails(command.UserId, address, period, command.PricePerDay, today);

            await unitOfWork.SaveChangesAsync(ct);
        }
    }
}