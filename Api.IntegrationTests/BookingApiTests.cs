using Domain.Aggregates.Accommodations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Persistence;
using Persistence.Repositories;
using Shared.UseCaseDtos;
using System.Net.Http.Json;

namespace Api.IntegrationTests
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
                    new Domain.ValueObjects.Address("Street", "44", "City", "1919", "Country"),
                    new Domain.ValueObjects.DateRange(accommodationAvailableStart, accommodationAvailableEnd),
                    pricePerDay: 100m);
                context.Accommodations.Add(accommodation);
                await context.SaveChangesAsync();
            }
            return accommodationId; 
        }

        [Fact]
        public async Task CreateBooking_OverlappingDates_Returns409WithProblemDetails()
        {
            // Arrange
            var accommodationId = await SeedAccommodationAsync();

            var bookingStart = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
            var bookingEnd = DateOnly.FromDateTime(DateTime.Now.AddDays(32));

            // Act 1| POST a booking, expected: 201
            var request = new CreateBookingRequest(Guid.NewGuid(), bookingStart, bookingEnd);
            var response = await _client.PostAsJsonAsync($"api/accommodations/{accommodationId}/bookings", request);
            Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);

            // Act 2| POST a booking with overlapping dates
            var overlappingRequest = new CreateBookingRequest(Guid.NewGuid(), bookingStart, bookingEnd);
            var overlappingResponse = await _client.PostAsJsonAsync($"api/accommodations/{accommodationId}/bookings", overlappingRequest);

            // Assert Salad:
            // Assert 409, and body type is ProblemDetails
            Assert.Equal(System.Net.HttpStatusCode.Conflict, overlappingResponse.StatusCode);
            Assert.Equal("application/problem+json", overlappingResponse.Content.Headers.ContentType?.MediaType);

            // Assert the response body contains ProblemDetails with the expected title and status
            var problem = await overlappingResponse.Content.ReadFromJsonAsync<Microsoft.AspNetCore.Mvc.ProblemDetails>();
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

            // Act 1 POST en reschedule på en booking der ikke findes
            var rescheduleRequest = new RescheduleBookingRequest(DateOnly.FromDateTime(DateTime.Now.AddDays(10)), DateOnly.FromDateTime(DateTime.Now.AddDays(12)));
            var rescheduleResponse = await _client.PostAsJsonAsync($"api/accommodations/{accommodationId}/bookings/{Guid.NewGuid()}/reschedule", rescheduleRequest);            

            // Assert 404, og bodyen er ProblemDetails
            Assert.Equal(System.Net.HttpStatusCode.NotFound, rescheduleResponse.StatusCode);
            Assert.Equal("application/problem+json", rescheduleResponse.Content.Headers.ContentType?.MediaType);
            var problem = await rescheduleResponse.Content.ReadFromJsonAsync<Microsoft.AspNetCore.Mvc.ProblemDetails>();
            Assert.Equal("Booking not found", problem.Title);
            Assert.NotNull(problem);
            Assert.Equal(404, problem.Status);
        }

        [Fact]
        public async Task RescheduleBooking_EndDateBeforeStartDate_Returns400WithProblemDetails()
        {
            // Arrange create accommodation
            var accommodationId = await SeedAccommodationAsync();

            // Act 1
            // POST a reschedule with endDate < startDate
            var rescheduleRequest = new RescheduleBookingRequest(DateOnly.FromDateTime(DateTime.Now.AddDays(12)), DateOnly.FromDateTime(DateTime.Now.AddDays(10)));
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

            var bookingStart = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
            var bookingEnd = DateOnly.FromDateTime(DateTime.Now.AddDays(32));
            var tasks = new List<Task<HttpResponseMessage>>();

            // Act
            // Send 10 concurrent requests to create overlapping bookings
            for (int i = 0; i < 10; i++)
            {
                var request = new CreateBookingRequest(Guid.NewGuid(), bookingStart, bookingEnd);
                tasks.Add(_client.PostAsJsonAsync($"api/accommodations/{accommodationId}/bookings", request));
            }
            var responses = await Task.WhenAll(tasks);

            // Assert: Only one request should succeed (201 Created), others should fail with 409 Conflict
            int successCount = responses.Count(r => r.StatusCode == System.Net.HttpStatusCode.Created);
            int conflictCount = responses.Count(r => r.StatusCode == System.Net.HttpStatusCode.Conflict);
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
    }
}
