
using Microsoft.EntityFrameworkCore.Storage;
using NaviGoApi.Domain.Interfaces;
using NaviGoApi.Infrastructure.Postgresql.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Infrastructure.Postgresql.Repositories
{
	public class UnitOfWork : IUnitOfWork
	{
		private readonly ApplicationDbContext _context;
		public ICompanyRepository Companies { get; }

		public IUserRepository Users { get; }

		public IContractRepository Contracts { get; }

		public IDriverRepository Drivers { get; }

		public IForwarderOfferRepository ForwarderOffers { get; }

		public IRouteRepository Routes { get; }

		public IShipmentRepository Shipments { get; }

		public IPaymentRepository Payments { get; }

		public IVehicleRepository Vehicles { get; }

		public ILocationRepository Locations { get; }

		public IVehicleTypeRepository VehicleTypes  { get; }

	public IShipmentDocumentRepository ShipmentDocuments { get; }

	public IShipmentStatusHistoryRepository ShipmentStatusHistories { get; }

		public IPickupChangeRepository PickupChanges { get; }

		public IVehicleMaintenanceRepository VehicleMaintenances { get; }

		public ICargoTypeRepository CargoTypes { get; }

		public IDelayPenaltyRepository DelayPenalties { get; }

		public IRoutePriceRepository RoutePrices { get; }
		public UnitOfWork(
			ApplicationDbContext context,
			ICompanyRepository companyRepository, 
			ICargoTypeRepository cargoTypeRepository,
			IContractRepository contractRepository,
			IDelayPenaltyRepository delayPenaltyRepository,
			IDriverRepository driverRepository,
			IForwarderOfferRepository forwarderOfferRepository,
			ILocationRepository locationRepository,
			IPaymentRepository paymentRepository,
			IPickupChangeRepository pickupChangeRepository,
			IRouteRepository routeRepository,
			IRoutePriceRepository routePriceRepository,
			IShipmentRepository shipmentRepository,
			IShipmentDocumentRepository shipmentDocumentRepository,
			IShipmentStatusHistoryRepository shipmentStatusHistoryRepository,
			IUserRepository userRepository,
			IVehicleMaintenanceRepository vehicleMaintenanceRepository,
			IVehicleTypeRepository vehicleTypeRepository,
			IVehicleRepository vehicleRepository
			)
        {
			_context = context;
			Companies = companyRepository;
			CargoTypes = cargoTypeRepository;
			Contracts = contractRepository;
			DelayPenalties = delayPenaltyRepository;
			Drivers = driverRepository;
			ForwarderOffers = forwarderOfferRepository;
			Locations = locationRepository;
			Payments = paymentRepository;
			PickupChanges = pickupChangeRepository;
			Routes = routeRepository;
			RoutePrices = routePriceRepository;
			Shipments = shipmentRepository;
			ShipmentDocuments = shipmentDocumentRepository;
			ShipmentStatusHistories = shipmentStatusHistoryRepository;
			Users = userRepository;
			VehicleMaintenances = vehicleMaintenanceRepository;
			VehicleTypes = vehicleTypeRepository;
			Vehicles = vehicleRepository;
        }
        public async Task<int> SaveChangesAsync()
		{
			return await _context.SaveChangesAsync();
		}
	}
}
