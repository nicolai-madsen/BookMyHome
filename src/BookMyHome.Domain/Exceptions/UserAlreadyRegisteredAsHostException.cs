namespace BookMyHome.Domain.Exceptions
{
    public class UserAlreadyRegisteredAsHostException : DomainException
    {
        public UserAlreadyRegisteredAsHostException(Guid userId)
            : base($"User with ID {userId} is already registered as a host.")
        { }
    }
}
