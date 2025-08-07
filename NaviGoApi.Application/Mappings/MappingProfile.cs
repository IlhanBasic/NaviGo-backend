using AutoMapper;
using NaviGoApi.Application.DTOs.CargoType;
using NaviGoApi.Application.DTOs.Company;
using NaviGoApi.Application.DTOs.Location;
using NaviGoApi.Application.DTOs.User;
using NaviGoApi.Application.DTOs.Vehicle;
using NaviGoApi.Application.DTOs.VehicleType;
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

			CreateMap<Location, LocationDto>().ReverseMap();
			CreateMap<Location, LocationCreateDto>().ReverseMap();
			CreateMap<Location, LocationUpdateDto>().ReverseMap();

			CreateMap<VehicleType, VehicleTypeDto>().ReverseMap();
			CreateMap<VehicleType, VehicleTypeCreateDto>().ReverseMap();
			CreateMap<VehicleType, VehicleTypeUpdateDto>().ReverseMap();

			CreateMap<CargoType, CargoTypeDto>().ReverseMap();
			CreateMap<CargoType, CargoTypeCreateDto>().ReverseMap();
			CreateMap<CargoType, CargoTypeUpdateDto>().ReverseMap();

			// Company mappings
			CreateMap<Company, CompanyDto>()
				.ForMember(dest => dest.CompanyType, opt => opt.MapFrom(src => src.CompanyType.ToString()))
				.ForMember(dest => dest.CompanyStatus, opt => opt.MapFrom(src => src.CompanyStatus.ToString()));

			CreateMap<CompanyCreateDto, Company>();

			CreateMap<CompanyUpdateDto, Company>();
			//Vehicle
			CreateMap<Vehicle, VehicleDto>()
				.ForMember(dest => dest.VehicleStatus, opt => opt.MapFrom(src => src.VehicleStatus.ToString()));

			CreateMap<VehicleCreateDto, Vehicle>();
			CreateMap<VehicleUpdateDto, Vehicle>();

		}
	}
}
