using Domain.Aggregates.Accommodations;
using Domain.Errors;
using Domain.Exceptions;
using Domain.ValueObjects;
using System.Diagnostics;

namespace Tests
{
    public class DomainTests
    {
        [Fact]
        public void TotalDays_InclusiveRange_CountsBothEndoints()
        {
            // Arrange 
            var range = new DateRange(new DateOnly(2026, 6, 1), new DateOnly(2026, 6, 10));

            // Act
            var result = range.TotalDays;

            // Assert
            Assert.Equal(10, result);
        }

        [Theory]
        [InlineData(1, 5, 10, 15, false)] // Before
        [InlineData(10, 15, 1, 5, false)] // After
        [InlineData(1, 10, 10, 15, true)] // touches 
        [InlineData(1, 20, 5, 10, true)] // inside
        [InlineData(1, 10, 1, 10, true)] // identical
        public void OverlapsWith_ReturnsExpected(int aStart, int aEnd,  int bStart, int bEnd, bool expected)
        {
            var a = new DateRange(new DateOnly(2026, 6, aStart), new DateOnly(2026, 6, aEnd));
            var b = new DateRange(new DateOnly(2026, 6, bStart), new DateOnly(2026, 6, bEnd));

            Assert.Equal(expected, a.OverlapsWith(b));
        }

        [Fact]
        public void Ctor_EndBeforeStart_Throws()
        {
            Assert.Throws<ArgumentException>(() =>
                new DateRange(new DateOnly(2026, 6, 10), new DateOnly(2026, 6, 1)));
        }

        [Theory]
        [InlineData(10, 10, 15, true)] // Start
        [InlineData(10, 5, 15, true)] // somewhere in there
        [InlineData(15, 10, 15, true)] // end
        [InlineData(10, 15, 20, false)] // before
        [InlineData(20, 10, 15, false)] // after
        public void Contains_ReturnsExpected(int date, int start, int end, bool expected)
        {
            var testDate = new DateOnly(2026, 6, date);
            var testDateRange = new DateRange(new DateOnly(2026, 6, start), new DateOnly(2026, 6, end));

            Assert.Equal(expected, testDateRange.Contains(testDate));
        }

        [Fact]
        public void Constructor_StartDateBeforeToday_Throws()
        {
            // Arrange
            var today = new DateOnly(2026, 9, 10);
            var period = new DateRange(new DateOnly(2026, 9, 9), new DateOnly(2026, 9, 12));

            // Act & Assert
            var exception = Assert.Throws<Exceptions.StartTimeMustBeInTheFuture>(() =>
                new Booking(Guid.NewGuid(), Guid.NewGuid(), period, 100m, today));
            Assert.Equal("StartTimeMustBeInTheFuture", exception.Message);
        }
    }
}
