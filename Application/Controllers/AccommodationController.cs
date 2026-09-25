using Application.Mapping;
using Domain;
using Domain.Exceptions;
using Domain.Repositories;
using Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using Shared.DomainDtos;
using Shared.UseCaseDtos;

namespace Application.Controllers
{
    [ApiController]
    [Route("api/accommodations")]
    public class AccommodationController : ControllerBase
    {
        private readonly IAccommodationRepository _repository;
        private readonly IUnitOfWork _unitOfWork;

        public AccommodationController(IAccommodationRepository repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AccommodationDto>>> GetAll()
        {
            var accommodations = await _repository.GetAllAsync();
            var dtos = accommodations.Select(a => new AccommodationDto(a.Id, a.PricePerDay));

            return Ok(dtos);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<AccommodationDto>> GetById(Guid id)
        {
            var accommodation = await _repository.GetByIdAsync(id);

            if (accommodation == null)
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Accommodation not found",
                    detail: $"No accommodation exists with id: {id}.");

            var dto = new AccommodationDto(accommodation.Id, accommodation.PricePerDay);
            return Ok(dto);
        }

        [HttpGet("{accommodationId:guid}/bookings/{bookingId:guid}", Name = "GetBooking")]
        public async Task<ActionResult<BookingDto>> GetBooking(Guid accommodationId, Guid bookingId)
        {
            var accommodation = await _repository.GetByIdAsync(accommodationId);

            if (accommodation == null)
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Accommodation not found",
                    detail: $"No accommodation exists with id: {accommodationId}.");

            var booking = accommodation.Bookings.FirstOrDefault(b => b.Id == bookingId);

            if (booking is null)
                return NotFound($"No booking exists with Id: {bookingId}.");
            return booking.ToDto();

        }

        [HttpGet("{accommodationId:guid}/bookings")]
        [ProducesResponseType<IEnumerable<BookingDto>>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<BookingDto>>> GetAllBookings(Guid accommodationId)
        {
            var accommodation = await _repository.GetByIdAsync(accommodationId);
            if (accommodation == null)
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Accommodation not found",
                    detail: $"No accommodation exists with id: {accommodationId}.");

            return Ok(accommodation.Bookings.Select(b => b.ToDto()));
        }

        [HttpPost("{accommodationId:guid}/bookings")]
        [ProducesResponseType<BookingDto>(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BookingDto>> CreateBooking(Guid accommodationId, CreateBookingRequest request) 
        {
            var accommodation = await _repository.GetByIdAsync(accommodationId);
            
            if (accommodation == null)
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Accommodation not found",
                    detail: $"No accommodation exists with id: {accommodationId}.");

                var period = new DateRange(request.StartDate, request.EndDate);

                // TODO: "today" skal komme fra en injiceret TimeProvider og bruge boligens tidszone, ikke serverens lokale tid. Se domænets today-parameter.
                var booking = accommodation.AddBooking(request.GuestId, period, DateOnly.FromDateTime(DateTime.Today));

                await _unitOfWork.SaveChangesAsync();

                return CreatedAtAction(nameof(GetBooking), new { accommodationId, bookingId = booking.Id }, booking.ToDto());        
        }

        [HttpPost("{accommodationId:guid}/bookings/{bookingId:guid}/reschedule")]
        [ProducesResponseType<BookingDto>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BookingDto>> RescheduleBooking(Guid accommodationId, Guid bookingId, RescheduleBookingRequest request)
        {
            var accommodation = await _repository.GetByIdAsync(accommodationId);
            if (accommodation == null)
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Accommodation not found",
                    detail: $"No accommodation exists with id: {accommodationId}.");

            var booking = accommodation.RescheduleBooking(bookingId, new DateRange(request.NewStartDate, request.NewEndDate), DateOnly.FromDateTime(DateTime.Today));

            await _unitOfWork.SaveChangesAsync();
            return Ok(booking.ToDto());
        }
    }
}