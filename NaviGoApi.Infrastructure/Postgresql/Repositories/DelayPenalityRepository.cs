using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using NaviGoApi.Infrastructure.Postgresql.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NaviGoApi.Infrastructure.Postgresql.Repositories
{
	public class DelayPenalityRepository : IDelayPenaltyRepository
	{
		private readonly ApplicationDbContext _context;

		public DelayPenalityRepository(ApplicationDbContext context)
		{
			_context = context;
		}

		public async Task AddAsync(DelayPenalty penalty)
		{
			await _context.DelayPenalties.AddAsync(penalty);
		}

		public async Task DeleteAsync(int id)
		{
			var entity = await _context.DelayPenalties.FindAsync(id);
			if (entity != null)
			{
				_context.DelayPenalties.Remove(entity);
			}
		}

		public async Task<IEnumerable<DelayPenalty>> GetAllAsync()
		{
			return await _context.DelayPenalties.ToListAsync();
		}

		public async Task<DelayPenalty?> GetByIdAsync(int id)
		{
			return await _context.DelayPenalties.FindAsync(id);
		}

		public async Task UpdateAsync(DelayPenalty penalty)
		{
			_context.DelayPenalties.Update(penalty);
		}
		public async Task<IEnumerable<DelayPenalty>> GetByContractIdAsync(int contractId)
		{
			return await _context.DelayPenalties
				.Include(dp => dp.Shipment)
				.Where(dp => dp.Shipment != null && dp.Shipment.ContractId == contractId)
				.ToListAsync();
		}

		public async Task<DelayPenalty?> GetByShipmentIdAsync(int shipmentId)
		{
			return await _context.DelayPenalties
				.FirstOrDefaultAsync(dp => dp.ShipmentId == shipmentId);
		}

	}
}
