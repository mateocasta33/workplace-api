using AutoMapper;
using workplace.Domain.Entities;

namespace workplace.Application.Dtos;

public class MapProfile : Profile
{
    public MapProfile()
    {
        CreateMap<UserRegisterRequestDto, User>()
            .ForMember(
                dest => dest.Name,
                opt => opt.MapFrom(src => src.name))
            .ForMember(
                dest => dest.Email,
                opt => opt.MapFrom(src => src.email))
            .ForMember(
                dest => dest.RoleValue,
                opt => opt.MapFrom(src => src.role));

        CreateMap<User, UserLoginResponseDto>();
        CreateMap<User, UserResponseDto>();
    }
}