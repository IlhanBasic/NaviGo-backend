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
			await _shipmentsCollection.InsertOneAsync(shipment);
		}

		// Prepravljen na void po interfejsu:
		public void Delete(Shipment shipment)
		{
			_shipmentsCollection.DeleteOneAsync(s => s.Id == shipment.Id).GetAwaiter().GetResult();
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

		// Prepravljen na void po interfejsu:
		public void Update(Shipment shipment)
		{
			_shipmentsCollection.ReplaceOneAsync(s => s.Id == shipment.Id, shipment).GetAwaiter().GetResult();
		}
	}
}
