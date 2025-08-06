using NaviGoApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Domain.Interfaces
{
	public interface IVehicleMaintenanceRepository
	{
		Task<IEnumerable<VehicleMaintenance>> GetAllAsync();
		Task<VehicleMaintenance?> GetByIdAsync(Guid id);
		Task AddAsync(VehicleMaintenance maintenance);
		Task UpdateAsync(VehicleMaintenance maintenance);
		Task DeleteAsync(int id);
	}
}
