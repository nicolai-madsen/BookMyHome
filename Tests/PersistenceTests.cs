using Domain.Aggregates.Accommodations;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Persistence.Repositories;

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

        private static Accommodation CreateAccommodation(Guid id, decimal pricePerDay = 100m) =>
        new Accommodation(
            id,
            Guid.NewGuid(),
            new Address("TestStreet", "43", "Testcity", "1919", "Jugoslavia"),
            new DateRange(new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31)),
            pricePerDay);

        [Fact]
        public async Task GetByIdAsync_ReturnsAccommodationWithItsBookings()
        {
            var accommodationId = Guid.NewGuid();
            Guid bookingId;
            
            using (var context = CreateContext("testDb"))
            {
                await context.Database.EnsureDeletedAsync();
                await context.Database.EnsureCreatedAsync();

                var accommodation = CreateAccommodation(accommodationId);

                var today = new DateOnly(2026, 1, 1);
                var period = new DateRange(new DateOnly(2026, 9, 10), new DateOnly(2026, 9, 11));
                var booking = accommodation.AddBooking(Guid.NewGuid(), period, today);

                bookingId = booking.Id;

                context.Add(accommodation);
                await context.SaveChangesAsync();

                
            }

            using (var anotherOne = CreateContext("testDb"))
            {
                var repository = new AccommodationRepository(anotherOne);

                var accommodation = await repository.GetByIdAsync(accommodationId);

                var booking = accommodation?.Bookings.FirstOrDefault(b => b.Id == bookingId);

                Assert.NotNull(booking);
                var loadedBooking = Assert.Single(accommodation.Bookings);
                Assert.Equal(bookingId, loadedBooking.Id);
            }


        }
    }
}