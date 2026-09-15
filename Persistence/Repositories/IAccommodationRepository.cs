using Domain.Aggregates.Accommodations;

namespace Persistence.Repositories
{
    public interface IAccommodationRepository
    {
        Task<Accommodation?> GetByIdAsync(Guid id);
        Task<IEnumerable<Accommodation>> GetAllAsync();
        Task Add(Accommodation accommodation);
        Task SaveChangesAsync();
    }
}
