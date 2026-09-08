using Domain.Errors;

namespace Domain.ValueObjects
{
    public sealed record DateRange
    {
        public DateOnly StartDate { get; }
        public DateOnly EndDate { get; }
        public int TotalDays => EndDate.DayNumber - StartDate.DayNumber + 1; // dato er "til -og med", derfor + 1
        private DateRange() { } // Empty and PRIVATE ctor for EF Core
        public DateRange(DateOnly start, DateOnly end)
        {
            if (start >= end)
                throw new ArgumentException(DomainErrorMessages.EndTimeMustBeLaterThanStartTime, nameof(start));

            StartDate = start;
            EndDate = end;
        }

        public bool OverlapsWith(DateRange other) // Does this DateRange object overlap with this other DateRange object?
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            return other.StartDate <= EndDate && other.EndDate >= StartDate; // <= "less than or equal to" er "til -og med"
        }

        public bool Contains(DateOnly date) // Does this date appear in this DateRange object?
        {
            return date >= StartDate && date <= EndDate;  
        }

        public override string ToString()
        {
            return $"{StartDate} - {EndDate}";
        }
    };
}
