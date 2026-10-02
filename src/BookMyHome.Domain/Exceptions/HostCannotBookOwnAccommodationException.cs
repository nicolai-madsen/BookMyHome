using BookMyHome.Domain.Exceptions;

public sealed class HostCannotBookOwnAccommodationException : DomainException
{
    public HostCannotBookOwnAccommodationException(Guid accommodationId, Guid userId)
        : base($"User {userId} is the host of accommodation {accommodationId} and cannot book it.") { }
}