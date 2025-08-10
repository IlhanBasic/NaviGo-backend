using MongoDB.Bson;
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
			location.Id = await GetNextIdAsync();
			await _userLocationsCollection.InsertOneAsync(location);
		}

		private async Task<int> GetNextIdAsync()
		{
			var counters = _userLocationsCollection.Database.GetCollection<BsonDocument>("Counters");

			var filter = Builders<BsonDocument>.Filter.Eq("_id", "UserLocations");
			var update = Builders<BsonDocument>.Update.Inc("SequenceValue", 1);

			var options = new FindOneAndUpdateOptions<BsonDocument>
			{
				IsUpsert = true,
				ReturnDocument = ReturnDocument.After
			};

			var result = await counters.FindOneAndUpdateAsync(filter, update, options);
			return result["SequenceValue"].AsInt32;
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

		// MongoDB nema SaveChanges jer su promene odmah upisane, pa samo vrati CompletedTask
		public Task SaveChangesAsync()
		{
			return Task.CompletedTask;
		}
	}
}
