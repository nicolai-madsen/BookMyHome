using System.Net;
using System.Net.Http.Json;
using BookMyHome.Domain.Aggregates.Accommodations;
using BookMyHome.Domain.Aggregates.Users;
using BookMyHome.Domain.ValueObjects;
using BookMyHome.Persistence;
using BookMyHome.Persistence.Repositories;
using BookMyHome.Shared.DomainDtos;
using BookMyHome.Shared.UseCaseDtos.Accommodations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;


namespace BookMyHome.Api.IntegrationTests
{
    public class BookingApiTests(BookMyHomeApiFactory factory) : IClassFixture<BookMyHomeApiFactory>
    {
        private readonly HttpClient _client = factory.CreateClient();

        private async Task<Guid> SeedAccommodationAsync() // Helper method to seed an accommodation
        {            
            var accommodationAvailableStart = DateOnly.FromDateTime(DateTime.Now.AddDays(1));
            var accommodationAvailableEnd = DateOnly.FromDateTime(DateTime.Now.AddDays(365));
            
            Guid accommodationId = Guid.NewGuid();

            using (var scope = factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<BookMyHomeContext>();

                var accommodation = new Accommodation(
                    accommodationId,
                    Guid.NewGuid(),
                    new Address("Street", "44", "City", "1919", "Country"),
                    new DateRange(accommodationAvailableStart, accommodationAvailableEnd),
                    pricePerDay: 100m);
                context.Accommodations.Add(accommodation);
                await context.SaveChangesAsync();
            }
            return accommodationId; 
        }

        private async Task<Guid> SeedUserAsync()
        {
            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<BookMyHomeContext>();
            var email = $"test-{Guid.NewGuid()}@test.dk";
            var username = $"username{Guid.NewGuid()}";

            var user = new User(Guid.NewGuid(), new FullName("TestDude", "Testsen"), new Email(email), new PhoneNumber("123456789"), username, "secretPassword");
            db.Users.Add(user);
            await db.SaveChangesAsync();

            return user.Id;
        }

        [Fact]
        public async Task CreateBooking_OverlappingDates_Returns409WithProblemDetails()
        {
            // Arrange
            var accommodationId = await SeedAccommodationAsync();
            var guestId = await SeedUserAsync();

            var bookingStart = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
            var bookingEnd = DateOnly.FromDateTime(DateTime.Now.AddDays(32));

            // Act 1| POST a booking, expected: 201
            var request = new CreateBookingRequest(guestId, bookingStart, bookingEnd);
            var response = await _client.PostAsJsonAsync($"api/accommodations/{accommodationId}/bookings", request);
            Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);

            // Act 2| POST a booking with overlapping dates
            var overlappingRequest = new CreateBookingRequest(guestId, bookingStart, bookingEnd);
            var overlappingResponse = await _client.PostAsJsonAsync($"api/accommodations/{accommodationId}/bookings", overlappingRequest);

            // "Assert Salad":
            // Assert 409, and body type is ProblemDetails
            Assert.Equal(HttpStatusCode.Conflict, overlappingResponse.StatusCode);
            Assert.Equal("application/problem+json", overlappingResponse.Content.Headers.ContentType?.MediaType);

            // Assert the response body contains ProblemDetails with the expected title and status
            var problem = await overlappingResponse.Content.ReadFromJsonAsync<ProblemDetails>();
            // Assert that the problem details are not null and have the expected title and status
            Assert.Equal("Booking conflict", problem.Title);
            // Assert that the problem details are not null and have the expected title and status
            Assert.NotNull(problem);
            // Assert that the problem details have the expected status code
            Assert.Equal(409, problem.Status);
        }

        [Fact]
        public async Task RescheduleBooking_UnknownBooking_Returns404WithProblemDetails()
        {
            // Arrange lav accommodation
            var accommodationId = await SeedAccommodationAsync();
            var guestId = await SeedUserAsync();

            // Act 1 POST en reschedule på en booking der ikke findes
            var rescheduleRequest = new RescheduleBookingRequest(guestId, DateOnly.FromDateTime(DateTime.Now.AddDays(10)), DateOnly.FromDateTime(DateTime.Now.AddDays(12)));
            var rescheduleResponse = await _client.PostAsJsonAsync($"api/accommodations/{accommodationId}/bookings/{Guid.NewGuid()}/reschedule", rescheduleRequest);            

            // Assert 404, og bodyen er ProblemDetails
            Assert.Equal(System.Net.HttpStatusCode.NotFound, rescheduleResponse.StatusCode);
            Assert.Equal("application/problem+json", rescheduleResponse.Content.Headers.ContentType?.MediaType);
            var problem = await rescheduleResponse.Content.ReadFromJsonAsync<ProblemDetails>();
            Assert.Equal("Booking not found", problem.Title);
            Assert.NotNull(problem);
            Assert.Equal(404, problem.Status);
        }

