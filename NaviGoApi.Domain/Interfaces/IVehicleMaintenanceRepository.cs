using NaviGoApi.Common.DTOs;
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
		Task<IEnumerable<VehicleMaintenance>> GetAllAsync(VehicleMaintenanceSearchDto vehicleMaintenanceSearch);
		Task<VehicleMaintenance?> GetByIdAsync(int id);
		Task AddAsync(VehicleMaintenance maintenance);
		Task UpdateAsync(VehicleMaintenance maintenance);
		Task DeleteAsync(int id);
	}
}
