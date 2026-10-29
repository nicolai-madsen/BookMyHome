using BookMyHome.Domain.Aggregates.Accommodations;

namespace BookMyHome.Application.Accommodations.GetAllAccommodations
{
    public interface IGetAllAccommodationsUseCase
    {
        Task<IReadOnlyCollection<Accommodation>> ExecuteAsync(GetAllAccommodationsQuery query, CancellationToken ct); 
    }
}
