using MongoDB.Bson;
using MongoDB.Driver;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NaviGoApi.Infrastructure.MongoDB.Repositories
{
	public class PickupChangeMongoRepository : IPickupChangeRepository
	{
		private readonly IMongoCollection<PickupChange> _pickupChangesCollection;

		public PickupChangeMongoRepository(IMongoDatabase database)
		{
			_pickupChangesCollection = database.GetCollection<PickupChange>("PickupChanges");
		}

		public async Task AddAsync(PickupChange change)
		{
			change.Id = await GetNextIdAsync();
			await _pickupChangesCollection.InsertOneAsync(change);
		}

		private async Task<int> GetNextIdAsync()
		{
			var counters = _pickupChangesCollection.Database.GetCollection<BsonDocument>("Counters");
			var filter = Builders<BsonDocument>.Filter.Eq("_id", "PickupChanges");
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
			await _pickupChangesCollection.DeleteOneAsync(pc => pc.Id == id);
		}

		public async Task<IEnumerable<PickupChange>> GetAllAsync()
		{
			return await _pickupChangesCollection.Find(_ => true).ToListAsync();
		}

		public async Task<PickupChange?> GetByIdAsync(int id)
		{
			return await _pickupChangesCollection.Find(pc => pc.Id == id).FirstOrDefaultAsync();
		}

		public async Task UpdateAsync(PickupChange change)
		{
			await _pickupChangesCollection.ReplaceOneAsync(pc => pc.Id == change.Id, change);
		}

		public async Task<PickupChange?> GetByShipmentAndClientAsync(int shipmentId, int clientId)
		{
			return await _pickupChangesCollection
				.Find(pc => pc.ShipmentId == shipmentId && pc.ClientId == clientId)
				.SortByDescending(pc => pc.Id)
				.FirstOrDefaultAsync();
		}
	}
}
