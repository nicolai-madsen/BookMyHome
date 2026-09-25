using Domain.Aggregates.Accommodations;
using Microsoft.Extensions.DependencyInjection;
using Persistence;
using Shared.UseCaseDtos;
using System.Net.Http.Json;

namespace Api.IntegrationTests
{
    public class BookingApiTests(BookMyHomeApiFactory factory) : IClassFixture<BookMyHomeApiFactory>
    {
        private readonly HttpClient _client = factory.CreateClient();

        private async Task<Guid> SeedAccommodationAsync()
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

            // Act 1 POST en booking, forvent 201
            var request = new CreateBookingRequest(Guid.NewGuid(), bookingStart, bookingEnd);
            var response = await _client.PostAsJsonAsync($"api/accommodations/{accommodationId}/bookings", request);
            Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);

            // Act 2 POST en booking på samme datoer
            var overlappingRequest = new CreateBookingRequest(Guid.NewGuid(), bookingStart, bookingEnd);
            var overlappingResponse = await _client.PostAsJsonAsync($"api/accommodations/{accommodationId}/bookings", overlappingRequest);

            // Assert 409, og bodyen er ProblemDetails
            Assert.Equal(System.Net.HttpStatusCode.Conflict, overlappingResponse.StatusCode);
            Assert.Equal("application/problem+json", overlappingResponse.Content.Headers.ContentType?.MediaType);

            var problem = await overlappingResponse.Content.ReadFromJsonAsync<Microsoft.AspNetCore.Mvc.ProblemDetails>();
            Assert.Equal("Booking conflict", problem.Title);
            Assert.NotNull(problem);
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
        public async Task RescheduleBooking_EndDateBeforeStartDate_Returns404WithProblemDetails()
        {
            // Arrange lav accommodation
            var accommodationId = await SeedAccommodationAsync();

            // Act 1 POST en reschedule med endDate < startDate
            var rescheduleRequest = new RescheduleBookingRequest(DateOnly.FromDateTime(DateTime.Now.AddDays(12)), DateOnly.FromDateTime(DateTime.Now.AddDays(10)));
            var rescheduleResponse = await _client.PostAsJsonAsync($"api/accommodations/{accommodationId}/bookings/{Guid.NewGuid()}/reschedule", rescheduleRequest);

            // Assert 400, og bodyen er ProblemDetails
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, rescheduleResponse.StatusCode);
            Assert.Equal("application/problem+json", rescheduleResponse.Content.Headers.ContentType?.MediaType);
            var problem = await rescheduleResponse.Content.ReadFromJsonAsync<Microsoft.AspNetCore.Mvc.ProblemDetails>();
            Assert.Equal("Invalid booking", problem.Title);
            Assert.NotNull(problem);
            Assert.Equal(400, problem.Status);
        }
    }
}
