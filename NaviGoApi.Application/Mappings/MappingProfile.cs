using AutoMapper;
using NaviGoApi.Application.DTOs.CargoType;
using NaviGoApi.Application.DTOs.Company;
using NaviGoApi.Application.DTOs.Contract;
using NaviGoApi.Application.DTOs.DelayPenalty;
using NaviGoApi.Application.DTOs.Driver;
using NaviGoApi.Application.DTOs.ForwarderOffer;
using NaviGoApi.Application.DTOs.Location;
using NaviGoApi.Application.DTOs.Payment;
using NaviGoApi.Application.DTOs.PickupChange;
using NaviGoApi.Application.DTOs.Route;
using NaviGoApi.Application.DTOs.RoutePrice;
using NaviGoApi.Application.DTOs.Shipment;
using NaviGoApi.Application.DTOs.ShipmentDocument;
using NaviGoApi.Application.DTOs.ShipmentStatusHistory;
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
			//User mappings
			CreateMap<UserCreateDto, User>();
			CreateMap<User, UserDto>()
				.ForMember(dest => dest.UserRole, opt => opt.MapFrom(src => src.UserRole.ToString()));
			CreateMap<UserSuperAdminCreateDto, User>()
				.ForMember(dest => dest.UserRole, opt => opt.MapFrom(src => UserRole.SuperAdmin))
				.ForMember(dest => dest.PasswordHash, opt => opt.Ignore()); 

			//Location mappings
			CreateMap<Location, LocationDto>().ReverseMap();
			CreateMap<Location, LocationCreateDto>().ReverseMap();
			CreateMap<Location, LocationUpdateDto>().ReverseMap();
			//Vehicle mappings
			CreateMap<VehicleType, VehicleTypeDto>().ReverseMap();
			CreateMap<VehicleType, VehicleTypeCreateDto>().ReverseMap();
			CreateMap<VehicleType, VehicleTypeUpdateDto>().ReverseMap();
			//CargoType mappings
			CreateMap<CargoType, CargoTypeDto>().ReverseMap();
			CreateMap<CargoType, CargoTypeCreateDto>().ReverseMap();
			CreateMap<CargoType, CargoTypeUpdateDto>().ReverseMap();

			// Company mappings
			CreateMap<Company, CompanyDto>()
				.ForMember(dest => dest.CompanyType, opt => opt.MapFrom(src => src.CompanyType.ToString()))
				.ForMember(dest => dest.CompanyStatus, opt => opt.MapFrom(src => src.CompanyStatus.ToString()));
			CreateMap<CompanyCreateDto, Company>();
			CreateMap<CompanyUpdateDto, Company>();
			//Vehicle mappings
			CreateMap<Vehicle, VehicleDto>()
				.ForMember(dest => dest.VehicleStatus, opt => opt.MapFrom(src => src.VehicleStatus.ToString()));
			CreateMap<VehicleCreateDto, Vehicle>();
			CreateMap<VehicleUpdateDto, Vehicle>();
			//VehicleMaintenance mappings
			CreateMap<VehicleMaintenance, VehicleMaintenanceDto>()
				.ForMember(dest => dest.Severity, opt => opt.MapFrom(src => src.Severity.ToString()))
				.ForMember(dest => dest.MaintenanceType, opt => opt.MapFrom(src => src.MaintenanceType.ToString()))
				.ForMember(dest => dest.ReportedByUserEmail, opt => opt.MapFrom(src => src.ReportedByUser != null ? src.ReportedByUser.Email : null));
			CreateMap<VehicleMaintenanceCreateDto, VehicleMaintenance>();
			CreateMap<VehicleMaintenanceUpdateDto, VehicleMaintenance>();
			//Driver mappings
			CreateMap<DriverCreateDto, Driver>();
			CreateMap<DriverUpdateDto, Driver>()
				.ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
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
			// RoutesPrices mappings
			CreateMap<RoutePriceCreateDto, RoutePrice>();
			CreateMap<RoutePriceUpdateDto, RoutePrice>();
			CreateMap<RoutePrice, RoutePriceDto>()
				.ForMember(dest => dest.VehicleTypeName, opt => opt.MapFrom(src => src.VehicleType != null ? src.VehicleType.TypeName : string.Empty));
			// Contract mappings
			CreateMap<ContractCreateDto, Contract>();
			CreateMap<ContractUpdateDto, Contract>()
				.ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
			CreateMap<Contract, ContractDto>()
				.ForMember(dest => dest.ClientFullName, opt => opt.MapFrom(src => src.Client != null ? $"{src.Client.FirstName} {src.Client.LastName}" : null))
				.ForMember(dest => dest.ForwarderCompanyName, opt => opt.MapFrom(src => src.Forwarder != null ? src.Forwarder.CompanyName : null))
				.ForMember(dest => dest.ContractStatus, opt => opt.MapFrom(src => src.ContractStatus.ToString()));
			// Payment mappings
			CreateMap<PaymentCreateDto, Payment>()
				.ForMember(dest => dest.PaymentStatus, opt => opt.Ignore());

			CreateMap<PaymentUpdateDto, Payment>();

			CreateMap<Payment, PaymentDto>()
				.ForMember(dest => dest.PaymentStatus, opt => opt.MapFrom(src => src.PaymentStatus.ToString()));
			// Forwarder Offer mappings
			CreateMap<ForwarderOfferCreateDto, ForwarderOffer>();
			CreateMap<ForwarderOfferUpdateDto, ForwarderOffer>()
				.ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null)); 
			CreateMap<ForwarderOffer, ForwarderOfferDto>()
				.ForMember(dest => dest.ForwarderOfferStatus, opt => opt.MapFrom(src => src.ForwarderOfferStatus.ToString()))
				.ForMember(dest => dest.RouteName, opt => opt.MapFrom(src => src.Route != null ? $"{src.Route.StartLocation.FullAddress} - {src.Route.EndLocation.FullAddress}" : null))
				.ForMember(dest => dest.ForwarderCompanyName, opt => opt.MapFrom(src => src.Forwarder != null ? src.Forwarder.CompanyName : null));
			// Shipment mappings
			CreateMap<Shipment, ShipmentDto>()
				.ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

			CreateMap<ShipmentCreateDto, Shipment>();    
			CreateMap<ShipmentUpdateDto, Shipment>();
			// Shipment Document mappings
			CreateMap<ShipmentDocumentCreateDto, ShipmentDocument>();

			CreateMap<ShipmentDocumentUpdateDto, ShipmentDocument>();

			CreateMap<ShipmentDocument, ShipmentDocumentDto>()
				.ForMember(dest => dest.DocumentType, opt => opt.MapFrom(src => src.DocumentType.ToString()));
			// ShipmentStatusHistory mappings
			CreateMap<ShipmentStatusHistoryCreateDto, ShipmentStatusHistory>();

			CreateMap<ShipmentStatusHistoryUpdateDto, ShipmentStatusHistory>();

			CreateMap<ShipmentStatusHistory, ShipmentStatusHistoryDto>()
				.ForMember(dest => dest.ShipmentStatus, opt => opt.MapFrom(src => src.ShipmentStatus.ToString()));
			// PickupChange mappings
			CreateMap<PickupChangeCreateDto, PickupChange>();
			CreateMap<PickupChangeUpdateDto, PickupChange>();
			CreateMap<PickupChange, PickupChangeDto>();
			// DelayPenalty mappings
			CreateMap<DelayPenaltyCreateDto, DelayPenalty>();
			CreateMap<DelayPenaltyUpdateDto, DelayPenalty>();
			CreateMap<DelayPenalty, DelayPenaltyDto>()
				.ForMember(dest => dest.DelayPenaltiesStatus, opt => opt.MapFrom(src => src.DelayPenaltiesStatus.ToString()));

		}
	}
}
