namespace Domain.Exceptions
{
    public sealed class ConcurrencyConflictException(string message, Exception inner)
        : Exception(message, inner);
}
