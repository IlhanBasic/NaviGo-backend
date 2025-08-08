using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using NaviGoApi.Infrastructure.Postgresql.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Infrastructure.Postgresql.Repositories
{
	public class UserLocationRepository : IUserLocationRepository
	{
		private readonly ApplicationDbContext _context;

		public UserLocationRepository(ApplicationDbContext context)
		{
			_context = context;
		}

		public async Task AddAsync(UserLocation location)
		{
			await _context.UserLocations.AddAsync(location);
		}

		public async Task<List<UserLocation>> GetRecentLocationsAsync(int userId, TimeSpan interval)
		{
			//var cutoff = DateTime.UtcNow.Subtract(interval);
			//return await _context.UserLocations
			//	.Where(ul => ul.UserId == userId && ul.AccessTime >= cutoff)
			//	.ToListAsync();
			return null;
		}

		public async Task SaveChangesAsync()
		{
			await _context.SaveChangesAsync();
		}
	}

}
