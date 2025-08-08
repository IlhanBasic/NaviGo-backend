using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;

namespace NaviGoApi.Domain.Interfaces
{
	public interface IUnitOfWork
	{
		ICompanyRepository Companies { get; }
		IUserRepository Users { get; }
		IContractRepository Contracts { get; }
		IDriverRepository Drivers { get; }
		IForwarderOfferRepository ForwarderOffers { get; }
		IRouteRepository Routes { get; }
		IShipmentRepository Shipments { get; }
		IPaymentRepository Payments { get; }
		IVehicleRepository Vehicles { get; }
		ILocationRepository Locations { get; }
		IVehicleTypeRepository VehicleTypes { get; }
		IShipmentDocumentRepository ShipmentDocuments { get; }
		IShipmentStatusHistoryRepository ShipmentStatusHistories { get; }
		IPickupChangeRepository PickupChanges { get; }
		IVehicleMaintenanceRepository VehicleMaintenances { get; }
		ICargoTypeRepository CargoTypes { get; }
		IDelayPenaltyRepository DelayPenalties { get; }
		IRoutePriceRepository RoutePrices { get; }
		IUserLocationRepository UserLocations { get; }
		Task<int> SaveChangesAsync();

	}
}
