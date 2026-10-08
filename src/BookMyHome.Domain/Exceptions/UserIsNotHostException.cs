namespace BookMyHome.Domain.Exceptions
{
    public class UserIsNotHostException : DomainException
    {
        public UserIsNotHostException(Guid userId)
            : base($"User with ID {userId} is not registered as a host.")
        { }
    }
}
