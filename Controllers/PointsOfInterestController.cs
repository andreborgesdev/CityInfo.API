using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.JsonPatch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CityInfo.API.Models;
using Microsoft.Extensions.Logging;
using CityInfo.API.Services;
using AutoMapper;

namespace CityInfo.API.Controllers
{
    [Route("api/cities")]
    public class PointsOfInterestController : Controller
    {
        private ILogger<PointsOfInterestController> _logger;
        private IMailService _mailService;
        private ICityInfoRepository _cityInfoRepository;
        private readonly IMapper _mapper;

        public PointsOfInterestController(ILogger<PointsOfInterestController> logger,
            IMailService mailService, 
            ICityInfoRepository cityInfoRepository,
            IMapper mapper)
        {
            _logger = logger;
            _mailService = mailService;
            _cityInfoRepository = cityInfoRepository;
            _mapper = mapper;
        }

        [HttpGet("{cityId}/pointsofinterest")]
        public IActionResult GetPointsOfInterest(int cityId)
        {
            try
            {
                //var cityToReturn = CitiesDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);

                //return Ok(cityToReturn.PointsOfInterest);

                //var pointsOfInterestForCityResults = new List<PointOfInterestDto>();
                //foreach (var poi in pointsOfInterestForCity)
                //{
                //    pointsOfInterestForCityResults.Add(new PointOfInterestDto()
                //    {
                //        Id = poi.Id,
                //        Name = poi.Name,
                //        Description = poi.Description
                //    });
                //}

                if (!_cityInfoRepository.CityExists(cityId))
                {
                    _logger.LogInformation($"City with ID {cityId} wasn't found when accessing points of interest");
                    return NotFound();
                }

                var pointsOfInterestForCity = _cityInfoRepository.GetPointsOfInterestForCity(cityId);

                var pointsOfInterestForCityResults = _mapper.Map<IEnumerable<PointOfInterestDto>>(pointsOfInterestForCity);

                return Ok(pointsOfInterestForCityResults);
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"City with ID {cityId} wasn't found when accessing points of interest", ex);
                return StatusCode(500, "A problem happened while handling your request");
            }
        }

        [HttpGet("{cityId}/pointsofinterest/{pointOfInterestId}", Name = "GetPointOfInterest")]
        public IActionResult GetPointOfInterest(int cityId, int pointOfInterestId)
        {
            //var cityToReturn = CitiesDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);

            //if (cityToReturn == null)
            //{
            //    return NotFound();
            //}

            //var pointOfInterest = cityToReturn.PointsOfInterest.FirstOrDefault(p => p.Id == pointOfInterestId);

            //if (pointOfInterest == null)
            //{
            //    return NotFound();
            //}

            //return Ok(pointOfInterest);



            //var pointOfInterestResult = new PointOfInterestDto()
            //{
            //    Id = pointOfInterest.Id,
            //    Name = pointOfInterest.Name,
            //    Description = pointOfInterest.Description
            //};

            if (!_cityInfoRepository.CityExists(cityId))
            {
                _logger.LogInformation($"City with ID {cityId} wasn't found when accessing points of interest");
                return NotFound();
            }

            var pointOfInterest = _cityInfoRepository.GetPointOfInterestForCity(cityId, pointOfInterestId);

            if (pointOfInterest == null)
            {
                return NotFound();
            }

            var pointOfInterestResult = _mapper.Map<PointOfInterestDto>(pointOfInterest);

            return Ok(pointOfInterestResult);
        }

