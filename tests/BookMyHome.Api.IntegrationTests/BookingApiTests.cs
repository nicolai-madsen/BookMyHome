using System.Net;
using System.Net.Http.Json;
using BookMyHome.Domain.Aggregates.Accommodations;
using BookMyHome.Domain.Aggregates.Users;
using BookMyHome.Domain.ValueObjects;
using BookMyHome.Persistence;
using BookMyHome.Persistence.Repositories;
using BookMyHome.Shared.UseCaseDtos.Accommodations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace BookMyHome.Api.IntegrationTests
{
    public class BookingApiTests(BookMyHomeApiFactory factory) : IClassFixture<BookMyHomeApiFactory>
    {
        private readonly BookMyHomeApiFactory _factory = factory;
        private static CancellationToken Ct => TestContext.Current.CancellationToken;

        private static string BookingsUrl(Guid accommodationId) => $"api/accommodations/{accommodationId}/bookings";

        private async Task<Guid> SeedAccommodationAsync()
        {
            var accommodationId = Guid.NewGuid();

            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<BookMyHomeContext>();

            context.Accommodations.Add(new Accommodation(
                accommodationId,
                Guid.NewGuid(),
                new Address("Street", "44", "City", "1919", "Country"),
                new DateRange(TestsData.Today, TestsData.Today.AddDays(365)),
                pricePerDay: 100m));

            await context.SaveChangesAsync(Ct);
            return accommodationId;
        }

        private async Task<Guid> SeedUserAsync()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<BookMyHomeContext>();

            var user = new User(
                Guid.NewGuid(),
                new FullName("TestDude", "Testsen"),
                new Email($"test-{Guid.NewGuid()}@test.dk"),
                new PhoneNumber("123456789"),
                $"username{Guid.NewGuid()}",
                "secretPassword");

            db.Users.Add(user);
            await db.SaveChangesAsync(Ct);
            return user.Id;
        }

        private async Task<HttpClient> SeedGuestClientAsync() =>
            _factory.CreateClientAs(await SeedUserAsync());

        [Fact]
        public async Task CreateBooking_NotLoggedIn_Returns401()
        {
            var accommodationId = await SeedAccommodationAsync();
            var anonymous = _factory.CreateClient();

            var response = await anonymous.PostAsJsonAsync(BookingsUrl(accommodationId), TestsData.Booking(), Ct);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task CreateBooking_OverlappingDates_Returns409WithProblemDetails()
        {
            // Arrange
            var accommodationId = await SeedAccommodationAsync();
            var guest = await SeedGuestClientAsync();

            var first = await guest.PostAsJsonAsync(BookingsUrl(accommodationId), TestsData.Booking(), Ct);
            Assert.Equal(HttpStatusCode.Created, first.StatusCode);

            // Act
            var response = await guest.PostAsJsonAsync(BookingsUrl(accommodationId), TestsData.Booking(), Ct);

            // Assert
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
            Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

            var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(Ct);
            Assert.NotNull(problem);
            Assert.Equal("Booking conflict", problem.Title);
            Assert.Equal(409, problem.Status);
        }


        [Fact]
        public async Task RescheduleBooking_UnknownBooking_Returns404WithProblemDetails()
        {
            // Arrange
            var accommodationId = await SeedAccommodationAsync();
            var guest = await SeedGuestClientAsync();

            // Act
            var response = await guest.PostAsJsonAsync(
                $"{BookingsUrl(accommodationId)}/{Guid.NewGuid()}/reschedule", TestsData.Reschedule(), Ct);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

            var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(Ct);
            Assert.NotNull(problem);
            Assert.Equal("Booking not found", problem.Title);
            Assert.Equal(404, problem.Status);
        }

        [Fact]
        public async Task RescheduleBooking_EndDateBeforeStartDate_Returns400WithProblemDetails()
        {
            // Arrange
            var accommodationId = await SeedAccommodationAsync();
            var guest = await SeedGuestClientAsync();
            var invalid = new RescheduleBookingRequest(TestsData.Today.AddDays(12), TestsData.Today.AddDays(10));

            // Act
            var response = await guest.PostAsJsonAsync(
                $"{BookingsUrl(accommodationId)}/{Guid.NewGuid()}/reschedule", invalid, Ct);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

            var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(Ct);
            Assert.NotNull(problem);
            Assert.Equal("Invalid booking", problem.Title);
            Assert.Equal(400, problem.Status);
        }

        [Fact]
        public async Task CreateBooking_ConcurrencyOverlappingRequests_OnlyOneSucceeds()
        {
            // Arrange
            var accommodationId = await SeedAccommodationAsync();
            var guest = await SeedGuestClientAsync();

            // Act 10 requests på samme tid, for præcis de samme datoer
            var tasks = Enumerable.Range(0, 10)
                .Select(_ => guest.PostAsJsonAsync(BookingsUrl(accommodationId), TestsData.Booking(), Ct));
            var responses = await Task.WhenAll(tasks);

            // Assert
            Assert.Equal(1, responses.Count(r => r.StatusCode == HttpStatusCode.Created));
            Assert.Equal(9, responses.Count(r => r.StatusCode == HttpStatusCode.Conflict));
            await AssertExactlyOneBookingInDatabaseAsync(accommodationId);
        }

        // Sekventiel tvilling af testen ovenfor - requests ikke på samme tid, så den isolerer domænets overlap-tjek.
        // Består denne, men den parallelle fejler, så ligger fejlen i concurrency-håndteringen, ikke i domænet.
        [Fact]
        public async Task CreateBooking_OverlappingRequestsOneByOne_OnlyOneSucceeds()
        {
            // Arrange
            var accommodationId = await SeedAccommodationAsync();
            var guest = await SeedGuestClientAsync();
            var responses = new List<HttpResponseMessage>();

            // Act
            for (var i = 0; i < 10; i++)
                responses.Add(await guest.PostAsJsonAsync(BookingsUrl(accommodationId), TestsData.Booking(), Ct));

            // Assert
            Assert.Equal(1, responses.Count(r => r.StatusCode == HttpStatusCode.Created));
            Assert.Equal(9, responses.Count(r => r.StatusCode == HttpStatusCode.Conflict));
            await AssertExactlyOneBookingInDatabaseAsync(accommodationId);
        }

        // Nyt scope, så change trackeren ikke genbruger entiteter fra testens tidligere scopes
        private async Task AssertExactlyOneBookingInDatabaseAsync(Guid accommodationId)
        {
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<BookMyHomeContext>();
            var accommodation = await new AccommodationRepository(context).GetByIdAsync(accommodationId);

            Assert.NotNull(accommodation);
            Assert.Single(accommodation.Bookings);
        }

        // IDOR TEST

        [Fact]
        public async Task CancelBooking_AnotherUsersBooking_Returns403WithProblemDetails()
        {
            // Arrange - gæst A booker, og en anden bruger prøver at aflyse
            var accommodationId = await SeedAccommodationAsync();
            var owner = await SeedGuestClientAsync();
            var stranger = await SeedGuestClientAsync();

            var created = await owner.PostAsJsonAsync(BookingsUrl(accommodationId), TestsData.Booking(), Ct);
            Assert.Equal(HttpStatusCode.Created, created.StatusCode);
            var bookingUrl = created.Headers.Location!;

            // Act
            var response = await stranger.PostAsync($"{bookingUrl}/cancel", null, Ct);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

            var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(Ct);
            Assert.NotNull(problem);
            Assert.Equal("Forbidden", problem.Title);
            Assert.Equal(403, problem.Status);
        }
    }
}