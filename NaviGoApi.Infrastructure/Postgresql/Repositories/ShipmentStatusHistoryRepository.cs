using Microsoft.EntityFrameworkCore;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using NaviGoApi.Infrastructure.Postgresql.Persistence;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NaviGoApi.Infrastructure.Postgresql.Repositories
{
	public class ShipmentStatusHistoryRepository : IShipmentStatusHistoryRepository
	{
		private readonly ApplicationDbContext _context;
		public ShipmentStatusHistoryRepository(ApplicationDbContext context)
		{
			_context = context;
		}

		public async Task AddAsync(ShipmentStatusHistory history)
		{
			await _context.ShipmentStatusHistories.AddAsync(history);
			await _context.SaveChangesAsync();
		}

		public async Task DeleteAsync(int id)
		{
			var entity = await _context.ShipmentStatusHistories.FindAsync(id);
			if (entity != null)
			{
				_context.ShipmentStatusHistories.Remove(entity);
				await _context.SaveChangesAsync();
			}
		}

		public async Task<IEnumerable<ShipmentStatusHistory>> GetAllAsync()
		{
			return await _context.ShipmentStatusHistories
				.Include(s => s.Shipment)
				.Include(s => s.ChangedByUser)
				.ToListAsync();
		}

		public async Task<ShipmentStatusHistory?> GetByIdAsync(int id)
		{
			return await _context.ShipmentStatusHistories
				.Include(s => s.Shipment)
				.Include(s => s.ChangedByUser)
				.FirstOrDefaultAsync(s => s.Id == (int)(object)id);
		}

		public async Task UpdateAsync(ShipmentStatusHistory history)
		{
			_context.ShipmentStatusHistories.Update(history);
			await _context.SaveChangesAsync();
		}
	}
}
