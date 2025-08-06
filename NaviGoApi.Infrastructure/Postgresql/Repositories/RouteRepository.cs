using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Infrastructure.Postgresql.Repositories
{
	public class RouteRepository : IRouteRepository
	{
		public Task AddAsync(Route route)
		{
			throw new NotImplementedException();
		}

		public void Delete(Route route)
		{
			throw new NotImplementedException();
		}

		public Task<IEnumerable<Route>> GetActiveRoutesAsync()
		{
			throw new NotImplementedException();
		}

		public Task<IEnumerable<Route>> GetAllAsync()
		{
			throw new NotImplementedException();
		}

		public Task<Route?> GetByIdAsync(int id)
		{
			throw new NotImplementedException();
		}

		public Task<IEnumerable<Route>> GetRoutesByCompanyIdAsync(int companyId)
		{
			throw new NotImplementedException();
		}

		public void Update(Route route)
		{
			throw new NotImplementedException();
		}
	}
}
