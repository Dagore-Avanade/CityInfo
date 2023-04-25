using CityInfo.API.Models;
using CityInfo.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace CityInfo.API.Controllers
{
    [Route("api/cities/{cityId}/pointsofinterest")]
    [ApiController]
    public class PointOfInterestController : ControllerBase
    {
        private readonly ILogger<PointOfInterestController> logger;
        private readonly IMailService mailService;
        private readonly CitiesDataStore citiesDataStore;

        public PointOfInterestController(ILogger<PointOfInterestController> logger, IMailService mailService, CitiesDataStore citiesDataStore)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.mailService = mailService ?? throw new ArgumentNullException(nameof(mailService));
            this.citiesDataStore = citiesDataStore ?? throw new ArgumentNullException(nameof(citiesDataStore));
        }

        [HttpGet]
        public ActionResult<IEnumerable<PointOfInterestDTO>> GetPointOfInterestOfCity(int cityId)
        {
            if (CityDoesNotExist(cityId, out var city))
            {
                logger.LogInformation($"City with id {cityId} wasn't found when accessing points of interest.");
                return NotFound();
            }

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
            var maxPointOfInterestId = citiesDataStore.Cities
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
            mailService.Send("Point of interest deleted", $"Point of interest {pointOfInterestFromStore.Name} with id {pointOfInterestFromStore.Id} was deleted.");
            return NoContent();
        }

        bool CityDoesNotExist(int cityId, out CityDTO? city)
        {
            city = citiesDataStore.Cities.FirstOrDefault(city => city.Id == cityId);
            return city is null;
        }
        bool PointOfInterestDoesNotExist(CityDTO? city, int pointOfInterestId, out PointOfInterestDTO? pointOfInterestFromStore)
        {
            pointOfInterestFromStore = city?.PointsOfInterest
                .FirstOrDefault(p => p.Id == pointOfInterestId);
            
            return pointOfInterestFromStore is null;
        }
    }
}
