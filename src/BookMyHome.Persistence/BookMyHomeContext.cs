using BookMyHome.Domain.Aggregates.Accommodations;
using BookMyHome.Domain.Aggregates.Users;
using Microsoft.EntityFrameworkCore;

namespace BookMyHome.Persistence
{
    public class BookMyHomeContext : DbContext
    {
        public DbSet<Accommodation> Accommodations { get; set; }
        public DbSet<User> Users { get; set; }

        public BookMyHomeContext(DbContextOptions<BookMyHomeContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(BookMyHomeContext).Assembly);
        }
    }
}
