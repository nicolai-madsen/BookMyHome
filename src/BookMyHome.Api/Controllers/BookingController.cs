using BookMyHome.Api.Extensions;
using BookMyHome.Api.Mapping;
using BookMyHome.Application.Accommodations.CreateBooking;
using BookMyHome.Application.Accommodations.RescheduleBooking;
using BookMyHome.Domain.Aggregates.Accommodations;
using BookMyHome.Domain.Exceptions;
using BookMyHome.Domain.Interfaces;
using BookMyHome.Domain.Interfaces.Repositories;
using BookMyHome.Shared.DomainDtos;
using BookMyHome.Shared.UseCaseDtos.Accommodations;
using Microsoft.AspNetCore.Mvc;

namespace BookMyHome.Api.Controllers
{
    [ApiController]
    [Route("api/accommodations/{accommodationId:guid}/bookings")]
    public class BookingsController(
        ICreateBookingUseCase createBooking,
        IRescheduleBookingUseCase rescheduleBooking,
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

        private async Task<BookingDto> GetBookingDtoAsync(Guid accommodationId, Guid bookingId)
        {
            var accommodation = await LoadAsync(accommodationId);

            var booking = accommodation.Bookings.FirstOrDefault(b => b.Id == bookingId)
                ?? throw new BookingNotFoundException(bookingId);

            return booking.ToDto();
        }

        [HttpGet("{bookingId:guid}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BookingDto>> GetById(Guid accommodationId, Guid bookingId)
        {
            var accommodation = await LoadAsync(accommodationId);

            var booking = accommodation.Bookings.FirstOrDefault(b => b.Id == bookingId)
                ?? throw new BookingNotFoundException(bookingId);

            return Ok(await GetBookingDtoAsync(accommodationId, bookingId));
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Create(Guid accommodationId, CreateBookingRequest request, CancellationToken ct)
        {
            var command = new CreateBookingCommand(
                accommodationId, User.GetUserId(), request.StartDate, request.EndDate);

            var id = await createBooking.ExecuteAsync(command, ct);
            return CreatedAtAction(nameof(GetById), new { accommodationId, bookingId = id }, null);
        }

        [HttpPost("{bookingId:guid}/reschedule")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Reschedule(Guid accommodationId, Guid bookingId, RescheduleBookingRequest request, CancellationToken ct)
        {
            var command = new RescheduleBookingUseCaseCommand(
                accommodationId, bookingId, User.GetUserId(), request.NewStartDate, request.NewEndDate);

            await rescheduleBooking.ExecuteAsync(command, ct);
            return NoContent();
        }

        [HttpPost("{bookingId:guid}/cancel")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Cancel(Guid accommodationId, Guid bookingId, CancellationToken ct)
        {
            var accommodation = await LoadAsync(accommodationId);
            accommodation.CancelBooking(bookingId, User.GetUserId());   // tilpas til din domænesignatur
            await unitOfWork.SaveChangesAsync(ct);
            return NoContent();
        }

        private async Task<Accommodation> LoadAsync(Guid accommodationId) =>
            await repository.GetByIdAsync(accommodationId)
            ?? throw new AccommodationNotFoundException(accommodationId);

        private DateOnly Today() => DateOnly.FromDateTime(time.GetLocalNow().DateTime);
    }
}