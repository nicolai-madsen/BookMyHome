namespace Domain.Exceptions
{
    public class EndDateIsBeforeStartDateException : DomainException
    {
        public EndDateIsBeforeStartDateException()
            : base("Start date of the booking cannot be after the end date.")
        { }
    }
}
