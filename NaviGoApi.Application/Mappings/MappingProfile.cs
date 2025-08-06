using AutoMapper;
using NaviGoApi.Application.DTOs.User;
using NaviGoApi.Domain.Entities;

namespace NaviGoApi.Application.MappingProfiles
{
	public class MappingProfile : Profile
	{
		public MappingProfile()
		{
			// DTO -> Entity
			CreateMap<UserCreateDto, User>();

			// Entity -> DTO (konvertujemo enum u string)
			CreateMap<User, UserDto>()
				.ForMember(dest => dest.UserRole, opt => opt.MapFrom(src => src.UserRole.ToString()))
				.ForMember(dest => dest.UserStatus, opt => opt.MapFrom(src => src.UserStatus.ToString()));
		}
	}
}
