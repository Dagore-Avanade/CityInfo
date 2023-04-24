using CityInfo.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CityInfo.API.Controllers
{
    [Route("api/cities/{cityId}/pointsofinterest")]
    [ApiController]
    public class PointOfInterestController : ControllerBase
    {
        [HttpGet]
        public ActionResult<IEnumerable<PointOfInterestDTO>> GetPointOfInterestOfCity(int cityId)
        {
            if (CityDoesNotExist(cityId, out var city))
                return NotFound();

            return Ok(city.PointsOfInterest);
        }

        [HttpGet("{pointOfInterestId}", Name = nameof(GetPointOfInterest))]
        public ActionResult<PointOfInterestDTO> GetPointOfInterest(int cityId, int pointOfInterestId)
        {
            if (CityDoesNotExist(cityId, out var city))
                return NotFound();

            var pointOfInterest = city.PointsOfInterest.FirstOrDefault(pointOfInterest => pointOfInterest.Id == pointOfInterestId);
            if (pointOfInterest is null)
                return NotFound();
            
            return Ok(pointOfInterest);
        }

        [HttpPost]
        public ActionResult<PointOfInterestDTO> CreatePointOfInterest(int cityId, PointOfInterestCreationDTO pointOfInterest)
        {
            if (CityDoesNotExist(cityId, out var city))
                return NotFound();

            // demo purposes - to be improved
            var maxPointOfInterestId = CitiesDataStore.Current.Cities
                .SelectMany(city => city.PointsOfInterest)
                .Max(p => p.Id);

            var finalPointOfInterest = new PointOfInterestDTO()
            {
                Id = ++maxPointOfInterestId,
                Name = pointOfInterest.Name,
                Description = pointOfInterest.Description
            };

            city.PointsOfInterest.Add(finalPointOfInterest);
            return CreatedAtRoute(nameof(GetPointOfInterest),
                new
                {
                    cityId,
                    pointOfInterestId = finalPointOfInterest.Id
                }, finalPointOfInterest);
        }

        [HttpPut("{pointofinterestId}")]
        public ActionResult UpdatePointOfInterest(int cityId, int pointOfInterestId, PointOfInterestForUpdateDTO pointOfInterest)
        {
            if (CityDoesNotExist(cityId, out var city))
                return NotFound();

            var pointOfInterestFromStore = city.PointsOfInterest
                .FirstOrDefault(p => p.Id == pointOfInterestId);
            if (pointOfInterestFromStore is null)
                return NotFound();

            pointOfInterestFromStore.Name = pointOfInterest.Name;
            pointOfInterestFromStore.Description = pointOfInterest.Description;

            return NoContent();
        }

        static bool CityDoesNotExist(int cityId, out CityDTO city)
        {
            city = CitiesDataStore.Current.Cities.FirstOrDefault(city => city.Id == cityId);
            return city is null;
        }
    }
}
