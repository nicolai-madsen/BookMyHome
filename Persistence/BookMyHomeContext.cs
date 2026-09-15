using System.Security;
using Domain.Aggregates.Accommodations;
using Microsoft.EntityFrameworkCore;

namespace Persistence
{
    public class BookMyHomeContext : DbContext
    {
        public DbSet<Accommodation> Accommodations { get; set; }

        public BookMyHomeContext(DbContextOptions<BookMyHomeContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(BookMyHomeContext).Assembly);
        }
    }
}
