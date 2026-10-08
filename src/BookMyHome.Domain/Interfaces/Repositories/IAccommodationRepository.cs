using BookMyHome.Domain.Aggregates.Accommodations;

namespace BookMyHome.Domain.Interfaces.Repositories
{
    public interface IAccommodationRepository
    {
        Task<Accommodation?> GetByIdAsync(Guid id);
        Task<IEnumerable<Accommodation>> GetAllAsync();
        Task<IEnumerable<Accommodation>> GetByHostIdAsync(Guid hostId);
        void Add(Accommodation accommodation);
    }
}
