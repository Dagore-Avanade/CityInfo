using AutoMapper;

namespace CityInfo.API.Profiles
{
    public class CityProfile : Profile
    {
        public CityProfile()
        {
            CreateMap<Entities.City, Models.CityWithoutPointsOfInterestDTO>();
            CreateMap<Entities.City, Models.CityDTO>();
            CreateMap<Models.CityCreationDTO, Entities.City>();
            CreateMap<Entities.City, Models.CityCreatedDTO>();
        }
    }
}
