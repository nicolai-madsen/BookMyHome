using Domain.Aggregates.Accommodations;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Tests
{
    public class PersistenceTests
    {
        private static BookMyHomeContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<BookMyHomeContext>()
                .UseSqlServer($@"Server=(localdb)\mssqllocaldb;Database={dbName};Trusted_Connection=True;TrustServerCertificate=True;")
                .Options;
            return new BookMyHomeContext(options);
        }

        [Fact]
        public async Task GetAccommodation_WithoutInclude_BookingsAreEmpty()
        {
            // Arrange
            var dbName = "BookMyHome_WithoutInclude";
            var accommodationId = Guid.NewGuid();

            using (var context = CreateContext(dbName))
            {
                await context.Database.EnsureDeletedAsync();   // clean up
                await context.Database.EnsureCreatedAsync();

                var accommodation = new Accommodation(accommodationId, Guid.NewGuid(), new Address("Vej1", "2", "By1", "1111", "Country1"),  new DateRange(new DateOnly(2026, 1, 1), new DateOnly(2027, 1, 1)), 1000m);
                accommodation.AddBooking(Guid.NewGuid(), new DateRange(new DateOnly(2026, 5, 10), new DateOnly(2026, 5, 15)), new DateOnly(2026, 1, 1));
                context.Accommodations.Add(accommodation);
                await context.SaveChangesAsync();
            }

            // Act, but with a new context and a new change tracker
            using (var context = CreateContext(dbName))
            {
                var loaded = await context.Accommodations
                    .FirstAsync(a => a.Id == accommodationId);

                // Assert
                Assert.Empty(loaded.Bookings);
            }
        }

        [Fact]
        public async Task GetAccommodation_WithInclude_BookingsLoadSuccessfully()
        {
            // Arrange
            var dbName = "BookMyHome_WithInclude";
            var accommodationId = Guid.NewGuid();

            using (var context = CreateContext(dbName))
            {
                await context.Database.EnsureDeletedAsync();   // clean up
                await context.Database.EnsureCreatedAsync();

                var accommodation = new Accommodation(accommodationId, Guid.NewGuid(), new Address("Vej1", "2", "By1", "1111", "Country1"), new DateRange(new DateOnly(2026, 1, 1), new DateOnly(2027, 1, 1)), 1000m);
                accommodation.AddBooking(Guid.NewGuid(), new DateRange(new DateOnly(2026, 5, 10), new DateOnly(2026, 5, 15)), new DateOnly(2026, 1, 1));
                context.Accommodations.Add(accommodation);
                await context.SaveChangesAsync();
            }

            // Act, but with a new context and a new change tracker
            using (var context = CreateContext(dbName))
            {
                var loaded = await context.Accommodations
                    .Include(a => a.Bookings)
                    .FirstAsync(a => a.Id == accommodationId);

                // Assert
                Assert.Single(loaded.Bookings);
            }
        }
    }
}