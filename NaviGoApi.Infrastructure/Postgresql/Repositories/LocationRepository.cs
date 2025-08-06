using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Infrastructure.Postgresql.Repositories
{
	public class LocationRepository : ILocationRepository
	{
		public Task AddAsync(Location location)
		{
			throw new NotImplementedException();
		}

		public Task DeleteAsync(int id)
		{
			throw new NotImplementedException();
		}

		public Task<IEnumerable<Location>> GetAllAsync()
		{
			throw new NotImplementedException();
		}

		public Task<Location?> GetByIdAsync(int id)
		{
			throw new NotImplementedException();
		}

		public Task UpdateAsync(Location location)
		{
			throw new NotImplementedException();
		}
	}
}
