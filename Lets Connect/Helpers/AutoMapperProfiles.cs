using AutoMapper;
using Lets_Connect.Data.DTO;
using Lets_Connect.Extensions;
using Lets_Connect.Model;

namespace Lets_Connect.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        { 
            CreateMap<User, MemberDto>()
                .ForMember(d => d.Age, o => o.MapFrom(s => s.DateOfBirth.CalculateAge()))
                .ForMember(d => d.PhotoUrl, o => o.MapFrom(s => s.Photos.FirstOrDefault(x => x.IsMain)!.Url));
            CreateMap<Photo, PhotoDto>();
            CreateMap<MemberUpdateDto, User>();
            CreateMap<RegisterDto, User>();
            CreateMap<string, DateOnly>().ConvertUsing(s => DateOnly.Parse(s));
        }

    }
}
