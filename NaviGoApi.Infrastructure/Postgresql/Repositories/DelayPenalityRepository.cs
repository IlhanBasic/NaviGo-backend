using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Infrastructure.Postgresql.Repositories
{
	public class DelayPenalityRepository : IDelayPenaltyRepository
	{
		public Task AddAsync(DelayPenalty penalty)
		{
			throw new NotImplementedException();
		}

		public Task DeleteAsync(int id)
		{
			throw new NotImplementedException();
		}

		public Task<IEnumerable<DelayPenalty>> GetAllAsync()
		{
			throw new NotImplementedException();
		}

		public Task<DelayPenalty?> GetByIdAsync(Guid id)
		{
			throw new NotImplementedException();
		}

		public Task UpdateAsync(DelayPenalty penalty)
		{
			throw new NotImplementedException();
		}
	}
}
