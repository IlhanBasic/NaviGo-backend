//using MongoDB.Bson;
//using MongoDB.Driver;
//using NaviGoApi.Domain.Entities;
//using NaviGoApi.Domain.Interfaces;
//using System.Collections.Generic;
//using System.Threading.Tasks;

//namespace NaviGoApi.Infrastructure.MongoDB.Repositories
//{
//	public class ShipmentStatusHistoryMongoRepository : IShipmentStatusHistoryRepository
//	{
//		private readonly IMongoCollection<ShipmentStatusHistory> _shipmentStatusHistoryCollection;

//		public ShipmentStatusHistoryMongoRepository(IMongoDatabase database)
//		{
//			_shipmentStatusHistoryCollection = database.GetCollection<ShipmentStatusHistory>("ShipmentStatusHistories");
//		}

//		public async Task AddAsync(ShipmentStatusHistory history)
//		{
//			history.Id = await GetNextIdAsync();
//			await _shipmentStatusHistoryCollection.InsertOneAsync(history);
//		}

//		private async Task<int> GetNextIdAsync()
//		{
//			var counters = _shipmentStatusHistoryCollection.Database.GetCollection<BsonDocument>("Counters");

//			var filter = Builders<BsonDocument>.Filter.Eq("_id", "ShipmentStatusHistories");
//			var update = Builders<BsonDocument>.Update.Inc("SequenceValue", 1);

//			var options = new FindOneAndUpdateOptions<BsonDocument>
//			{
//				IsUpsert = true,
//				ReturnDocument = ReturnDocument.After
//			};

//			var result = await counters.FindOneAndUpdateAsync(filter, update, options);
//			return result["SequenceValue"].AsInt32;
//		}

//		public async Task DeleteAsync(int id)
//		{
//			await _shipmentStatusHistoryCollection.DeleteOneAsync(e => e.Id == id);
//		}

//		public async Task<IEnumerable<ShipmentStatusHistory>> GetAllAsync()
//		{
//			return await _shipmentStatusHistoryCollection.Find(_ => true).ToListAsync();
//		}

//		public async Task<ShipmentStatusHistory?> GetByIdAsync(int id)
//		{
//			var history = await _shipmentStatusHistoryCollection.Find(e => e.Id == id).FirstOrDefaultAsync();
//			if (history == null)
//				return null;
//			var shipmentsCollection = _shipmentStatusHistoryCollection.Database.GetCollection<Shipment>("Shipments");
//			history.Shipment = await shipmentsCollection.Find(s => s.Id == history.ShipmentId).FirstOrDefaultAsync();

//			if (history.Shipment != null)
//			{
//				var contractsCollection = _shipmentStatusHistoryCollection.Database.GetCollection<Contract>("Contracts");
//				var contract = await contractsCollection.Find(c => c.Id == history.Shipment.ContractId).FirstOrDefaultAsync();
//				if (contract != null)
//				{
//					var paymentsCollection = _shipmentStatusHistoryCollection.Database.GetCollection<Payment>("Payments");
//					contract.Payment = await paymentsCollection.Find(p => p.ContractId == contract.Id).FirstOrDefaultAsync();
//					var companiesCollection = _shipmentStatusHistoryCollection.Database.GetCollection<Company>("Companies");
//					contract.Forwarder = await companiesCollection.Find(c => c.Id == contract.ForwarderId).FirstOrDefaultAsync();
//					var routesCollection = _shipmentStatusHistoryCollection.Database.GetCollection<Route>("Routes");
//					contract.Route = await routesCollection.Find(r => r.Id == contract.RouteId).FirstOrDefaultAsync();
//				}

//				history.Shipment.Contract = contract;
//			}

//			return history;
//		}


//		public async Task UpdateAsync(ShipmentStatusHistory history)
//		{
//			await _shipmentStatusHistoryCollection.ReplaceOneAsync(e => e.Id == history.Id, history);
//		}

