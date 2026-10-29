using BookMyHome.Shared.UseCaseDtos.Accommodations;

namespace BookMyHome.Api.IntegrationTests
{
    public static class TestsData
    {
        public static readonly DateOnly Today = new(2026, 11, 1);
        public static CreateBookingRequest Booking(int startInDays = 30, int nights = 3)
        {
            var s = Today.AddDays(startInDays);
            return new CreateBookingRequest(s, s.AddDays(nights));
        }

        public static CreateAccommodationRequest Accommodation(decimal pricePerDay = 500m) =>
            new("Testvej", "1", "Vejle", "7100", "Danmark",
               Today, Today.AddYears(1), pricePerDay);
    }
}
