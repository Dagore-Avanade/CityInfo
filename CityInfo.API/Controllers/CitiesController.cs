using AutoMapper;
using CityInfo.API.Models;
using CityInfo.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace CityInfo.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CitiesController : ControllerBase
    {
        private readonly ICityInfoRepository cityInfoRepository;
        private readonly IMapper mapper;
        const int maxCitiesPageSize = 20;

        public CitiesController(ICityInfoRepository cityInfoRepository, IMapper mapper)
        {
            this.cityInfoRepository = cityInfoRepository ?? throw new ArgumentNullException(nameof(cityInfoRepository));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CityWithoutPointsOfInterestDTO>>> GetCities(string? name, string? search, int pageNumber = 1, int pageSize = 10)
        {
            if (pageSize > maxCitiesPageSize)
                pageSize = maxCitiesPageSize;
            var (cities, paginationMetadata) = await cityInfoRepository
                .GetCitiesAsync(name, search, pageNumber, pageSize);

            Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(paginationMetadata));
            return Ok(mapper.Map<IEnumerable<CityWithoutPointsOfInterestDTO>>(cities));
        }

        [HttpGet("{id}", Name = nameof(GetCity))]
        public async Task<IActionResult> GetCity(int id, bool includePointsOfInterest = false)
        {
            var city = await cityInfoRepository.GetCityAsync(id, includePointsOfInterest);
            if (city is null)
                return NotFound();

            if (includePointsOfInterest)
                return Ok(mapper.Map<CityDTO>(city));

            return Ok(mapper.Map<CityWithoutPointsOfInterestDTO>(city));
        }

        [HttpPost]
        public async Task<ActionResult<CityCreatedDTO>> CreateCity(CityCreationDTO city)
        {
            var finalCity = mapper.Map<Entities.City>(city);
            cityInfoRepository.AddCity(finalCity);
            await cityInfoRepository.SaveChangesAsync();
            var createdCity = mapper.Map<CityCreatedDTO>(finalCity);

            return CreatedAtRoute(nameof(GetCity), new
            {
                id = finalCity.Id
            }, createdCity);
        }
    }
}
