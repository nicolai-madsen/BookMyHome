using BookMyHome.Domain.Aggregates.Accommodations;

namespace BookMyHome.Domain.Interfaces.Repositories
{
    public interface IAccommodationRepository
    {
        Task<Accommodation?> GetByIdAsync(Guid id);
        Task<IReadOnlyCollection<Accommodation>> GetAllAsync();
        Task<IReadOnlyCollection<Accommodation>> GetByHostIdAsync(Guid hostId);
        void Add(Accommodation accommodation);
    }
}
