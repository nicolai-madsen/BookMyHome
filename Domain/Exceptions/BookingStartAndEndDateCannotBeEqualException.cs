namespace Domain.Exceptions
{
    public class BookingStartAndEndDateCannotBeEqualException : DomainException
    {
        public BookingStartAndEndDateCannotBeEqualException()
            : base("Booking start -& end date cannot be on the same date")
        { }
    }
}
