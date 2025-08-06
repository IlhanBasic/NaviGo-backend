using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Infrastructure.Postgresql.Repositories
{
	public class VehicleMaintenanceRepository : IVehicleMaintenanceRepository
	{
		public Task AddAsync(VehicleMaintenance maintenance)
		{
			throw new NotImplementedException();
		}

		public Task DeleteAsync(int id)
		{
			throw new NotImplementedException();
		}

		public Task<IEnumerable<VehicleMaintenance>> GetAllAsync()
		{
			throw new NotImplementedException();
		}

		public Task<VehicleMaintenance?> GetByIdAsync(Guid id)
		{
			throw new NotImplementedException();
		}

		public Task UpdateAsync(VehicleMaintenance maintenance)
		{
			throw new NotImplementedException();
		}
	}
}
