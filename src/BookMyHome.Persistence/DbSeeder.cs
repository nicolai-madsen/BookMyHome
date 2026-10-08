using BookMyHome.Domain.Aggregates.Accommodations;
using BookMyHome.Domain.Aggregates.Users;
using BookMyHome.Domain.ValueObjects;

namespace BookMyHome.Persistence
{
    public class DbSeeder(BookMyHomeContext context)
    {
        // Easy IDs for PostMan stuff
        public static readonly Guid Host1Id = Guid.Parse("11111111-1111-1111-1111-111111111111");
        public static readonly Guid Host2Id = Guid.Parse("22222222-2222-2222-2222-222222222222");
        public static readonly Guid Guest1Id = Guid.Parse("33333333-3333-3333-3333-333333333333");
        public static readonly Guid Guest2Id = Guid.Parse("44444444-4444-4444-4444-444444444444");

        private const string PlaceholderHash = "not-a-real-hash"; // HUSKE!!! Opgave 11: rigtig hashing

        public async Task SeedAsync()
        {
            if (context.Users.Any()) return;

            var seededAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
            var today = new DateOnly(2026, 1, 1);

            // --- Users ---
            var host1 = new User(Host1Id, new FullName("Hanne", "Host"), new Email("hanne@bookmyhome.dk"),
                                 new PhoneNumber("+4511111111"), "hanne", PlaceholderHash);
            var host2 = new User(Host2Id, new FullName("Henrik", "Host"), new Email("henrik@bookmyhome.dk"),
                                 new PhoneNumber("+4522222222"), "henrik", PlaceholderHash);
            var guest1 = new User(Guest1Id, new FullName("Gitte", "Guest"), new Email("gitte@bookmyhome.dk"),
                                  new PhoneNumber("+4533333333"), "gitte", PlaceholderHash);
            var guest2 = new User(Guest2Id, new FullName("Gustav", "Guest"), new Email("gustav@bookmyhome.dk"),
                                  new PhoneNumber("+4544444444"), "gustav", PlaceholderHash);

            host1.BecomeHost(seededAt);
            host2.BecomeHost(seededAt);

            // Regler og vilkår gælder. That means testing data gets created properly through the domain
            var accommodation1 = new Accommodation(Guid.NewGuid(), Host1Id,
                new Address("Strandvejen", "1", "Vejle", "7100", "Denmark"),
                new DateRange(new DateOnly(2024, 1, 1), new DateOnly(2033, 1, 1)), 150m);

            accommodation1.CreateBooking(Guest1Id, new DateRange(new DateOnly(2026, 2, 1), new DateOnly(2026, 2, 5)), today);
            accommodation1.CreateBooking(Guest2Id, new DateRange(new DateOnly(2026, 2, 15), new DateOnly(2026, 2, 25)), today);
            accommodation1.CreateBooking(Guest1Id, new DateRange(new DateOnly(2026, 12, 20), new DateOnly(2026, 12, 27)), today);

            var accommodation2 = new Accommodation(Guid.NewGuid(), Host1Id,
                new Address("Fjordvej", "2", "Fredericia", "7000", "Denmark"),
                new DateRange(new DateOnly(2024, 12, 1), new DateOnly(2035, 12, 6)), 100m);

            accommodation2.CreateBooking(Guest2Id, new DateRange(new DateOnly(2026, 2, 28), new DateOnly(2026, 3, 15)), today);

            var accommodation3 = new Accommodation(Guid.NewGuid(), Host2Id,
                new Address("Skovstien", "3", "Kolding", "6000", "Denmark"),
                new DateRange(new DateOnly(2022, 1, 1), new DateOnly(2031, 1, 1)), 1500m);

            accommodation3.CreateBooking(Guest1Id, new DateRange(new DateOnly(2026, 4, 16), new DateOnly(2026, 4, 20)), today);

            context.Users.AddRange(host1, host2, guest1, guest2);
            context.Accommodations.AddRange(accommodation1, accommodation2, accommodation3);

            await context.SaveChangesAsync(); // Save it all at one time!
        }
    }
}