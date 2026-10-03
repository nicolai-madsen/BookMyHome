using BookMyHome.Domain.Aggregates.Accommodations;

namespace BookMyHome.Domain.Interfaces.Repositories
{
    public interface IAccommodationRepository
    {
        Task<Accommodation?> GetByIdAsync(Guid id);
        Task<IEnumerable<Accommodation>> GetAllAsync();
        void Add(Accommodation accommodation);
    }
}
