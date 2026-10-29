using BookMyHome.Domain.Aggregates.Accommodations;
using BookMyHome.Domain.Interfaces.Repositories;

namespace BookMyHome.Application.Accommodations.GetAllAccommodations
{
    public class GetAllAccommodationsUseCase : IGetAllAccommodationsUseCase
    {
        private readonly IAccommodationRepository _accommodations;
        public GetAllAccommodationsUseCase(IAccommodationRepository accommodations)
        {
            _accommodations = accommodations;
        }

        public async Task<IReadOnlyCollection<Accommodation>> ExecuteAsync(GetAllAccommodationsQuery query, CancellationToken ct) =>
            query.HostId is { } hostId
                ? await _accommodations.GetByHostIdAsync(hostId)
                : await _accommodations.GetAllAsync();

    }
}
