using BookMyHome.Api.Mapping;
using BookMyHome.Application.Accommodations.CreateAccommodation;
using BookMyHome.Application.Accommodations.DeleteAccommodation;
using BookMyHome.Application.Accommodations.UpdateAccommodation;
using BookMyHome.Domain.Exceptions;
using BookMyHome.Domain.Interfaces.Repositories;
using BookMyHome.Shared.DomainDtos;
using BookMyHome.Shared.UseCaseDtos;
using Microsoft.AspNetCore.Mvc;

namespace BookMyHome.Api.Controllers
{
    [ApiController]
    [Route("api/accommodations")]
    public class AccommodationsController(
        IAccommodationRepository repository,
        ICreateAccommodationUseCase createAccommodation,
        IUpdateAccommodationUseCase updateAccommodation,
        IDeleteAccommodationUseCase deleteAccommodation) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AccommodationDto>>> GetAll([FromQuery] Guid? hostId)
        {
            var accommodations = hostId is { } id
                ? await repository.GetByHostIdAsync(id)
                : await repository.GetAllAsync();

            return Ok(accommodations.Select(a => a.ToDto()));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AccommodationDto>> GetById(Guid id)
        {
            var accommodation = await repository.GetByIdAsync(id)
                ?? throw new AccommodationNotFoundException(id);

            return Ok(accommodation.ToDto());
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Create(CreateAccommodationRequest request, CancellationToken ct)
        {
            var command = new CreateAccommodationUseCaseCommand(
                request.HostId, request.StreetName, request.StreetNumber, request.City,
                request.ZipCode, request.Country, request.AvailableFrom, request.AvailableTo,
                request.PricePerDay);

            var id = await createAccommodation.ExecuteAsync(command, ct);

            return CreatedAtAction(nameof(GetById), new { id }, null);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Update(Guid id, UpdateAccommodationRequest request, CancellationToken ct)
        {
            var command = new UpdateAccommodationUseCaseCommand(
                id, request.UserId, request.StreetName, request.StreetNumber, request.City,
                request.ZipCode, request.Country, request.AvailableFrom, request.AvailableTo,
                request.PricePerDay);

            await updateAccommodation.ExecuteAsync(command, ct);
            return NoContent();
        }

        // HUSK!!! Opgave 11: userId fra JWT. Query string er en midlertidig løsning,fordi DELETE-requests ikke skal have en body.
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Delete(Guid id, [FromQuery] Guid userId, CancellationToken ct)
        {
            await deleteAccommodation.ExecuteAsync(new DeleteAccommodationUseCaseCommand(id, userId), ct);
            return NoContent();
        }
    }
}