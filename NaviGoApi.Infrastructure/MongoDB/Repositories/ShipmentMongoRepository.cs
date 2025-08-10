using MongoDB.Bson;
using MongoDB.Driver;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NaviGoApi.Infrastructure.MongoDB.Repositories
{
	public class ShipmentMongoRepository : IShipmentRepository
	{
		private readonly IMongoCollection<Shipment> _shipmentsCollection;

		public ShipmentMongoRepository(IMongoDatabase database)
		{
			_shipmentsCollection = database.GetCollection<Shipment>("Shipments");
		}

		public async Task AddAsync(Shipment shipment)
		{
			shipment.Id = await GetNextIdAsync();
			await _shipmentsCollection.InsertOneAsync(shipment);
		}

		private async Task<int> GetNextIdAsync()
		{
			var counters = _shipmentsCollection.Database.GetCollection<BsonDocument>("Counters");

			var filter = Builders<BsonDocument>.Filter.Eq("_id", "Shipments");
			var update = Builders<BsonDocument>.Update.Inc("SequenceValue", 1);

			var options = new FindOneAndUpdateOptions<BsonDocument>
			{
				IsUpsert = true,
				ReturnDocument = ReturnDocument.After
			};

			var result = await counters.FindOneAndUpdateAsync(filter, update, options);
			return result["SequenceValue"].AsInt32;
		}

		public async Task DeleteAsync(Shipment shipment)
		{
			await _shipmentsCollection.DeleteOneAsync(s => s.Id == shipment.Id);
		}

		public async Task<IEnumerable<Shipment>> GetAllAsync()
		{
			return await _shipmentsCollection.Find(_ => true).ToListAsync();
		}

		public async Task<IEnumerable<Shipment>> GetByContractIdAsync(int contractId)
		{
			return await _shipmentsCollection.Find(s => s.ContractId == contractId).ToListAsync();
		}

		public async Task<Shipment?> GetByIdAsync(int id)
		{
			return await _shipmentsCollection.Find(s => s.Id == id).FirstOrDefaultAsync();
		}

		public async Task<IEnumerable<Shipment>> GetByStatusAsync(ShipmentStatus status)
		{
			return await _shipmentsCollection.Find(s => s.Status == status).ToListAsync();
		}

		public async Task UpdateAsync(Shipment shipment)
		{
			await _shipmentsCollection.ReplaceOneAsync(s => s.Id == shipment.Id, shipment);
		}
	}
}
