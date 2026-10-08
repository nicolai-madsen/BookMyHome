namespace BookMyHome.Domain.Exceptions
{
    public sealed class AccommodationHasActiveBookingsException : DomainException
    {
        public AccommodationHasActiveBookingsException(Guid accommodationId)
            : base($"Accommodation {accommodationId} has upcoming or ongoing bookings and cannot be deleted.") { }
    }
}