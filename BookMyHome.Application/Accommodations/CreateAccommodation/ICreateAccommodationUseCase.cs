namespace BookMyHome.Application.Accommodations.CreateAccommodation
{
    public interface ICreateAccommodationUseCase
    {
        Task<Guid> ExecuteAsync(CreateAccommodationUseCaseCommand command);
    }
}
