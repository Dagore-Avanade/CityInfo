using CityInfo.API.DbContexts;
using CityInfo.API.Entities;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using CityInfo.API.Models;

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
        
        public async Task<(IEnumerable<City>, PaginationMetadata)> GetCitiesAsync(string? name, string? search, int pageNumber, int pageSize)
        {
            // Cast to IQueryable to gain deferred execution.
            var collection = (IQueryable<City>)cityInfoContext.Cities;

            if (!string.IsNullOrWhiteSpace(name))
            {
                name = name.Trim();
                collection = collection
                    .Where(city => city.Name == name);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                collection = collection
                    .Where(city => city.Name.Contains(search) || (city.Description != null && city.Description.Contains(search)));
            }

            var totalItemCount = await collection.CountAsync();
            var paginationMetadata = new PaginationMetadata(totalItemCount, pageSize, pageNumber);

            // Until the call of ToListAsync no query is executed.
            var collectionToReturn = await collection
                .OrderBy(city => city.Name)
                .Skip(pageSize * (pageNumber - 1))
                .Take(pageSize)
                .ToListAsync();

            return (collectionToReturn, paginationMetadata);
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

        public async Task<User?> GetUserAsync(AuthenticationRequestBody authenticationRequestBody)
        {
            var user = await cityInfoContext.Users.SingleOrDefaultAsync(user => user.Username == authenticationRequestBody.UserName);
            if (user == null || !BCrypt.Net.BCrypt.Verify(authenticationRequestBody.Password, user.Password))
                return null;

            return user;
        }

        public async Task<User?> RegisterUserAsync(AuthenticationRequestBody authenticationRequestBody)
        {
            if (string.IsNullOrEmpty(authenticationRequestBody.UserName) || (await cityInfoContext.Users.AnyAsync(user => user.Username == authenticationRequestBody.UserName)))
                return null;

            var user = new User()
            {
                Username = authenticationRequestBody.UserName,
                Password = BCrypt.Net.BCrypt.HashPassword(authenticationRequestBody.Password)
            };
            await cityInfoContext.Users.AddAsync(user);
            await SaveChangesAsync();

            return new User
            {
                Username = authenticationRequestBody.UserName,
                Password = authenticationRequestBody.Password
            };
        }

        public void AddCity(City city)
        {
            cityInfoContext.Cities.Add(city);
        }
    }
}
