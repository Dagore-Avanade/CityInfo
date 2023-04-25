using AutoMapper;
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
        private readonly ICityInfoRepository cityInfoRepository;
        private readonly IMapper mapper;

        public PointOfInterestController(ILogger<PointOfInterestController> logger, IMailService mailService, ICityInfoRepository cityInfoRepository, IMapper mapper)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.mailService = mailService ?? throw new ArgumentNullException(nameof(mailService));
            this.cityInfoRepository = cityInfoRepository ?? throw new ArgumentNullException(nameof(cityInfoRepository));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PointOfInterestDTO>>> GetPointOfInterestOfCity(int cityId)
        {
            if (!(await cityInfoRepository.CityExistsAsync(cityId)))
            {
                logger.LogInformation($"City with id {cityId} wasn't found when accessing points of interest.");
                return NotFound();
            }

            var pointsOfInterestForCity = await cityInfoRepository.GetPointsOfInterestForCityAsync(cityId);
            
            return Ok(mapper.Map<IEnumerable<PointOfInterestDTO>>(pointsOfInterestForCity));
        }

        [HttpGet("{pointofinterestid}", Name = nameof(GetPointOfInterest))]
        public async Task<ActionResult<PointOfInterestDTO>> GetPointOfInterest(int cityId, int pointOfInterestId)
        {
            if (!(await cityInfoRepository.CityExistsAsync(cityId)))
                return NotFound();

            var pointOfInterest = await cityInfoRepository.GetPointOfInterestForCityAsync(cityId, pointOfInterestId);
            if (pointOfInterest is null)
                return NotFound();

            return Ok(mapper.Map<PointOfInterestDTO>(pointOfInterest));
        }

        [HttpPost]
        public async Task<ActionResult<PointOfInterestDTO>> CreatePointOfInterest(int cityId, PointOfInterestCreationDTO pointOfInterest)
        {
            if (!(await cityInfoRepository.CityExistsAsync(cityId)))
                return NotFound();

            var finalPointOfInterest = mapper.Map<Entities.PointOfInterest>(pointOfInterest);
            await cityInfoRepository.AddPointOfInterestForCityAsync(cityId, finalPointOfInterest);
            await cityInfoRepository.SaveChangesAsync();
            var createdPointOfInterestToReturn = mapper.Map<PointOfInterestDTO>(finalPointOfInterest);

            return CreatedAtRoute(nameof(GetPointOfInterest), new
            {
                cityId,
                pointOfInterestId = createdPointOfInterestToReturn.Id
            }, createdPointOfInterestToReturn);
        }

        [HttpPut("{pointofinterestid}")]
        public async Task<ActionResult> UpdatePointOfInterest(int cityId, int pointOfInterestId, PointOfInterestForUpdateDTO pointOfInterest)
        {
            if (!(await cityInfoRepository.CityExistsAsync(cityId)))
                return NotFound();

            var pointOfInterestFromDB = await cityInfoRepository.GetPointOfInterestForCityAsync(cityId, pointOfInterestId);
            if (pointOfInterestFromDB is null)
                return NotFound();

            mapper.Map(pointOfInterest, pointOfInterestFromDB);
            await cityInfoRepository.SaveChangesAsync();

            return NoContent();
        }

        [HttpPatch("{pointofinterestid}")]
        public async Task<ActionResult> PartiallyUpdatePointOfInterest(int cityId, int pointOfInterestId, JsonPatchDocument<PointOfInterestForUpdateDTO> patchDocument)
        {
            if (!(await cityInfoRepository.CityExistsAsync(cityId)))
                return NotFound();

            var pointOfInterestFromDB = await cityInfoRepository.GetPointOfInterestForCityAsync(cityId, pointOfInterestId);
            if (pointOfInterestFromDB is null)
                return NotFound();

            var pointOfInterestToPatch = mapper.Map<PointOfInterestForUpdateDTO>(pointOfInterestFromDB);
            patchDocument.ApplyTo(pointOfInterestToPatch, ModelState);

            if (!ModelState.IsValid || !TryValidateModel(pointOfInterestToPatch))
                return BadRequest(ModelState);

            mapper.Map(pointOfInterestToPatch, pointOfInterestFromDB);
            await cityInfoRepository.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{pointofinterestid}")]
        public async Task<ActionResult> DeletePointOfInterest(int cityId, int pointOfInterestId)
        {
            if (!(await cityInfoRepository.CityExistsAsync(cityId)))
                return NotFound();

            var pointOfInterestFromDB = await cityInfoRepository.GetPointOfInterestForCityAsync(cityId, pointOfInterestId);
            if (pointOfInterestFromDB is null)
                return NotFound();

            cityInfoRepository.DeletePointOfInterest(pointOfInterestFromDB);
            await cityInfoRepository.SaveChangesAsync();

            mailService.Send("Point of interest deleted", $"Point of interest {pointOfInterestFromDB.Name} with id {pointOfInterestFromDB.Id} was deleted.");

            return NoContent();
        }
    }
}
