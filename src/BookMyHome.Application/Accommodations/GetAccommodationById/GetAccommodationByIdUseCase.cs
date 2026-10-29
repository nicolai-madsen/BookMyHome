using BookMyHome.Domain.Aggregates.Accommodations;
using BookMyHome.Domain.Exceptions;
using BookMyHome.Domain.Interfaces.Repositories;

namespace BookMyHome.Application.Accommodations.GetAccommodationById
{
    public class GetAccommodationByIdUseCase : IGetAccommodationByIdUseCase
    {
        private readonly IAccommodationRepository _repository;
        public GetAccommodationByIdUseCase(IAccommodationRepository repository)
        {
            _repository = repository;
        }
        public async Task<Accommodation> ExecuteAsync(GetAccommodationByIdQuery query, CancellationToken ct)
        {
            return await _repository.GetByIdAsync(query.AccommodationId) ?? throw new AccommodationNotFoundException(query.AccommodationId);
        }
    }
}
