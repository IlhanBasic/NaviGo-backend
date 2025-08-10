using NaviGoApi.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NaviGoApi.Domain.Interfaces
{
	public interface IShipmentRepository
	{
		Task<Shipment?> GetByIdAsync(int id);
		Task<IEnumerable<Shipment>> GetAllAsync();
		Task AddAsync(Shipment shipment);
		Task UpdateAsync(Shipment shipment);
		Task DeleteAsync(Shipment shipment);

		// Specifične metode za Shipment ako trebaš
		Task<IEnumerable<Shipment>> GetByContractIdAsync(int contractId);
		Task<IEnumerable<Shipment>> GetByStatusAsync(ShipmentStatus status);
	}
}
