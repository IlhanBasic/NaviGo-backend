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
		}

		public async Task DeleteAsync(int id)
		{
			var entity = await _context.ShipmentStatusHistories.FindAsync(id);
			if (entity != null)
			{
				_context.ShipmentStatusHistories.Remove(entity);
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
					.ThenInclude(sh => sh.Contract)
						.ThenInclude(c => c.Forwarder)
				.Include(s => s.Shipment)
					.ThenInclude(sh => sh.Contract)
						.ThenInclude(c => c.Route)
				.Include(s => s.Shipment)
					.ThenInclude(sh => sh.Contract)
						.ThenInclude(c => c.Client)
				.Include(s => s.ChangedByUser)
				.FirstOrDefaultAsync(s => s.Id == id);
		}


		public async Task UpdateAsync(ShipmentStatusHistory history)
		{
			_context.ShipmentStatusHistories.Update(history);
		}
		public async Task<ShipmentStatusHistory?> GetLastStatusForShipmentAsync(int shipmentId)
		{
			return await _context.ShipmentStatusHistories
				.Where(h => h.ShipmentId == shipmentId)
				.OrderByDescending(h => h.ChangedAt) 
				.FirstOrDefaultAsync();
		}

	}
}
