using Domain.Aggregates.Accommodations;
using Domain.ValueObjects;
using Persistence.Repositories;

namespace Tests
{
    [Collection("SqlServer")]
    public class PersistenceTests(SqlServerFixture db)
    {

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
            var dbName = $"test_{Guid.NewGuid():N}";

            var accommodationId = Guid.NewGuid();
            Guid bookingId;
            
            await using (var context = db.CreateContext(dbName))
            {
                var accommodation = CreateAccommodation(accommodationId);

                var today = new DateOnly(2026, 1, 1);
                var period = new DateRange(new DateOnly(2026, 9, 10), new DateOnly(2026, 9, 11));
                var booking = accommodation.CreateBooking(Guid.NewGuid(), period, today);

                bookingId = booking.Id;

                context.Add(accommodation);
                await context.SaveChangesAsync();

                
            }

            await using (var anotherOne = db.CreateContext(dbName))
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