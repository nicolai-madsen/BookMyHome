using Domain.Exceptions;
using Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using Persistence.Repositories;
using Shared.DomainDtos;
using Shared.UseCaseDtos;

namespace Application.Controllers
{
    [ApiController]
    [Route("api/accommodations")]
    public class AccommodationController : ControllerBase
    {
        private readonly IAccommodationRepository _repository;

        public AccommodationController(IAccommodationRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AccommodationDto>>> GetaAll()
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
                return NotFound($"No accommodation exists with id:{id}.");

            var dto = new AccommodationDto(accommodation.Id, accommodation.PricePerDay);
            return Ok(dto);
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
                return NotFound($"No accommodation exists with id: {accommodationId}.");

            try
            {
                var period = new DateRange(request.StartDate, request.EndDate);

                // TODO: "today" skal komme fra en injiceret TimeProvider og bruge boligens
                // tidszone, ikke serverens lokale tid. Se domænets today-parameter.
                var booking = accommodation.AddBooking(request.GuestId, period, DateOnly.FromDateTime(DateTime.Today));

                await _repository.SaveChangesAsync();

                return CreatedAtAction(nameof(GetById), new { accommodationId, bookingId = booking.Id });
            }
            catch(OverlappingBookingException ex)
            {
                return Conflict(ex.Message); // 409 Conflict
            }
        }
    }
}