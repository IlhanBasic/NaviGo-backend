using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Infrastructure.Postgresql.Repositories
{
	public class PickupChangeRepository : IPickupChangeRepository
	{
		public Task AddAsync(PickupChange change)
		{
			throw new NotImplementedException();
		}

		public Task DeleteAsync(int id)
		{
			throw new NotImplementedException();
		}

		public Task<IEnumerable<PickupChange>> GetAllAsync()
		{
			throw new NotImplementedException();
		}

		public Task<PickupChange?> GetByIdAsync(Guid id)
		{
			throw new NotImplementedException();
		}

		public Task UpdateAsync(PickupChange change)
		{
			throw new NotImplementedException();
		}
	}
}
