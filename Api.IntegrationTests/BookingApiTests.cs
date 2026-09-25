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

        [Fact]
        public async Task CreateBooking_OverlappingDates_Returns409WithProblemDetails()
        {
            // Arrange lav accommodation
            var accommodationAvailableStart = DateOnly.FromDateTime(DateTime.Now.AddDays(1));
            var accommodationAvailableEnd = DateOnly.FromDateTime(DateTime.Now.AddDays(365));

            var bookingStart = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
            var bookingEnd = DateOnly.FromDateTime(DateTime.Now.AddDays(32));

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
            Assert.NotNull(problem);
            Assert.Equal(409, problem.Status);
        }
    }
}
