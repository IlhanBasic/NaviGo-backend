using MongoDB.Bson;
using MongoDB.Driver;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace NaviGoApi.Infrastructure.MongoDB.Repositories
{
	public class LocationMongoRepository : ILocationRepository
	{
		private readonly IMongoCollection<Location> _locationsCollection;

		public LocationMongoRepository(IMongoDatabase database)
		{
			_locationsCollection = database.GetCollection<Location>("Locations");
		}

		public async Task AddAsync(Location location)
		{
			location.Id = await GetNextIdAsync();
			await _locationsCollection.InsertOneAsync(location);
		}

		private async Task<int> GetNextIdAsync()
		{
			var counters = _locationsCollection.Database.GetCollection<BsonDocument>("Counters");
			var filter = Builders<BsonDocument>.Filter.Eq("_id", "Locations");
			var update = Builders<BsonDocument>.Update.Inc("SequenceValue", 1);
			var options = new FindOneAndUpdateOptions<BsonDocument>
			{
				IsUpsert = true,
				ReturnDocument = ReturnDocument.After
			};

			var result = await counters.FindOneAndUpdateAsync(filter, update, options);
			return result["SequenceValue"].AsInt32;
		}

		public async Task DeleteAsync(int id)
		{
			var result = await _locationsCollection.DeleteOneAsync(l => l.Id == id);
			if (result.DeletedCount == 0)
				throw new ValidationException($"Location with Id {id} not found for deletion.");
		}

		public async Task<IEnumerable<Location>> GetAllAsync()
		{
			return await _locationsCollection.Find(_ => true).ToListAsync();
		}

		public async Task<Location?> GetByIdAsync(int id)
		{
			return await _locationsCollection.Find(l => l.Id == id).FirstOrDefaultAsync();
		}

		public async Task<Location?> GetByFullLocationAsync(string zip, string fullAddress, string city)
		{
			var filter = Builders<Location>.Filter.And(
				Builders<Location>.Filter.Eq(l => l.ZIP, zip),
				Builders<Location>.Filter.Eq(l => l.FullAddress, fullAddress),
				Builders<Location>.Filter.Eq(l => l.City, city)
			);

			return await _locationsCollection.Find(filter).FirstOrDefaultAsync();
		}

		public async Task UpdateAsync(Location location)
		{
			var result = await _locationsCollection.ReplaceOneAsync(l => l.Id == location.Id, location);
			if (result.MatchedCount == 0)
				throw new ValidationException($"Location with Id {location.Id} not found for update.");
		}
	}
}
