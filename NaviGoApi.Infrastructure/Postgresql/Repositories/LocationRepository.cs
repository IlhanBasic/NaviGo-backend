using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using NaviGoApi.Infrastructure.Postgresql.Persistence;

namespace NaviGoApi.Infrastructure.Postgresql.Repositories
{
	public class LocationRepository : ILocationRepository
	{
		private readonly ApplicationDbContext _context;

		public LocationRepository(ApplicationDbContext context)
		{
			_context = context;
		}

		public async Task AddAsync(Location location)
		{
			await _context.Locations.AddAsync(location);
		}

		public async Task DeleteAsync(int id)
		{
			var location = await _context.Locations.FindAsync(id);
			if (location != null)
			{
				_context.Locations.Remove(location);
			}
			// SaveChanges radi UnitOfWork
		}

		public async Task<IEnumerable<Location>> GetAllAsync()
		{
			return await _context.Locations.ToListAsync();
		}

		public async Task<Location?> GetByIdAsync(int id)
		{
			return await _context.Locations.FindAsync(id);
		}

		public async Task UpdateAsync(Location location)
		{
			_context.Locations.Update(location);
		}
	}
}
