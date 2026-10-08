using BookMyHome.Application.Accommodations.CreateAccommodation;
using BookMyHome.Domain.Aggregates.Accommodations;
using BookMyHome.Domain.Aggregates.Users;
using BookMyHome.Domain.Exceptions;
using BookMyHome.Domain.Interfaces;
using BookMyHome.Domain.Interfaces.Repositories;
using BookMyHome.Domain.ValueObjects;
using Moq;

namespace BookMyHome.Application.Tests
{
    public class CreateAccommodationUseCaseTests
    {
        private readonly Mock<IUserRepository> _users = new();
        private readonly Mock<IAccommodationRepository> _accommodations = new();
        private readonly Mock<IUnitOfWork> _unitOfWork = new();
        private readonly CreateAccommodationUseCase _sut;

        public CreateAccommodationUseCaseTests()
        {
            _sut = new CreateAccommodationUseCase(
                accommodationRepository: _accommodations.Object,
                userRepository: _users.Object,
                unitOfWork: _unitOfWork.Object);
        }

        private static User MakeUser(Guid id, bool isHost)
        {
            var user = new User(id, new FullName("Test", "User"), new Email("test@test.dk"),
                                new PhoneNumber("+4512345678"), "test", "hash");
            if (isHost) user.BecomeHost(DateTimeOffset.UtcNow);
            return user;
        }

        private static CreateAccommodationUseCaseCommand MakeCommand(Guid hostId) =>
            new(hostId, "Vej", "1", "By", "1234", "Denmark",
                new DateOnly(2027, 1, 1), new DateOnly(2027, 12, 31), 100m);

        [Fact]
        public async Task ExecuteAsync_UserNotFound_ThrowsUserNotFoundException()
        {
            var hostId = Guid.NewGuid();
            _users.Setup(r => r.GetByIdAsync(hostId)).ReturnsAsync((User?)null);

            await Assert.ThrowsAsync<UserNotFoundException>(() => _sut.ExecuteAsync(MakeCommand(hostId)));
        }

        [Fact]
        public async Task ExecuteAsync_UserIsNotHost_ThrowsUserIsNotHostException()
        {
            var userId = Guid.NewGuid();
            _users.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(MakeUser(userId, isHost: false));

            await Assert.ThrowsAsync<UserIsNotHostException>(() => _sut.ExecuteAsync(MakeCommand(userId)));
        }

        [Fact]
        public async Task ExecuteAsync_UserIsNotHost_DoesNotSave()
        {
            var userId = Guid.NewGuid();
            _users.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(MakeUser(userId, isHost: false));

            await Assert.ThrowsAsync<UserIsNotHostException>(() => _sut.ExecuteAsync(MakeCommand(userId)));

            _accommodations.Verify(r => r.Add(It.IsAny<Accommodation>()), Times.Never);
            _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_ValidHost_AddsAccommodationAndSavesOnce()
        {
            var hostId = Guid.NewGuid();
            _users.Setup(r => r.GetByIdAsync(hostId)).ReturnsAsync(MakeUser(hostId, isHost: true));

            Accommodation? added = null;
            _accommodations.Setup(r => r.Add(It.IsAny<Accommodation>()))
                           .Callback<Accommodation>(a => added = a);

            var returnedId = await _sut.ExecuteAsync(MakeCommand(hostId));

            Assert.NotNull(added);
            Assert.Equal(hostId, added.HostId);
            Assert.Equal(added.Id, returnedId);
            _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}