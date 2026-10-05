using BookMyHome.Domain.Aggregates.Users;

namespace BookMyHome.Domain.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id);
        void Add(User user);
    }
}
