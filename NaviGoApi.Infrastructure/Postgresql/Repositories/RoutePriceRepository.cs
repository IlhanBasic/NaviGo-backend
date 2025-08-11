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

		public async Task<bool> ExistsAsync(Expression<Func<RoutePrice, bool>> predicate)
		{
			return await _context.RoutesPrices.AnyAsync(predicate);
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

		public Task UpdateAsync(RoutePrice price)
		{
			_context.RoutesPrices.Update(price);
			return Task.CompletedTask;
		}
	}
}
