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
        public async Task<ActionResult<BookingDto>> CreateBooking(Guid accommodationId, CreateBookingRequest request) 
        {
            var accommodation = await _repository.GetByIdAsync(accommodationId);
            if (accommodation == null)
                return NotFound($"No accommodation exists with id: {accommodationId}.");

            var period = new DateRange(request.StartDate, request.EndDate);
            accommodation.AddBooking(request.GuestId, period, );
        }
    }
}