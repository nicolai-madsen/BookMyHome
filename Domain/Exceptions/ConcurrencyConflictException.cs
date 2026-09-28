namespace Domain.Exceptions
{
    public class ConcurrencyConflictException : Exception
    {
        public ConcurrencyConflictException(Exception innerException)
            :base("The data was modified by another request.", innerException) { }
    }
}
