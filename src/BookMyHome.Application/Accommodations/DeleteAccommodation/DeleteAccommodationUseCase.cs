using BookMyHome.Domain.Exceptions;
using BookMyHome.Domain.Interfaces;
using BookMyHome.Domain.Interfaces.Repositories;

namespace BookMyHome.Application.Accommodations.DeleteAccommodation
{
    public sealed class DeleteAccommodationUseCase(IAccommodationRepository accommodations, IUnitOfWork unitOfWork, TimeProvider time) : IDeleteAccommodationUseCase
    {
        public async Task ExecuteAsync(DeleteAccommodationUseCaseCommand command, CancellationToken ct = default)
        {
            var accommodation = await accommodations.GetByIdAsync(command.AccommodationId)
                ?? throw new AccommodationNotFoundException(command.AccommodationId);

            accommodation.Delete(command.UserId, time.GetUtcNow());

            await unitOfWork.SaveChangesAsync(ct);
        }
    }
}