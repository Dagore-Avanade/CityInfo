using CityInfo.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace CityInfo.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CitiesController : ControllerBase
    {
        private readonly CitiesDataStore citiesDataStore;

        public CitiesController(CitiesDataStore citiesDataStore)
        {
            this.citiesDataStore = citiesDataStore ?? throw new ArgumentNullException(nameof(citiesDataStore));
        }

        [HttpGet]
        public ActionResult<IEnumerable<CityDTO>> GetCities()
        {
            return Ok(citiesDataStore.Cities);
        }

        [HttpGet("{id}")]
        public ActionResult<CityDTO> GetCity(int id)
        {
            var city = citiesDataStore.Cities.FirstOrDefault(city => city.Id == id);
            if (city is null)
                return NotFound();

            return Ok(city);
        }
    }
}
