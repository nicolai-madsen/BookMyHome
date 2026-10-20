using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using BookMyHome.Persistence;
using BookMyHome.Shared.UseCaseDtos;

namespace BookMyHome.Api.IntegrationTests
{
    public class AccommodationApiTests(BookMyHomeApiFactory factory)  : IClassFixture<BookMyHomeApiFactory>
    {
        private readonly HttpClient _client = factory.CreateClient();
        private static DateOnly Days(int n) => DateOnly.FromDateTime(DateTime.Today.AddDays(n));

        private static CreateAccommodationRequest NewAccommodation(Guid hostId) => new(
            hostId, "Testvej", "9", "Vejle", "7100", "Denmark", Days(1), Days(365), 250m);

        private static CreateAccommodationRequest New0PricePerDayAccommodation(Guid hostId) => new(
           hostId, "Testvej", "9", "Vejle", "7100", "Denmark", Days(1), Days(365), 0m);

        private async Task<Guid> CreateAccommodationAsync(Guid hostId)
        {
            var response = await _client.PostAsJsonAsync("api/accommodations", NewAccommodation(hostId));
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            return Guid.Parse(response.Headers.Location!.Segments.Last());
        }

        [Fact]
        public async Task Create_AllIsOk_Returns201()
        {
            var response = await _client.PostAsJsonAsync("api/accommodations", NewAccommodation(DbSeeder.Host1Id));

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task Create_ByUserWhoIsNotHost_Returns403()
        {
            var response = await _client.PostAsJsonAsync("api/accommodations", NewAccommodation(DbSeeder.Guest1Id));

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Create_UserDoesntExist_Returns404()
        {
            var response = await _client.PostAsJsonAsync("api/accommodations", NewAccommodation(Guid.NewGuid()));

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }


        [Fact]
        public async Task Create_PricePerDayIs0_Returns400()
        {
            var response = await _client.PostAsJsonAsync("api/accommodations", New0PricePerDayAccommodation(DbSeeder.Host1Id));

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }



        [Fact]
        public async Task Delete_WithUpcomingBooking_Returns409()
        {
            var id = await CreateAccommodationAsync(DbSeeder.Host1Id);
            var booking = new CreateBookingRequest(DbSeeder.Guest1Id, Days(10), Days(15));
            (await _client.PostAsJsonAsync($"api/accommodations/{id}/bookings", booking)).EnsureSuccessStatusCode();

            var response = await _client.DeleteAsync($"api/accommodations/{id}/?userId={DbSeeder.Host1Id}");

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact]
        public async Task Delete_ThenGet_Returns404()
        {
            var id = await CreateAccommodationAsync(DbSeeder.Host1Id);

            var delete = await _client.DeleteAsync($"api/accommodations/{id}?userId={DbSeeder.Host1Id}");
            var get = await _client.GetAsync($"api/accommodations/{id}");

            Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
        }
    }
}