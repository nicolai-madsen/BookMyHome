using BookMyHome.Api.Mapping;
using BookMyHome.Domain.Aggregates.Accommodations;
using BookMyHome.Domain.Exceptions;
using BookMyHome.Domain.Interfaces;
using BookMyHome.Domain.Interfaces.Repositories;
using BookMyHome.Domain.ValueObjects;
using BookMyHome.Shared.DomainDtos;
using BookMyHome.Shared.UseCaseDtos;
using Microsoft.AspNetCore.Mvc;

namespace BookMyHome.Api.Controllers
{
    [ApiController]
    [Route("api/accommodations/{accommodationId:guid}/bookings")]
    public class BookingsController(
        IAccommodationRepository repository,
        IUnitOfWork unitOfWork,
        TimeProvider time) : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<BookingDto>>> GetAll(Guid accommodationId)
        {
            var accommodation = await LoadAsync(accommodationId);
            return Ok(accommodation.Bookings.ToDtos());
        }

        [HttpGet("{bookingId:guid}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BookingDto>> GetById(Guid accommodationId, Guid bookingId)
        {
            var accommodation = await LoadAsync(accommodationId);

            var booking = accommodation.Bookings.FirstOrDefault(b => b.Id == bookingId)
                ?? throw new BookingNotFoundException(bookingId);

            return Ok(booking.ToDto());
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<BookingDto>> Create(Guid accommodationId, CreateBookingRequest request, CancellationToken ct)
        {
            var accommodation = await LoadAsync(accommodationId);

            var booking = accommodation.CreateBooking(
                request.GuestId, new DateRange(request.StartDate, request.EndDate), Today());

            await unitOfWork.SaveChangesAsync(ct);

            return CreatedAtAction(nameof(GetById),
                new { accommodationId, bookingId = booking.Id }, booking.ToDto());
        }

        [HttpPost("{bookingId:guid}/reschedule")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<BookingDto>> Reschedule(Guid accommodationId, Guid bookingId, RescheduleBookingRequest request, CancellationToken ct)
        {
            var accommodation = await LoadAsync(accommodationId);

            var booking = accommodation.RescheduleBooking(
                bookingId, new DateRange(request.NewStartDate, request.NewEndDate), Today());

            await unitOfWork.SaveChangesAsync(ct);
            return Ok(booking.ToDto());
        }

        [HttpPost("{bookingId:guid}/cancel")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<BookingDto>> Cancel(Guid accommodationId, Guid bookingId, CancelBookingRequest request, CancellationToken ct)
        {
            var accommodation = await LoadAsync(accommodationId);

            var booking = accommodation.CancelBooking(bookingId, request.UserId);

            await unitOfWork.SaveChangesAsync(ct);
            return Ok(booking.ToDto());
        }

        private async Task<Accommodation> LoadAsync(Guid accommodationId) =>
            await repository.GetByIdAsync(accommodationId)
            ?? throw new AccommodationNotFoundException(accommodationId);

        private DateOnly Today() => DateOnly.FromDateTime(time.GetLocalNow().DateTime);
    }
}