        [HttpPost("{cityId}/pointsofinterest")]
        public IActionResult CreatePointOfInterest(int cityId, 
            [FromBody] PointOfInterestForCreationDto pointOfInterest)
        {
            //var cityToReturn = CitiesDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);

            //var maxPointOfInterestId = CitiesDataStore.Current.Cities.SelectMany(
            //    c => c.PointsOfInterest).Max(p => p.Id);

            //var finalPointOfInterest = new PointOfInterestDto()
            //{
            //    Id = ++maxPointOfInterestId,
            //    Name = pointsOfInterest.Name,
            //    Description = pointsOfInterest.Description
            //};

            //cityToReturn.PointsOfInterest.Add(finalPointOfInterest);

            if (pointOfInterest == null)
            {
                return BadRequest();
            }

            if (pointOfInterest.Description == pointOfInterest.Name)
            {
                ModelState.AddModelError("Description", "The provided description should be different from the name.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!_cityInfoRepository.CityExists(cityId))
            {
                return NotFound();
            }

            var finalPointOfInterest = _mapper.Map<Entities.PointOfInterest>(pointOfInterest);

            _cityInfoRepository.AddPointOfInterestForCity(cityId, finalPointOfInterest);

            if (!_cityInfoRepository.Save())
            {
                return StatusCode(500, "A problem happened while handline your request.");
            }

            var createdPointOfInterestToReturn = _mapper.Map<Models.PointOfInterestDto>(finalPointOfInterest);

            return CreatedAtRoute("GetPointOfInterest", new
            { cityId = cityId, pointOfInterestId = createdPointOfInterestToReturn.Id }, createdPointOfInterestToReturn);
        }

        [HttpPut("{cityId}/pointsofinterest/{pointOfInterestId}")]
        public IActionResult UpdatePointOfInterest(int cityId, int pointOfInterestId,
            [FromBody] PointsOfInterestForUpdateDto pointOfInterest)
        {
            //var cityToReturn = CitiesDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);

            //if (cityToReturn == null)
            //{
            //    return NotFound();
            //}

            //var pointOfInterestFromStore = cityToReturn.PointsOfInterest.FirstOrDefault(p => p.Id == pointOfInterestId);

            //if (pointOfInterestFromStore == null)
            //{
            //    return NotFound();
            //}

            //pointOfInterestFromStore.Name = pointsOfInterest.Name;
            //pointOfInterestFromStore.Description = pointsOfInterest.Description;

            if (pointOfInterest == null)
            {
                return BadRequest();
            }

            if (pointOfInterest.Description == pointOfInterest.Name)
            {
                ModelState.AddModelError("Description", "The provided description should be different from the name.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!_cityInfoRepository.CityExists(cityId))
            {
                return NotFound();
            }

            var pointOfInterestEntity = _cityInfoRepository.GetPointOfInterestForCity(cityId, pointOfInterestId);

            if (pointOfInterestEntity == null)
            {
                return NotFound();
            }

            _mapper.Map(pointOfInterest, pointOfInterestEntity);

            if (!_cityInfoRepository.Save())
            {
                return StatusCode(500, "A problem happened while handline your request.");
            }

            return NoContent();
        }

        [HttpPatch("{cityId}/pointsofinterest/{pointOfInterestId}")]
        public IActionResult PartiallyUpdatePointOfInterest(int cityId, int pointOfInterestId,
            [FromBody] JsonPatchDocument<PointsOfInterestForUpdateDto> patchDoc)
        {
            //var cityToReturn = CitiesDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);

            //if (cityToReturn == null)
            //{
            //    return NotFound();
            //}

            //var pointOfInterestFromStore = cityToReturn.PointsOfInterest.FirstOrDefault(p => p.Id == pointOfInterestId);

            //if (pointOfInterestFromStore == null)
            //{
            //    return NotFound();
            //}

            //var pointOfInterestToPatch =
            //    new PointsOfInterestForUpdateDto()
            //    {
            //        Name = pointOfInterestFromStore.Name,
            //        Description = pointOfInterestFromStore.Description
            //    };

            //pointOfInterestFromStore.Name = pointOfInterestToPatch.Name;
            //pointOfInterestFromStore.Description = pointOfInterestToPatch.Description;

            if (patchDoc == null)
            {
                return BadRequest();
            }

            if (!_cityInfoRepository.CityExists(cityId))
            {
                return NotFound();
            }

            var pointsOfInterestEntity = _cityInfoRepository.GetPointOfInterestForCity(cityId, pointOfInterestId);

            if (pointsOfInterestEntity == null)
            {
                return NotFound();
            }

            var pointOfInterestToPatch = _mapper.Map<PointsOfInterestForUpdateDto>(pointsOfInterestEntity);

            patchDoc.ApplyTo(pointOfInterestToPatch, ModelState);

            if (pointOfInterestToPatch.Description == pointOfInterestToPatch.Name)
            {
                ModelState.AddModelError("Description", "The provided description should be different from the name.");
            }

            TryValidateModel(pointOfInterestToPatch);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _mapper.Map(pointOfInterestToPatch, pointsOfInterestEntity);

            if (!_cityInfoRepository.Save())
            {
                return StatusCode(500, "A problem happened while handline your request.");
            }

            return NoContent();
        }

        [HttpDelete("{cityId}/pointsofinterest/{pointOfInterestId}")]
        public IActionResult DeletePointOfInterest(int cityId, int pointOfInterestId)
        {
            //var cityToReturn = CitiesDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);

            //if (cityToReturn == null)
            //{
            //    return NotFound();
            //}

            //var pointOfInterestFromStore = cityToReturn.PointsOfInterest.FirstOrDefault(p => p.Id == pointOfInterestId);

            //if (pointOfInterestFromStore == null)
            //{
            //    return NotFound();
            //}

            //cityToReturn.PointsOfInterest.Remove(pointOfInterestFromStore);

            if (!_cityInfoRepository.CityExists(cityId))
            {
                return NotFound();
            }

            var pointsOfInterestEntity = _cityInfoRepository.GetPointOfInterestForCity(cityId, pointOfInterestId);

            if (pointsOfInterestEntity == null)
            {
                return NotFound();
            }

            _cityInfoRepository.DeletePointOfInterest(pointsOfInterestEntity);

            if (!_cityInfoRepository.Save())
            {
                return StatusCode(500, "A problem happened while handline your request.");
            }

            _mailService.Send("Points of interest deleted.",
                $"Point of interest {pointsOfInterestEntity.Name} with ID {pointsOfInterestEntity.Id} was deleted.");

            return NoContent();
        }
    }
}
