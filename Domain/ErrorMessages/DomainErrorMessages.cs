namespace Domain.Errors
{
    public static class DomainErrorMessages
    {
        public static string OverlappingBooking(Guid bookingId) =>
            $"Start time of the booking: {bookingId} overlaps with an already existing booking";

        public static string EndTimeMustBeLaterThanStartTime =
            "End must be later than the start time";


        public static string StartTimeMustBeInTheFuture =
           "Start must be in the future";
    }
}
