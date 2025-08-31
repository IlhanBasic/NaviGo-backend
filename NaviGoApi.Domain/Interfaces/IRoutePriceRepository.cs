using NaviGoApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Domain.Interfaces
{
	public interface IRoutePriceRepository
	{
		Task<IEnumerable<RoutePrice>> GetAllAsync();
		Task<RoutePrice?> GetByIdAsync(int id);
		Task<IEnumerable<RoutePrice>> GetByRouteIdAsync(int routeId);
		Task AddAsync(RoutePrice price);
		Task UpdateAsync(RoutePrice price);
		Task DeleteAsync(int id);
		Task<bool> ExistsAsync(Expression<Func<RoutePrice, bool>> predicate);
		Task<RoutePrice?> DuplicateRoutePrice(int routeId,int vehicleTypeId);
	}
}
