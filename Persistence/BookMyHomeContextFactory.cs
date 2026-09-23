using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Persistence
{
    public class BookMyHomeContextFactory : IDesignTimeDbContextFactory<BookMyHomeContext>
    {
        public BookMyHomeContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<BookMyHomeContext>();
            optionsBuilder.UseSqlServer(
                @"Server=(localdb)\mssqllocaldb;Database=BookMyHome;Trusted_Connection=True;TrustServerCertificate=True;");

            return new BookMyHomeContext(optionsBuilder.Options);
        }
    }
}