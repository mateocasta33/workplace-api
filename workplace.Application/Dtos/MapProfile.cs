using AutoMapper;
using workplace.Domain.Entities;

namespace workplace.Application.Dtos;

public class MapProfile : Profile
{
    public MapProfile()
    {
        // User mapper
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
        
        
        // Place mapper
        CreateMap<PlaceCreateDto, Place>()
            .ForMember(dest => dest.Name,
                otp => otp.MapFrom(src => src.name))
            .ForMember(dest => dest.Description,
                otp => otp.MapFrom(src => src.description))
            .ForMember(dest => dest.Capacity,
                otp => otp.MapFrom(src => src.capacity))
            .ForMember(dest => dest.IsActive,
                otp => otp.MapFrom(src => src.isActive));

        CreateMap<Place, PlaceResponseDto>();
    }
}