using BookMyHome.Domain.Aggregates.Users;
using BookMyHome.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BookMyHome.Persistence.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly BookMyHomeContext _context;
        public UserRepository(BookMyHomeContext context)
        {
            _context = context;
        }
        public async Task<User?> GetByIdAsync(Guid id)
        {
            return await _context.Users.FindAsync(id);
        }
        public void Add(User user)
        {
            _context.Users.Add(user);
        }
    }
}
