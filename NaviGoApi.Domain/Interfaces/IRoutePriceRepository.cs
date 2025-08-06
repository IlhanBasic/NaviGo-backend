using NaviGoApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Domain.Interfaces
{
	public interface IRoutePriceRepository
	{
		Task<IEnumerable<RoutePrice>> GetAllAsync();
		Task<RoutePrice?> GetByIdAsync(Guid id);
		Task AddAsync(RoutePrice price);
		Task UpdateAsync(RoutePrice price);
		Task DeleteAsync(int id);
	}
}
