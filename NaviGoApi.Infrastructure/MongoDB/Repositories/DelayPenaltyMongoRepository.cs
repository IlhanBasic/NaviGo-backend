//using MongoDB.Bson;
//using MongoDB.Driver;
//using NaviGoApi.Domain.Entities;
//using NaviGoApi.Domain.Interfaces;
//using System;
//using System.Collections.Generic;
//using System.Threading.Tasks;

//namespace NaviGoApi.Infrastructure.MongoDB.Repositories
//{
//	public class DelayPenaltyMongoRepository : IDelayPenaltyRepository
//	{
//		private readonly IMongoCollection<DelayPenalty> _delayPenaltiesCollection;
//		private readonly IMongoCollection<BsonDocument> _countersCollection;

//		public DelayPenaltyMongoRepository(IMongoDatabase database)
//		{
//			_delayPenaltiesCollection = database.GetCollection<DelayPenalty>("DelayPenalties");
//			_countersCollection = database.GetCollection<BsonDocument>("Counters");
//		}

//		public async Task AddAsync(DelayPenalty penalty)
//		{
//			penalty.Id = await GetNextIdAsync();
//			await _delayPenaltiesCollection.InsertOneAsync(penalty);
//		}

//		private async Task<int> GetNextIdAsync()
//		{
//			var filter = Builders<BsonDocument>.Filter.Eq("_id", "DelayPenalties");
//			var update = Builders<BsonDocument>.Update.Inc("SequenceValue", 1);
//			var options = new FindOneAndUpdateOptions<BsonDocument>
//			{
//				IsUpsert = true,
//				ReturnDocument = ReturnDocument.After
//			};

//			var result = await _countersCollection.FindOneAndUpdateAsync(filter, update, options);
//			return result["SequenceValue"].AsInt32;
//		}

//		public async Task DeleteAsync(int id)
//		{
//			var result = await _delayPenaltiesCollection.DeleteOneAsync(dp => dp.Id == id);
//			if (result.DeletedCount == 0)
//				throw new KeyNotFoundException($"DelayPenalty with Id {id} not found for deletion.");
//		}

//		public async Task<IEnumerable<DelayPenalty>> GetAllAsync()
//		{
//			return await _delayPenaltiesCollection.Find(_ => true).ToListAsync();
//		}

//		public async Task<DelayPenalty?> GetByIdAsync(int id)
//		{
//			return await _delayPenaltiesCollection.Find(dp => dp.Id == id).FirstOrDefaultAsync();
//		}

//		public async Task UpdateAsync(DelayPenalty penalty)
//		{
//			var result = await _delayPenaltiesCollection.ReplaceOneAsync(dp => dp.Id == penalty.Id, penalty);
//			if (result.MatchedCount == 0)
//				throw new KeyNotFoundException($"DelayPenalty with Id {penalty.Id} not found for update.");
//		}

//		public async Task<IEnumerable<DelayPenalty>> GetByContractIdAsync(int contractId)
//		{
//			var shipmentCollection = _delayPenaltiesCollection.Database.GetCollection<Shipment>("Shipments");

//			var pipeline = new[]
//			{
//        // Spoj DelayPenalties sa Shipment kolekcijom preko ShipmentId
//        new BsonDocument("$lookup", new BsonDocument
//			{
//				{ "from", "Shipments" },
//				{ "localField", "ShipmentId" },
//				{ "foreignField", "Id" },
//				{ "as", "ShipmentInfo" }
//			}),
//        // Razvuci niz ShipmentInfo da bismo mogli filtrirati
//        new BsonDocument("$unwind", "$ShipmentInfo"),
//        // Filteruj po ShipmentInfo.ContractId
//        new BsonDocument("$match", new BsonDocument("ShipmentInfo.ContractId", contractId))
//	};

//			var results = await _delayPenaltiesCollection.Aggregate<DelayPenalty>(pipeline).ToListAsync();
//			return results;
//		}


