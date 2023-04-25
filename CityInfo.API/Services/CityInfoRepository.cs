﻿using CityInfo.API.DbContexts;
using CityInfo.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace CityInfo.API.Services
{
    public class CityInfoRepository : ICityInfoRepository
    {
        private readonly CityInfoContext cityInfoContext;

        public CityInfoRepository(CityInfoContext cityInfoContext)
        {
            this.cityInfoContext = cityInfoContext ?? throw new ArgumentNullException(nameof(cityInfoContext));
        }

        public async Task<IEnumerable<City>> GetCitiesAsync()
        {
            return await cityInfoContext.Cities
                .OrderBy(city => city.Name)
                .ToListAsync();
        }
        
        public async Task<IEnumerable<City>> GetCitiesAsync(string? name)
        {
            if (name is null)
                return await GetCitiesAsync();

            name = name.Trim();
            return await cityInfoContext.Cities
                .Where(city => city.Name == name)
                .OrderBy(city => city.Name)
                .ToListAsync();
        }

        public async Task<City?> GetCityAsync(int cityId, bool includePointsOfInterest)
        {
            if (includePointsOfInterest)
                return await cityInfoContext.Cities
                    .Include(city => city.PointsOfInterest)
                    .Where(city => city.Id == cityId)
                    .FirstOrDefaultAsync();

            return await cityInfoContext.Cities
                .Where(city => city.Id == cityId)
                .FirstOrDefaultAsync();
        }

        public async Task<PointOfInterest?> GetPointOfInterestForCityAsync(int cityId, int pointOfInterestId)
        {
            return await cityInfoContext.PointsOfInterest
                .Where(pointOfInterest => pointOfInterest.CityId == cityId && pointOfInterest.Id == pointOfInterestId)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<PointOfInterest>> GetPointsOfInterestForCityAsync(int cityId)
        {
            return await cityInfoContext.PointsOfInterest
                .Where(pointOfInterest => pointOfInterest.CityId == cityId)
                .ToListAsync();
        }

        public async Task<bool> CityExistsAsync(int cityId)
        {
            return await cityInfoContext.Cities
                .AnyAsync(city => city.Id == cityId);
        }

        public async Task AddPointOfInterestForCityAsync(int cityId, PointOfInterest pointOfInterest)
        {
            var city = await GetCityAsync(cityId, false);
            // Not async because isn't going to the database right now.
            city?.PointsOfInterest.Add(pointOfInterest);
        }
        
        public async Task<bool> SaveChangesAsync()
        {
            return (await cityInfoContext.SaveChangesAsync() >= 0);
        }

        public void DeletePointOfInterest(PointOfInterest pointOfInterest)
        {
            cityInfoContext.PointsOfInterest.Remove(pointOfInterest);
        }
    }
}