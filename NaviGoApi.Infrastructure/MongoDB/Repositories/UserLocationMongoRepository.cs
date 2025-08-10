using MongoDB.Driver;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NaviGoApi.Infrastructure.MongoDB.Repositories
{
	public class UserLocationMongoRepository : IUserLocationRepository
	{
		private readonly IMongoCollection<UserLocation> _userLocationsCollection;

		public UserLocationMongoRepository(IMongoDatabase database)
		{
			_userLocationsCollection = database.GetCollection<UserLocation>("UserLocations");
		}

		public async Task AddAsync(UserLocation location)
		{
			await _userLocationsCollection.InsertOneAsync(location);
		}

		public async Task<List<UserLocation>> GetRecentLocationsAsync(int userId, TimeSpan interval)
		{
			var cutoff = DateTime.UtcNow.Subtract(interval);
			var filter = Builders<UserLocation>.Filter.And(
				Builders<UserLocation>.Filter.Eq(ul => ul.UserId, userId),
				Builders<UserLocation>.Filter.Gte(ul => ul.AccessTime, cutoff)
			);

			return await _userLocationsCollection.Find(filter).ToListAsync();
		}

		// Ako ti treba SaveChangesAsync zbog interface, ali u MongoDB nije neophodno jer je sve odmah snimljeno
		public Task SaveChangesAsync()
		{
			return Task.CompletedTask;
		}
	}
}
