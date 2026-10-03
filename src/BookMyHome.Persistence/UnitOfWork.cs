using BookMyHome.Domain.Interfaces;
using BookMyHome.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace BookMyHome.Persistence
{
    public sealed class UnitOfWork(BookMyHomeContext context) : IUnitOfWork
    {
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new ConcurrencyConflictException("The resource was modified by another request.", ex); 
            }
        }
    }
}
