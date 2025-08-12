using MongoDB.Bson;
using MongoDB.Driver;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NaviGoApi.Infrastructure.MongoDB.Repositories
{
	public class ShipmentStatusHistoryMongoRepository : IShipmentStatusHistoryRepository
	{
		private readonly IMongoCollection<ShipmentStatusHistory> _shipmentStatusHistoryCollection;

		public ShipmentStatusHistoryMongoRepository(IMongoDatabase database)
		{
			_shipmentStatusHistoryCollection = database.GetCollection<ShipmentStatusHistory>("ShipmentStatusHistories");
		}

		public async Task AddAsync(ShipmentStatusHistory history)
		{
			history.Id = await GetNextIdAsync();
			await _shipmentStatusHistoryCollection.InsertOneAsync(history);
		}

		private async Task<int> GetNextIdAsync()
		{
			var counters = _shipmentStatusHistoryCollection.Database.GetCollection<BsonDocument>("Counters");

			var filter = Builders<BsonDocument>.Filter.Eq("_id", "ShipmentStatusHistories");
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
			await _shipmentStatusHistoryCollection.DeleteOneAsync(e => e.Id == id);
		}

		public async Task<IEnumerable<ShipmentStatusHistory>> GetAllAsync()
		{
			return await _shipmentStatusHistoryCollection.Find(_ => true).ToListAsync();
		}

		public async Task<ShipmentStatusHistory?> GetByIdAsync(int id)
		{
			return await _shipmentStatusHistoryCollection.Find(e => e.Id == id).FirstOrDefaultAsync();
		}

		public async Task UpdateAsync(ShipmentStatusHistory history)
		{
			await _shipmentStatusHistoryCollection.ReplaceOneAsync(e => e.Id == history.Id, history);
		}

		public Task<ShipmentStatusHistory?> GetLastStatusForShipmentAsync(int shipmentId)
		{
			throw new NotImplementedException();
		}
	}
}
