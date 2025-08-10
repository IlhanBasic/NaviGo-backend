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
			await _pickupChangesCollection.InsertOneAsync(change);
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
	}
}