//		public async Task<DelayPenalty?> GetByShipmentIdAsync(int shipmentId)
//		{
//			return await _delayPenaltiesCollection.Find(dp => dp.ShipmentId == shipmentId).FirstOrDefaultAsync();
//		}
//	}
//}
using MongoDB.Bson;
using MongoDB.Driver;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NaviGoApi.Infrastructure.MongoDB.Repositories
{
	public class DelayPenaltyMongoRepository : IDelayPenaltyRepository
	{
		private readonly IMongoCollection<DelayPenalty> _delayPenaltiesCollection;
		private readonly IMongoCollection<BsonDocument> _countersCollection;

		public DelayPenaltyMongoRepository(IMongoDatabase database)
		{
			_delayPenaltiesCollection = database.GetCollection<DelayPenalty>("DelayPenalties");
			_countersCollection = database.GetCollection<BsonDocument>("Counters");
		}

		public async Task AddAsync(DelayPenalty penalty)
		{
			penalty.Id = await GetNextIdAsync();
			await _delayPenaltiesCollection.InsertOneAsync(penalty);
		}

		private async Task<int> GetNextIdAsync()
		{
			var filter = Builders<BsonDocument>.Filter.Eq("_id", "DelayPenalties");
			var update = Builders<BsonDocument>.Update.Inc("SequenceValue", 1);
			var options = new FindOneAndUpdateOptions<BsonDocument>
			{
				IsUpsert = true,
				ReturnDocument = ReturnDocument.After
			};

			var result = await _countersCollection.FindOneAndUpdateAsync(filter, update, options);
			return result["SequenceValue"].AsInt32;
		}

		public async Task DeleteAsync(int id)
		{
			var result = await _delayPenaltiesCollection.DeleteOneAsync(dp => dp.Id == id);
			if (result.DeletedCount == 0)
				throw new KeyNotFoundException($"DelayPenalty with Id {id} not found for deletion.");
		}

		public async Task<IEnumerable<DelayPenalty>> GetAllAsync()
		{
			var penalties = await _delayPenaltiesCollection.Find(_ => true).ToListAsync();
			return await PopulateRelationsAsync(penalties);
		}

		public async Task<DelayPenalty?> GetByIdAsync(int id)
		{
			var penalty = await _delayPenaltiesCollection.Find(dp => dp.Id == id).FirstOrDefaultAsync();
			if (penalty == null) return null;

			var populated = await PopulateRelationsAsync(new List<DelayPenalty> { penalty });
			return populated.FirstOrDefault();
		}

		public async Task UpdateAsync(DelayPenalty penalty)
		{
			var result = await _delayPenaltiesCollection.ReplaceOneAsync(dp => dp.Id == penalty.Id, penalty);
			if (result.MatchedCount == 0)
				throw new KeyNotFoundException($"DelayPenalty with Id {penalty.Id} not found for update.");
		}

		public async Task<IEnumerable<DelayPenalty>> GetByContractIdAsync(int contractId)
		{
			var penalties = await _delayPenaltiesCollection.Find(_ => true).ToListAsync();
			var populated = await PopulateRelationsAsync(penalties);
			return populated.Where(dp => dp.Shipment != null && dp.Shipment.ContractId == contractId);
		}

		public async Task<DelayPenalty?> GetByShipmentIdAsync(int shipmentId)
		{
			var penalty = await _delayPenaltiesCollection.Find(dp => dp.ShipmentId == shipmentId).FirstOrDefaultAsync();
			if (penalty == null) return null;

			var populated = await PopulateRelationsAsync(new List<DelayPenalty> { penalty });
			return populated.FirstOrDefault();
		}

		// Helper za ručno dohvatiti povezani Shipment
		private async Task<IEnumerable<DelayPenalty>> PopulateRelationsAsync(IEnumerable<DelayPenalty> penalties)
		{
			var penaltyList = penalties.ToList();
			if (!penaltyList.Any()) return penaltyList;

			var db = _delayPenaltiesCollection.Database;
			var shipmentsCollection = db.GetCollection<Shipment>("Shipments");

			foreach (var penalty in penaltyList)
			{
				if (penalty.ShipmentId != 0)
				{
					penalty.Shipment = await shipmentsCollection.Find(s => s.Id == penalty.ShipmentId).FirstOrDefaultAsync();
				}
			}

			return penaltyList;
		}
	}
}