        [Fact]
        public async Task RescheduleBooking_EndDateBeforeStartDate_Returns400WithProblemDetails()
        {
            // Arrange create accommodation
            var accommodationId = await SeedAccommodationAsync();
            var guestId = await SeedUserAsync();

            // Act 1
            // POST a reschedule with endDate < startDate
            var rescheduleRequest = new RescheduleBookingRequest(guestId, DateOnly.FromDateTime(DateTime.Now.AddDays(12)), DateOnly.FromDateTime(DateTime.Now.AddDays(10)));
            var rescheduleResponse = await _client.PostAsJsonAsync($"api/accommodations/{accommodationId}/bookings/{Guid.NewGuid()}/reschedule", rescheduleRequest);

            // Assert 400, and body type is ProblemDetails
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, rescheduleResponse.StatusCode);
            Assert.Equal("application/problem+json", rescheduleResponse.Content.Headers.ContentType?.MediaType);
            var problem = await rescheduleResponse.Content.ReadFromJsonAsync<Microsoft.AspNetCore.Mvc.ProblemDetails>();     
            Assert.NotNull(problem);
            Assert.Equal("Invalid booking", problem.Title);
            Assert.Equal(400, problem.Status);
        }

        [Fact]
        public async Task CreateBooking_ConcurrencyOverlappingRequests_OnlyOneSucceeds()
        {
            // Arrange
            var accommodationId = await SeedAccommodationAsync();
            var guestId = await SeedUserAsync();

            var bookingStart = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
            var bookingEnd = DateOnly.FromDateTime(DateTime.Now.AddDays(32));
            var tasks = new List<Task<HttpResponseMessage>>();

            // Act
            // Send 10 concurrent requests to create overlapping bookings
            for (int i = 0; i < 10; i++)
            {
                var request = new CreateBookingRequest(guestId, bookingStart, bookingEnd);
                tasks.Add(_client.PostAsJsonAsync($"api/accommodations/{accommodationId}/bookings", request));
            }
            var responses = await Task.WhenAll(tasks);


            // Assert: Only one request should succeed (201 Created), others should fail with 409 Conflict
            int successCount = responses.Count(r => r.StatusCode == HttpStatusCode.Created);
            int conflictCount = responses.Count(r => r.StatusCode == HttpStatusCode.Conflict);
            Assert.Equal(1, successCount);
            Assert.Equal(9, conflictCount);

            // Assert that the booking was actually created in the database with a new scope,
            // so the change tracker is not tracking the entities from the previous scope
            using (var scope = factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<BookMyHomeContext>();
                var accommodation = await new AccommodationRepository(context).GetByIdAsync(accommodationId);
                Assert.NotNull(accommodation);
                Assert.Single(accommodation.Bookings);
            }
        }

        // Sequential twin of above test: CreateBooking_ConcurrencyOverlappingRequests_OnlyOneSucceeds.
        // No concurrency here, so this isolates the domain's overlap check.
        // If this passes but the parallel test fails, then the bug has to be in concurrency handling, and not domain.
        [Fact]
        public async Task CreateBooking_ConcurrencyOverlappingRequestsOneByOne_OnlyOneSucceeds()
        {
            // Arrange
            var accommodationId = await SeedAccommodationAsync();
            var guestId = await SeedUserAsync();

            var bookingStart = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
            var bookingEnd = DateOnly.FromDateTime(DateTime.Now.AddDays(32));

            var responses = new List<HttpResponseMessage>();

            // Act
            
            // Send 10 requests 1 by 1
            for (int i = 0; i < 10; i++)
            {
                var request = new CreateBookingRequest(guestId, bookingStart, bookingEnd);
                responses.Add(await _client.PostAsJsonAsync($"api/accommodations/{accommodationId}/bookings", request));
            }

            // Assert: Only one request should succeed (201 Created), others should fail with 409 Conflict
            int successCount = responses.Count(r => r.StatusCode == HttpStatusCode.Created);
            int conflictCount = responses.Count(r => r.StatusCode == HttpStatusCode.Conflict);
            Assert.Equal(1, successCount);
            Assert.Equal(9, conflictCount);
     
            using (var scope = factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<BookMyHomeContext>();
                var accommodation = await new AccommodationRepository(context).GetByIdAsync(accommodationId);
                Assert.NotNull(accommodation);
                Assert.Single(accommodation.Bookings);
            }
        }

        [Fact]
        public async Task CancelBooking_UserIsNeitherHostNorGuest_Returns403WithProblemDetails()
        {
            // Arrange
            var accommodationId = await SeedAccommodationAsync();
            var guestId = await SeedUserAsync();
            var unauthorizedUser = Guid.NewGuid();

            var bookingStart = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
            var bookingEnd = DateOnly.FromDateTime(DateTime.Now.AddDays(32));

            var createResponse = await _client.PostAsJsonAsync(
                $"api/accommodations/{accommodationId}/bookings",
                new CreateBookingRequest(guestId, bookingStart, bookingEnd));

            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

            var booking = await createResponse.Content.ReadFromJsonAsync<BookingDto>();

            // Act
            var cancelResponse = await _client.PostAsJsonAsync(
                $"api/accommodations/{accommodationId}/bookings/{booking!.Id}/cancel",
                new CancelBookingRequest(unauthorizedUser));

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, cancelResponse.StatusCode);
            Assert.Equal("application/problem+json", cancelResponse.Content.Headers.ContentType?.MediaType);

            var problem = await cancelResponse.Content.ReadFromJsonAsync<ProblemDetails>();
            Assert.NotNull(problem);
            Assert.Equal("Forbidden", problem.Title);
            Assert.Equal(403, problem.Status);
        }
    }
}
