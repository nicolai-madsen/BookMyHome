using BookMyHome.Domain.Aggregates.Accommodations;
using BookMyHome.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BookMyHome.Persistence.Repositories
{
    public class AccommodationRepository : IAccommodationRepository
    {
        private readonly BookMyHomeContext _context;

        public AccommodationRepository(BookMyHomeContext context)
        {
            _context = context;
        }

        public async Task<Accommodation?> GetByIdAsync(Guid id)
        {
            return await _context.Accommodations
                .Include(a => a.Bookings)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<IEnumerable<Accommodation>> GetByHostIdAsync(Guid hostId)
        {
            return await _context.Accommodations
                .Where(a => a.HostId == hostId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Accommodation>> GetAllAsync()
        {
            return await _context.Accommodations
                .Include(a => a.Bookings)
                .ToListAsync();
        }

        public void Add(Accommodation accommodation)
        {
            _context.Accommodations.Add(accommodation);
        }
    }
}
