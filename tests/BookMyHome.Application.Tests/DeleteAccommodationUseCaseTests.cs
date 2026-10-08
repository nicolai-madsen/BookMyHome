using BookMyHome.Application.Accommodations.DeleteAccommodation;
using BookMyHome.Domain.Aggregates.Accommodations;
using BookMyHome.Domain.Exceptions;
using BookMyHome.Domain.Interfaces;
using BookMyHome.Domain.Interfaces.Repositories;
using BookMyHome.Domain.ValueObjects;
using Moq;

namespace BookMyHome.Application.Tests
{
    public class DeleteAccommodationUseCaseTests
    {
        private static readonly DateTimeOffset Now = new(2026, 10, 8, 12, 0, 0, TimeSpan.Zero);
        private static readonly DateOnly Today = DateOnly.FromDateTime(Now.DateTime);

        private readonly Mock<IAccommodationRepository> _accommodations = new();
        private readonly Mock<IUnitOfWork> _unitOfWork = new();
        private readonly DeleteAccommodationUseCase _sut;

        public DeleteAccommodationUseCaseTests()
        {
            _sut = new DeleteAccommodationUseCase(_accommodations.Object, _unitOfWork.Object, new FixedTimeProvider(Now));
        }

        private static Accommodation MakeAccommodation(Guid hostId) =>
            new(Guid.NewGuid(), hostId, new Address("Vej", "1", "By", "1234", "Denmark"),
                new DateRange(new DateOnly(2026, 1, 1), new DateOnly(2027, 12, 31)), 100m);

        [Fact]
        public async Task ExecuteAsync_UpcomingBooking_ThrowsAndDoesNotSave()
        {
            var hostId = Guid.NewGuid();
            var accommodation = MakeAccommodation(hostId);
            accommodation.CreateBooking(Guid.NewGuid(),
                new DateRange(Today.AddDays(10), Today.AddDays(15)), Today);

            _accommodations.Setup(r => r.GetByIdAsync(accommodation.Id)).ReturnsAsync(accommodation);

            await Assert.ThrowsAsync<AccommodationHasActiveBookingsException>(
                () => _sut.ExecuteAsync(new DeleteAccommodationUseCaseCommand(accommodation.Id, hostId)));

            Assert.False(accommodation.IsDeleted);
            _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_NoUpcomingBookings_SoftDeletesAndSaves()
        {
            var hostId = Guid.NewGuid();
            var accommodation = MakeAccommodation(hostId);
            _accommodations.Setup(r => r.GetByIdAsync(accommodation.Id)).ReturnsAsync(accommodation);

            await _sut.ExecuteAsync(new DeleteAccommodationUseCaseCommand(accommodation.Id, hostId));

            Assert.True(accommodation.IsDeleted);
            Assert.Equal(Now, accommodation.DeletedAt);
            _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}