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
			await _shipmentStatusHistoryCollection.InsertOneAsync(history);
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
	}
}
