namespace Domain.Errors
{
    public static class DomainErrorMessages
    {
        public const string OverlappingBooking =
            "Start time of the booking overlaps with an already existing booking";

        public const string EndTimeMustBeLaterThanStartTime =
            "End must be later than the start time";
    }
}
