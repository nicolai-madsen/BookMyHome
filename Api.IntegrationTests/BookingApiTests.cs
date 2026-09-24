namespace Api.IntegrationTests
{
    public class BookingApiTests(BookMyHomeApiFactory factory) : IClassFixture<BookMyHomeApiFactory>
    {
        private readonly HttpClient _client = factory.CreateClient();

        [Fact]
        public async Task CreateBooking_OverlappingDates_Returns409WithProblemDetails()
        {
            // Arrange lav accommodation

            // Act 1 POST en booking, forvent 201

            // Act 2 POST en booking på samme datoer

            // Assert 409, og bodyen er ProblemDetails
        }
    }
}
