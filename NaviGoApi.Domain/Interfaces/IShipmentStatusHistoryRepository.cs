using NaviGoApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Domain.Interfaces
{
	public interface IShipmentStatusHistoryRepository
	{
		Task<IEnumerable<ShipmentStatusHistory>> GetAllAsync();
		Task<ShipmentStatusHistory?> GetByIdAsync(int id);
		Task AddAsync(ShipmentStatusHistory history);
		Task UpdateAsync(ShipmentStatusHistory history);
		Task DeleteAsync(int id);
		Task<ShipmentStatusHistory?> GetLastStatusForShipmentAsync(int shipmentId);
	}
}
