using BookMyHome.Domain.Aggregates.Accommodations;

namespace BookMyHome.Application.Accommodations.GetAccommodationById
{
    public interface IGetAccommodationByIdUseCase
    {
        Task<Accommodation> ExecuteAsync(GetAccommodationByIdQuery query, CancellationToken ct);
    }
}
