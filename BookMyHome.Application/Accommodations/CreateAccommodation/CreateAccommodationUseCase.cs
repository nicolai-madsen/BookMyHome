using BookMyHome.Domain.Aggregates.Accommodations;
using BookMyHome.Domain.Exceptions;
using BookMyHome.Domain.Interfaces;
using BookMyHome.Domain.Interfaces.Repositories;
using BookMyHome.Domain.ValueObjects;

namespace BookMyHome.Application.Accommodations.CreateAccommodation
{
    public class CreateAccommodationUseCase : ICreateAccommodationUseCase
    {
        private readonly IAccommodationRepository _accommodationRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        
        public CreateAccommodationUseCase(IAccommodationRepository accommodationRepository, IUserRepository userRepository)
        {
            _accommodationRepository = accommodationRepository;
            _userRepository = userRepository;
        }

        public async Task<Guid> ExecuteAsync(CreateAccommodationUseCaseCommand command, CancellationToken ct = default)
        {
            var host = await _userRepository.GetByIdAsync(command.HostId) ?? throw new UserNotFoundException(command.HostId);

            if (!host.IsHost)
                throw new UserIsNotHostException(command.HostId);
            
            var address = new Address(command.StreetName, command.StreetNumber, command.City, command.ZipCode, command.Country);
            var period = new DateRange(command.AvailableFrom, command.AvailableTo);

            var accommodation = new Accommodation(Guid.NewGuid(), host.Id, address, period, command.PricePerDay);

            _accommodationRepository.Add(accommodation);

            await _unitOfWork.SaveChangesAsync(ct);

            return accommodation.Id;
        }
    }
}
