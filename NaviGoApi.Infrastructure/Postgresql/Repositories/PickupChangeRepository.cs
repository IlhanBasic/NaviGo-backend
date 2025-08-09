using Microsoft.EntityFrameworkCore;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using NaviGoApi.Infrastructure.Postgresql.Persistence;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NaviGoApi.Infrastructure.Postgresql.Repositories
{
	public class PickupChangeRepository : IPickupChangeRepository
	{
		private readonly ApplicationDbContext _context;
		public PickupChangeRepository(ApplicationDbContext context)
		{
			_context = context;
		}

		public async Task AddAsync(PickupChange change)
		{
			await _context.PickupChanges.AddAsync(change);
		}

		public async Task DeleteAsync(int id)
		{
			var entity = await _context.PickupChanges.FindAsync(id);
			if (entity != null)
			{
				_context.PickupChanges.Remove(entity);
			}
		}

		public async Task<IEnumerable<PickupChange>> GetAllAsync()
		{
			return await _context.PickupChanges.ToListAsync();
		}

		// Corrected version for int id:
		public async Task<PickupChange?> GetByIdAsync(int id)
		{
			return await _context.PickupChanges.FindAsync(id);
		}

		public async Task UpdateAsync(PickupChange change)
		{
			_context.PickupChanges.Update(change);
			await Task.CompletedTask; // No actual async operation needed here
		}
	}
}
