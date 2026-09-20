using Domain;

namespace Persistence
{
    public sealed class UnitOfWork(BookMyHomeContext context) : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => context.SaveChangesAsync(cancellationToken);
    }
}
