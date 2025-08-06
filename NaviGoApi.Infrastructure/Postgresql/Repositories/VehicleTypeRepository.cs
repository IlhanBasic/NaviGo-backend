using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Infrastructure.Postgresql.Repositories
{
	public class VehicleTypeRepository : IVehicleTypeRepository
	{
		public Task AddAsync(VehicleType vehicleType)
		{
			throw new NotImplementedException();
		}

		public Task DeleteAsync(int id)
		{
			throw new NotImplementedException();
		}

		public Task<IEnumerable<VehicleType>> GetAllAsync()
		{
			throw new NotImplementedException();
		}

		public Task<VehicleType?> GetByIdAsync(Guid id)
		{
			throw new NotImplementedException();
		}

		public Task UpdateAsync(VehicleType vehicleType)
		{
			throw new NotImplementedException();
		}
	}
}
