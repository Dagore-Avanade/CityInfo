using CityInfo.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
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

        [HttpGet("{pointofinterestid}", Name = nameof(GetPointOfInterest))]
        public ActionResult<PointOfInterestDTO> GetPointOfInterest(int cityId, int pointOfInterestId)
        {
            if (CityDoesNotExist(cityId, out var city))
                return NotFound();

            if (PointOfInterestDoesNotExist(city, pointOfInterestId, out var pointOfInterestFromStore))
                return NotFound();

            return Ok(pointOfInterestFromStore);
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

        [HttpPut("{pointofinterestid}")]
        public ActionResult UpdatePointOfInterest(int cityId, int pointOfInterestId, PointOfInterestForUpdateDTO pointOfInterest)
        {
            if (CityDoesNotExist(cityId, out var city))
                return NotFound();

            if (PointOfInterestDoesNotExist(city, pointOfInterestId, out var pointOfInterestFromStore))
                return NotFound();

            pointOfInterestFromStore.Name = pointOfInterest.Name;
            pointOfInterestFromStore.Description = pointOfInterest.Description;

            return NoContent();
        }

        [HttpPatch("{pointofinterestid}")]
        public ActionResult PartiallyUpdatePointOfInterest(int cityId, int pointOfInterestId, JsonPatchDocument<PointOfInterestForUpdateDTO> patchDocument)
        {
            if (CityDoesNotExist(cityId, out var city))
                return NotFound();

            if (PointOfInterestDoesNotExist(city, pointOfInterestId, out var pointOfInterestFromStore))
                return NotFound();

            var pointOfInterestToPatch = new PointOfInterestForUpdateDTO()
            {
                Name = pointOfInterestFromStore.Name,
                Description = pointOfInterestFromStore.Description
            };

            patchDocument.ApplyTo(pointOfInterestToPatch, ModelState);
            // This validation only applies to the JsonPatchDocument, so we need to check again using TryValidateModel on the result of the patch ApplyTo operation.
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!TryValidateModel(pointOfInterestToPatch))
                return BadRequest(ModelState);

            pointOfInterestFromStore.Name = pointOfInterestToPatch.Name;
            pointOfInterestFromStore.Description = pointOfInterestToPatch.Description;

            return NoContent();
        }

        [HttpDelete("{pointofinterestid}")]
        public ActionResult DeletePointOfInterest(int cityId, int pointOfInterestId)
        {
            if (CityDoesNotExist(cityId, out var city))
                return NotFound();

            if (PointOfInterestDoesNotExist(city, pointOfInterestId, out var pointOfInterestFromStore))
                return NotFound();

            city.PointsOfInterest.Remove(pointOfInterestFromStore);
            return NoContent();
        }

        static bool CityDoesNotExist(int cityId, out CityDTO? city)
        {
            city = CitiesDataStore.Current.Cities.FirstOrDefault(city => city.Id == cityId);
            return city is null;
        }
        static bool PointOfInterestDoesNotExist(CityDTO? city, int pointOfInterestId, out PointOfInterestDTO? pointOfInterestFromStore)
        {
            pointOfInterestFromStore = city?.PointsOfInterest
                .FirstOrDefault(p => p.Id == pointOfInterestId);
            
            return pointOfInterestFromStore is null;
        }
    }
}
