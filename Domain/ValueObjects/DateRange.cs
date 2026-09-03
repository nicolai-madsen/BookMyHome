namespace Domain.ValueObjects
{
    public sealed record DateRange
    {
        public DateOnly StartDate { get; }
        public DateOnly EndDate { get; }
        public int Duration => StartDate.DayNumber - EndDate.DayNumber;
        public DateRange() { } // Empty ctor for EF Core
        public DateRange(DateOnly start, DateOnly end)
        {
            if (start >= end)
                throw new ArgumentException(nameof(start));

            StartDate = start;
            EndDate = end;
        }

        public int DurationDays() // Probably did a dumb here. But it should return an integer, that represents the amount of days in the given date range
        {
            return Duration;
        }

        public bool OverlapsWith(DateRange other) // Does this DateRange object overlap with this other DateRange object?
        {
            if (other == null)
                throw new ArgumentException(nameof(other));

            return StartDate < other.EndDate &&
                EndDate > other.StartDate;
        }

        public bool Contains(DateOnly date) // Does this date appear in this DateRange object?
        {
            return date >= StartDate &&
                date < EndDate;
        }

        public override string ToString()
        {
            return $"{StartDate} - {EndDate}";
        }
    };
}
