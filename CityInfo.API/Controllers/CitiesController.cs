﻿using CityInfo.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace CityInfo.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CitiesController : ControllerBase
    {
        [HttpGet]
        public ActionResult<IEnumerable<CityDTO>> GetCities()
        {
            return Ok(CitiesDataStore.Current.Cities);
        }
        [HttpGet("{id}")]
        public ActionResult<CityDTO> GetCity(int id)
        {
            var city = CitiesDataStore.Current.Cities.FirstOrDefault(city => city.Id == id);
            if (city is null)
                return NotFound();

            return Ok(city);
        }
    }
}