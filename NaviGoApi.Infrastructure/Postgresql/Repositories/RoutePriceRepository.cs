using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Infrastructure.Postgresql.Repositories
{
	public class RoutePriceRepository : IRoutePriceRepository
	{
		public Task AddAsync(RoutePrice price)
		{
			throw new NotImplementedException();
		}

		public Task DeleteAsync(int id)
		{
			throw new NotImplementedException();
		}

		public Task<IEnumerable<RoutePrice>> GetAllAsync()
		{
			throw new NotImplementedException();
		}

		public Task<RoutePrice?> GetByIdAsync(Guid id)
		{
			throw new NotImplementedException();
		}

		public Task UpdateAsync(RoutePrice price)
		{
			throw new NotImplementedException();
		}
	}
}
