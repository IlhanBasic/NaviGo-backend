using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Infrastructure.Postgresql.Repositories
{
	public class VehicleRepository : IVehicleRepository
	{
		public Task AddAsync(Vehicle vehicle)
		{
			throw new NotImplementedException();
		}

		public void Delete(Vehicle vehicle)
		{
			throw new NotImplementedException();
		}

		public Task<IEnumerable<Vehicle>> GetAllAsync()
		{
			throw new NotImplementedException();
		}

		public Task<IEnumerable<Vehicle>> GetAvailableVehiclesAsync()
		{
			throw new NotImplementedException();
		}

		public Task<IEnumerable<Vehicle>> GetByCompanyIdAsync(int companyId)
		{
			throw new NotImplementedException();
		}

		public Task<Vehicle?> GetByIdAsync(int id)
		{
			throw new NotImplementedException();
		}

		public void Update(Vehicle vehicle)
		{
			throw new NotImplementedException();
		}
	}
}
