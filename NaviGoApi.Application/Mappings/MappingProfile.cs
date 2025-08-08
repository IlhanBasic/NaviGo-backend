using AutoMapper;
using NaviGoApi.Application.DTOs.CargoType;
using NaviGoApi.Application.DTOs.Company;
using NaviGoApi.Application.DTOs.Driver;
using NaviGoApi.Application.DTOs.Location;
using NaviGoApi.Application.DTOs.Route;
using NaviGoApi.Application.DTOs.User;
using NaviGoApi.Application.DTOs.Vehicle;
using NaviGoApi.Application.DTOs.VehicleMaintenance;
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
			//VehicleMaintenance
			CreateMap<VehicleMaintenance, VehicleMaintenanceDto>()
				.ForMember(dest => dest.Severity, opt => opt.MapFrom(src => src.Severity.ToString()))
				.ForMember(dest => dest.MaintenanceType, opt => opt.MapFrom(src => src.MaintenanceType.ToString()))
				.ForMember(dest => dest.ReportedByUserEmail, opt => opt.MapFrom(src => src.ReportedByUser != null ? src.ReportedByUser.Email : null));

			CreateMap<VehicleMaintenanceCreateDto, VehicleMaintenance>();
			CreateMap<VehicleMaintenanceUpdateDto, VehicleMaintenance>();
			//Driver

			CreateMap<DriverCreateDto, Driver>();

			// Mapiranje DriverUpdateDto -> Driver (enum mapira direktno, ignorisati null vrednosti)
			CreateMap<DriverUpdateDto, Driver>()
				.ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

			// Mapiranje Driver -> DriverDto (enum DriverStatus u string)
			CreateMap<Driver, DriverDto>()
				.ForMember(dest => dest.DriverStatus,
					opt => opt.MapFrom(src => src.DriverStatus.ToString()));
			// Route mappings
			CreateMap<RouteCreateDto, Route>()
				.ForMember(dest => dest.DistanceKm, opt => opt.Ignore())
				.ForMember(dest => dest.EstimatedDurationHours, opt => opt.Ignore())
				.ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));

			CreateMap<RouteUpdateDto, Route>()
				.ForMember(dest => dest.DistanceKm, opt => opt.Ignore())
				.ForMember(dest => dest.EstimatedDurationHours, opt => opt.Ignore())
				.ForMember(dest => dest.CreatedAt, opt => opt.Ignore()); 

			CreateMap<Route, RouteDto>()
				.ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Company != null ? src.Company.CompanyName : null))
				.ForMember(dest => dest.StartLocationName, opt => opt.MapFrom(src => src.StartLocation != null ? src.StartLocation.FullAddress : null))
				.ForMember(dest => dest.EndLocationName, opt => opt.MapFrom(src => src.EndLocation != null ? src.EndLocation.FullAddress : null));
		}
	}
}
