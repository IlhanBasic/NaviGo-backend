using NaviGoApi.Common.DTOs;
using NaviGoApi.Domain.Entities;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace NaviGoApi.Domain.Interfaces
{
	public interface IRouteRepository
	{
		Task<Route?> GetByIdAsync(int id);
		Task<IEnumerable<Route>> GetAllAsync();
		Task<IEnumerable<Route>> GetAllAsync(RouteSearchDto routeSearch);

		Task AddAsync(Route route);
		Task UpdateAsync(Route route);
		Task DeleteAsync(Route route);

		// Specifične metode za Route, ako su potrebne
		Task<IEnumerable<Route>> GetActiveRoutesAsync();
		Task<IEnumerable<Route>> GetRoutesByCompanyIdAsync(int companyId);
		Task<bool> DuplicateRoute(int companyId, int startLocationId, int endLocationId);
		Task<bool> DuplicateRouteUpdate(int companyId, int startLocationId, int endLocationId,int routeId);
	}
}