//		public async Task<ShipmentStatusHistory?> GetLastStatusForShipmentAsync(int shipmentId)
//		{
//			var filter = Builders<ShipmentStatusHistory>.Filter.Eq(h => h.ShipmentId, shipmentId);
//			var sort = Builders<ShipmentStatusHistory>.Sort.Descending(h => h.ChangedAt);

//			return await _shipmentStatusHistoryCollection
//				.Find(filter)
//				.Sort(sort)
//				.FirstOrDefaultAsync();
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
			var histories = await _shipmentStatusHistoryCollection.Find(_ => true).ToListAsync();
			return await LoadNavigationPropertiesAsync(histories);
		}

		public async Task<ShipmentStatusHistory?> GetByIdAsync(int id)
		{
			var history = await _shipmentStatusHistoryCollection.Find(e => e.Id == id).FirstOrDefaultAsync();
			if (history == null) return null;

			await LoadNavigationPropertiesAsync(new List<ShipmentStatusHistory> { history });
			return history;
		}

		public async Task UpdateAsync(ShipmentStatusHistory history)
		{
			await _shipmentStatusHistoryCollection.ReplaceOneAsync(e => e.Id == history.Id, history);
		}

		public async Task<ShipmentStatusHistory?> GetLastStatusForShipmentAsync(int shipmentId)
		{
			var filter = Builders<ShipmentStatusHistory>.Filter.Eq(h => h.ShipmentId, shipmentId);
			var sort = Builders<ShipmentStatusHistory>.Sort.Descending(h => h.ChangedAt);

			var history = await _shipmentStatusHistoryCollection
				.Find(filter)
				.Sort(sort)
				.FirstOrDefaultAsync();

			if (history != null)
				await LoadNavigationPropertiesAsync(new List<ShipmentStatusHistory> { history });

			return history;
		}

		private async Task<IEnumerable<ShipmentStatusHistory>> LoadNavigationPropertiesAsync(IEnumerable<ShipmentStatusHistory> histories)
		{
			var list = histories.ToList();
			if (!list.Any()) return list;

			var db = _shipmentStatusHistoryCollection.Database;

			var shipmentsCollection = db.GetCollection<Shipment>("Shipments");
			var contractsCollection = db.GetCollection<Contract>("Contracts");
			var companiesCollection = db.GetCollection<Company>("Companies");
			var routesCollection = db.GetCollection<Route>("Routes");
			var clientsCollection = db.GetCollection<User>("Clients");
			var paymentsCollection = db.GetCollection<Payment>("Payments");
			var usersCollection = db.GetCollection<User>("Users");

			foreach (var history in list)
			{
				// Load Shipment
				if (history.ShipmentId != 0)
					history.Shipment = await shipmentsCollection.Find(s => s.Id == history.ShipmentId).FirstOrDefaultAsync();

				// Load Contract + nested includes
				if (history.Shipment?.ContractId != 0)
				{
					var contract = await contractsCollection.Find(c => c.Id == history.Shipment.ContractId).FirstOrDefaultAsync();
					if (contract != null)
					{
						if (contract.ForwarderId != 0)
							contract.Forwarder = await companiesCollection.Find(c => c.Id == contract.ForwarderId).FirstOrDefaultAsync();
						if (contract.RouteId != 0)
							contract.Route = await routesCollection.Find(r => r.Id == contract.RouteId).FirstOrDefaultAsync();
						if (contract.ClientId != 0)
							contract.Client = await clientsCollection.Find(cl => cl.Id == contract.ClientId).FirstOrDefaultAsync();

						// Load Payment if needed
						contract.Payment = await paymentsCollection.Find(p => p.ContractId == contract.Id).FirstOrDefaultAsync();
					}
					history.Shipment.Contract = contract;
				}

				// Load ChangedByUser
				if (history.ChangedByUserId != 0)
					history.ChangedByUser = await usersCollection.Find(u => u.Id == history.ChangedByUserId).FirstOrDefaultAsync();
			}

			return list;
		}
	}
}
