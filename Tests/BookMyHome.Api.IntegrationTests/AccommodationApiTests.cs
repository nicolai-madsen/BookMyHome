using System.Net;
using System.Net.Http.Json;
using BookMyHome.Persistence;

namespace BookMyHome.Api.IntegrationTests
{
    public class AccommodationApiTests(BookMyHomeApiFactory factory) : IClassFixture<BookMyHomeApiFactory>
    {
        private readonly BookMyHomeApiFactory _factory = factory;
        private static CancellationToken Ct => TestContext.Current.CancellationToken;

        private const string Url = "api/accommodations";

        private HttpClient Host => _factory.CreateClientAs(DbSeeder.Host1Id);
        private HttpClient Guest => _factory.CreateClientAs(DbSeeder.Guest1Id);
        private HttpClient Anonymous => _factory.CreateClient();

        private async Task<Guid> CreateAccommodationAsHostAsync()
        {
            var response = await Host.PostAsJsonAsync(Url, TestsData.Accommodation(), Ct);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            return Guid.Parse(response.Headers.Location!.Segments.Last());
        }

        [Fact]
        public async Task Create_AllIsOk_Returns201()
        {
            var response = await Host.PostAsJsonAsync(Url, TestsData.Accommodation(), Ct);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task Create_NotLoggedIn_Returns401()
        {
            var response = await Anonymous.PostAsJsonAsync(Url, TestsData.Accommodation(), Ct);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Create_ByUserWhoIsNotHost_Returns403()
        {
            var response = await Guest.PostAsJsonAsync(Url, TestsData.Accommodation(), Ct);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        // Gyldig token, men brugeren findes ikke i db
        [Fact]
        public async Task Create_UserDoesntExist_Returns404()
        {
            var unknownUser = _factory.CreateClientAs(Guid.NewGuid());

            var response = await unknownUser.PostAsJsonAsync(Url, TestsData.Accommodation(), Ct);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Create_PricePerDayIs0_Returns400()
        {
            var response = await Host.PostAsJsonAsync(Url, TestsData.Accommodation(pricePerDay: 0m), Ct);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Delete_WithUpcomingBooking_Returns409()
        {
            // Arrange
            var id = await CreateAccommodationAsHostAsync();
            (await Guest.PostAsJsonAsync($"{Url}/{id}/bookings", TestsData.Booking(), Ct)).EnsureSuccessStatusCode();

            // Act
            var response = await Host.DeleteAsync($"{Url}/{id}", Ct);

            // Assert
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact]
        public async Task Delete_ThenGet_Returns404()
        {
            var id = await CreateAccommodationAsHostAsync();

            var delete = await Host.DeleteAsync($"{Url}/{id}", Ct);
            var get = await Anonymous.GetAsync($"{Url}/{id}", Ct);   // GET er offentlig, så ingen login

            Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
        }

        // IDOR - en anden bruger prøver at slette hostens bolig
        [Fact]
        public async Task Delete_SomeoneElsesAccommodation_Returns403()
        {
            var id = await CreateAccommodationAsHostAsync();

            var response = await Guest.DeleteAsync($"{Url}/{id}", Ct);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }
}