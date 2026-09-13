namespace Domain.Exceptions
{
    public class BookingStartDateCannotBeInThePastException : DomainException
    {
        public BookingStartDateCannotBeInThePastException()
            : base("Start date cannot be in the past")
        { }
    }
}
