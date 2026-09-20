using Domain.Aggregates.Accommodations;

namespace Domain.Repositories
{
    public interface IAccommodationRepository
    {
        Task<Accommodation?> GetByIdAsync(Guid id);
        Task<IEnumerable<Accommodation>> GetAllAsync();
        void Add(Accommodation accommodation);
    }
}
