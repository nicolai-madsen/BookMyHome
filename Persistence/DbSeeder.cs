using Domain.Aggregates.Accommodations;
using Domain.ValueObjects;

namespace Persistence
{
    public class DbSeeder
    {
        private readonly BookMyHomeContext _context;

        public DbSeeder(BookMyHomeContext context)
        {
            _context = context;
        }

        public async Task SeedAsync()
        {
            await SeedAccommodationsAsync();
        }



        private async Task SeedAccommodationsAsync()
        {
            if (_context.Accommodations.Any()) return;

            var today = new DateOnly(2026, 9, 10);

            var accommodation1 = new Accommodation(
                Guid.NewGuid(),
                Guid.NewGuid(),
                new Address("Vejnavn1", "1", "By1", "1111", "country1"),
                new DateRange(new DateOnly(2024, 1, 1), new DateOnly(2033, 1, 1)),
                150m);

            accommodation1.AddBooking(
                Guid.NewGuid(),
                new DateRange(new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 1)),
                today);

            accommodation1.AddBooking(
                Guid.NewGuid(),
                new DateRange(new DateOnly(2026, 2, 1), new DateOnly(2026, 2, 5)),
                today);

            accommodation1.AddBooking(
                Guid.NewGuid(),
                new DateRange(new DateOnly(2026, 2, 15), new DateOnly(2026, 2, 25)),
                today);



            var accommodation2 = new Accommodation(
                Guid.NewGuid(),
                Guid.NewGuid(),
                new Address("Vejnavn2", "2", "By2", "2222", "country2"),
                new DateRange(new DateOnly(2024, 12, 1), new DateOnly(2035, 12, 6)),
                100m);

            accommodation2.AddBooking(
               Guid.NewGuid(),
               new DateRange(new DateOnly(2026, 8, 15), new DateOnly(2026, 8, 22)),
               today);

            accommodation2.AddBooking(
                Guid.NewGuid(),
                new DateRange(new DateOnly(2026, 2, 28), new DateOnly(2026, 3, 15)),
                today);

            accommodation2.AddBooking(
                Guid.NewGuid(),
                new DateRange(new DateOnly(2026, 2, 10), new DateOnly(2026, 2, 13)),
                today);


            var accommodation3 = new Accommodation(
                Guid.NewGuid(),
                Guid.NewGuid(),
                new Address("Vejnavn3", "3", "By3", "3333", "country3"),
                new DateRange(new DateOnly(2022, 1, 1), new DateOnly(2031, 1, 1)),
                1500m);

            accommodation3.AddBooking(
               Guid.NewGuid(),
               new DateRange(new DateOnly(2026, 3, 11), new DateOnly(2026, 3, 15)),
               today);

            accommodation3.AddBooking(
                Guid.NewGuid(),
                new DateRange(new DateOnly(2026, 4, 16), new DateOnly(2026, 4, 20)),
                today);

            accommodation3.AddBooking(
                Guid.NewGuid(),
                new DateRange(new DateOnly(2026, 9, 10), new DateOnly(2026, 9, 16)),
                today);

            _context.Accommodations.Add(accommodation1);
            _context.Accommodations.Add(accommodation2);
            _context.Accommodations.Add(accommodation3);

            await _context.SaveChangesAsync();
        }
    }
}
