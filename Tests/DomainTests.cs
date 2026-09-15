using Domain.Aggregates.Accommodations;
using Domain.Exceptions;
using Domain.ValueObjects;

namespace Tests
{
    public class DomainTests
    {
        private static Accommodation CreateAccommodation(decimal pricePerDay = 100m) =>
           new Accommodation(
               Guid.NewGuid(),
               Guid.NewGuid(),
               new Address("TestStreet", "43", "Testcity", "1919", "Jugoslavia"),
               new DateRange(new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31)),
               pricePerDay);

        [Fact]
        public void BookingConstructor_StartDayIsToday_CreatesBookingSuccessfully()
        {
            // Arrange
            var today = new DateOnly(2026, 9, 10);
            var period = new DateRange(new DateOnly(2026, 9, 10), new DateOnly(2026, 9, 12));

            // Act and Assert
            var booking = new Booking(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), period, 100m, today);
        }

        #region DateRange Tests
        [Fact]
        public void DateRange_TotalDays_InclusiveRange_CountsBothEndoints()
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
        [InlineData(1, 10, 10, 15, true)] // touches on the left
        [InlineData(15, 20, 10, 15, true)] // touches on the right
        [InlineData(1, 20, 5, 10, true)] // inside
        [InlineData(1, 10, 1, 10, true)] // identical
        public void DateRange_OverlapsWith_ReturnsExpected(int aStart, int aEnd, int bStart, int bEnd, bool expected)
        {
            var a = new DateRange(new DateOnly(2026, 6, aStart), new DateOnly(2026, 6, aEnd));
            var b = new DateRange(new DateOnly(2026, 6, bStart), new DateOnly(2026, 6, bEnd));

            Assert.Equal(expected, a.OverlapsWith(b));
        }

        [Fact]
        public void DateRangeConstructor_EndDateIsBeforeStartDate_Throws()
        {
            Assert.Throws<EndDateIsBeforeStartDateException>(() =>
                new DateRange(new DateOnly(2026, 6, 10), new DateOnly(2026, 6, 1)));
        }

        [Theory]
        [InlineData(10, 10, 15, true)] // Start
        [InlineData(10, 5, 15, true)] // somewhere in there
        [InlineData(15, 10, 15, true)] // end
        [InlineData(10, 15, 20, false)] // before
        [InlineData(20, 10, 15, false)] // after
        public void DateRange_Contains_ReturnsExpected(int date, int start, int end, bool expected)
        {
            var testDate = new DateOnly(2026, 6, date);
            var testDateRange = new DateRange(new DateOnly(2026, 6, start), new DateOnly(2026, 6, end));

            Assert.Equal(expected, testDateRange.Contains(testDate));
        }

        [Fact]
        public void DateRangeConstructor_StartDateBeforeToday_Throws()
        {
            // Arrange
            var today = new DateOnly(2026, 9, 10);
            var period = new DateRange(new DateOnly(2026, 9, 9), new DateOnly(2026, 9, 12));

            // Act & Assert
            var exception = Assert.Throws<BookingStartDateCannotBeInThePastException>(() =>
                new Booking(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), period, 100m, today));
        }

        [Fact]
        public void DateRangeConstructor_StartEqualsEnd_Throws()
        {
            // Arractsert..?
            Assert.Throws<BookingStartAndEndDateCannotBeEqualException>(() =>
                new DateRange(new DateOnly(2026, 9, 10), new DateOnly(2026, 9, 10)));
        }
        #endregion

        #region AddBooking(); Tests
        [Fact]
        public void AddBooking_NoExistingBookings_AddsBooking()
        {
            // Arrange
            var accommodation = CreateAccommodation();
            var today = new DateOnly(2026, 9, 10);
            var period = new DateRange(new DateOnly(2026, 10, 20), new DateOnly(2026, 10, 25));

            // Act
            var booking = accommodation.AddBooking(Guid.NewGuid(), period, today);

            // Assert
            Assert.Single(accommodation.Bookings);
            Assert.Equal(accommodation.Id, booking.AccommodationId);
            Assert.Equal(600m, booking.TotalPrice); // 100m = price per day from the helper method above, 20-25th is 6 days with business rules
        }

        [Fact]
        public void AddBooking_NotOverlappingBooking_AddsBooking()
        {
            // Arrange
            var accommodation = CreateAccommodation();
            var today = new DateOnly(2026, 9, 10);
            var firstPeriod = new DateRange(new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 5));
            var secondPeriod = new DateRange(new DateOnly(2026, 10, 20), new DateOnly(2026, 10, 25));
            accommodation.AddBooking(Guid.NewGuid(), firstPeriod, today);
            accommodation.AddBooking(Guid.NewGuid(), secondPeriod, today);

            // Act
            var newPeriod = new DateRange(new DateOnly(2026, 10, 10), new DateOnly(2026, 10, 16));
            var newBooking = accommodation.AddBooking(Guid.NewGuid(), newPeriod, today);

            // Assert
            Assert.Equal(3, accommodation.Bookings.Count);
            Assert.Equal(700m, newBooking.TotalPrice);
        }

        [Fact]
        public void AddBooking_OverlappingBooking_Throws()
        {
            // Arrange
            var accommodation = CreateAccommodation();
            var today = new DateOnly(2026, 9, 10);
            var firstPeriod = new DateRange(new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 5));
            accommodation.AddBooking(Guid.NewGuid(), firstPeriod, today);
            // Act & Assert
            var overlappingPeriod = new DateRange(new DateOnly(2026, 10, 4), new DateOnly(2026, 10, 10));
            Assert.Throws<OverlappingBookingException>(() =>
                accommodation.AddBooking(Guid.NewGuid(), overlappingPeriod, today));
            Assert.Single(accommodation.Bookings);
        }

        [Fact]
        public void AddBooking_OverlappingWithCancelledBooking_AddsBooking()
        {
            // Arrange
            var accommodation = CreateAccommodation();
            var today = new DateOnly(2026, 9, 10);
            var firstPeriod = new DateRange(new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 5));
            var firstBooking = accommodation.AddBooking(Guid.NewGuid(), firstPeriod, today);
            firstBooking.CancelByGuest();
            // Act
            var overlappingPeriod = new DateRange(new DateOnly(2026, 10, 4), new DateOnly(2026, 10, 10));
            var newBooking = accommodation.AddBooking(Guid.NewGuid(), overlappingPeriod, today);
            // Assert
            Assert.Equal(2, accommodation.Bookings.Count); // The cancelled booking is still in the list of bookings
            Assert.Equal(accommodation.Id, newBooking.AccommodationId); // The new booking is added successfully
        }
        #endregion

        #region RescheduleBooking Tests
        [Fact]
        public void RescheduleBooking_NoConflicts_ReschedulesBooking()
        {
            // Arrange
            var accommodation = CreateAccommodation();
            var today = new DateOnly(2026, 9, 10);
            var originalPeriod = new DateRange(new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 5));
            var booking = accommodation.AddBooking(Guid.NewGuid(), originalPeriod, today);

            // Act
            var newPeriod = new DateRange(new DateOnly(2026, 10, 10), new DateOnly(2026, 10, 15));
            accommodation.RescheduleBooking(booking.Id, newPeriod, today);

            // Assert
            Assert.Equal(newPeriod.StartDate, booking.RentalPeriod.StartDate);
            Assert.Equal(newPeriod.EndDate, booking.RentalPeriod.EndDate);
            Assert.Equal(600m, booking.TotalPrice);
        }

        [Fact]
        public void RescheduleBooking_OverlappingBooking_Throws()
        {
            // Arrange
            var accommodation = CreateAccommodation();
            var today = new DateOnly(2026, 9, 10);
            var firstPeriod = new DateRange(new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 5));
            var secondPeriod = new DateRange(new DateOnly(2026, 10, 10), new DateOnly(2026, 10, 15));
            var firstBooking = accommodation.AddBooking(Guid.NewGuid(), firstPeriod, today);
            accommodation.AddBooking(Guid.NewGuid(), secondPeriod, today);

            // Act & Assert
            var overlappingPeriod = new DateRange(new DateOnly(2026, 10, 8), new DateOnly(2026, 10, 12));
            Assert.Throws<OverlappingBookingException>(() =>
                accommodation.RescheduleBooking(firstBooking.Id, overlappingPeriod, today));
        }

        [Fact]
        public void RescheduleBooking_NonExistentBooking_Throws()
        {
            // Arrange
            var accommodation = CreateAccommodation();
            var today = new DateOnly(2026, 9, 10);
            var period = new DateRange(new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 5));
            var booking = accommodation.AddBooking(Guid.NewGuid(), period, today);

            // Act & Assert
            var newPeriod = new DateRange(new DateOnly(2026, 10, 10), new DateOnly(2026, 10, 15));
            Assert.Throws<BookingNotFoundException>(() =>
                accommodation.RescheduleBooking(Guid.NewGuid(), newPeriod, today));
            Assert.Equal(period, booking.RentalPeriod);
        }

        [Fact]
        public void RescheduleBooking_CancelledBooking_Throws() 
        {
            // Arrange
            var accommodation = CreateAccommodation();
            var today = new DateOnly(2026, 9, 10);
            var period = new DateRange(new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 5));
            accommodation.AddBooking(Guid.NewGuid(), period, today);

            // Act & Assert
            var bookingToCancel = accommodation.Bookings.First();
            bookingToCancel.CancelByGuest();
            var newPeriod = new DateRange(new DateOnly(2026, 10, 10), new DateOnly(2026, 10, 15));
            Assert.Throws<BookingNotActiveException>(() =>
                accommodation.RescheduleBooking(bookingToCancel.Id, newPeriod, today));
        }
        #endregion
    }
}
