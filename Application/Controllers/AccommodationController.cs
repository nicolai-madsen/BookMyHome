using Microsoft.AspNetCore.Mvc;
using Persistence.Repositories;

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
    }
}