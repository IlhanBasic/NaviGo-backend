using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using NaviGoApi.Infrastructure.Postgresql.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace NaviGoApi.Infrastructure.Postgresql.Repositories
{
	public class RoutePriceRepository : IRoutePriceRepository
	{
		private readonly ApplicationDbContext _context;

		public RoutePriceRepository(ApplicationDbContext context)
		{
			_context = context;
		}

		public async Task AddAsync(RoutePrice price)
		{
			await _context.RoutesPrices.AddAsync(price);
		}

		public async Task DeleteAsync(int id)
		{
			var entity = await _context.RoutesPrices.FindAsync(id);
			if (entity != null)
			{
				_context.RoutesPrices.Remove(entity);
			}
		}

		public async Task<RoutePrice?> DuplicateRoutePrice(int routeId, int vehicleTypeId)
		{
			return await _context.RoutesPrices
				.FirstOrDefaultAsync(x => x.RouteId == routeId && x.VehicleTypeId == vehicleTypeId);
		}

		public async Task<IEnumerable<RoutePrice>> GetAllAsync()
		{
			return await _context.RoutesPrices
				.Include(rp => rp.VehicleType)
				.ToListAsync();
		}

		public async Task<RoutePrice?> GetByIdAsync(int id)
		{
			return await _context.RoutesPrices
				.Include(rp => rp.VehicleType)
				.FirstOrDefaultAsync(rp => rp.Id == id);
		}

		public async Task<IEnumerable<RoutePrice>> GetByRouteIdAsync(int routeId)
		{
			return await _context.RoutesPrices
				.Where(rp => rp.RouteId == routeId)
				.Include(rp => rp.VehicleType)
				.ToListAsync();
		}

		public Task UpdateAsync(RoutePrice price)
		{
			_context.RoutesPrices.Update(price);
			return Task.CompletedTask;
		}
	}
}
