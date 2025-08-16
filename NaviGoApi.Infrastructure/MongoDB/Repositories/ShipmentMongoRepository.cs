using Microsoft.AspNetCore.Routing;
using MongoDB.Bson;
using MongoDB.Driver;
using NaviGoApi.Common.DTOs;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using Route = NaviGoApi.Domain.Entities.Route;

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
			var shipment = await _shipmentsCollection.Find(s => s.Id == id).FirstOrDefaultAsync();
			if (shipment == null)
				return null;

			// Učitavamo Contract
			var contractsCollection = _shipmentsCollection.Database.GetCollection<Contract>("Contracts");
			var contract = await contractsCollection.Find(c => c.Id == shipment.ContractId).FirstOrDefaultAsync();
			if (contract == null)
				return shipment; // ili null, zavisi kako želiš

			// Učitavamo Payment za taj contract
			var paymentsCollection = _shipmentsCollection.Database.GetCollection<Payment>("Payments");
			contract.Payment = await paymentsCollection.Find(p => p.ContractId == contract.Id).FirstOrDefaultAsync();

			// Učitavamo Forwarder (Company)
			if (contract.ForwarderId != 0)
			{
				var companiesCollection = _shipmentsCollection.Database.GetCollection<Company>("Companies");
				contract.Forwarder = await companiesCollection.Find(c => c.Id == contract.ForwarderId).FirstOrDefaultAsync();
			}
			// Učitavamo Vehicle
			if (shipment.VehicleId != 0)
			{
				var vehiclesCollection = _shipmentsCollection.Database.GetCollection<Vehicle>("Vehicles");
				shipment.Vehicle = await vehiclesCollection.Find(v => v.Id == shipment.VehicleId).FirstOrDefaultAsync();
			}

			// Učitavamo Route
			if (contract.RouteId != 0)
			{
				var routesCollection = _shipmentsCollection.Database.GetCollection<Route>("Routes");
				var route = await routesCollection.Find(r => r.Id == contract.RouteId).FirstOrDefaultAsync();

				if (route != null)
				{
					// Učitavamo RoutePrices za taj Route
					var routePricesCollection = _shipmentsCollection.Database.GetCollection<RoutePrice>("RoutePrices");
					route.RoutePrices = await routePricesCollection.Find(rp => rp.RouteId == route.Id).ToListAsync();
				}

				contract.Route = route;
			}


			shipment.Contract = contract;
			return shipment;
		}

		public async Task<IEnumerable<Shipment>> GetByStatusAsync(ShipmentStatus status)
		{
			return await _shipmentsCollection.Find(s => s.Status == status).ToListAsync();
		}

		public async Task UpdateAsync(Shipment shipment)
		{
			await _shipmentsCollection.ReplaceOneAsync(s => s.Id == shipment.Id, shipment);
		}

		public Task<IEnumerable<Shipment>> GetAllAsync(ShipmentSearchDto shipmentSearch)
		{
			throw new NotImplementedException();
		}
	}
}
