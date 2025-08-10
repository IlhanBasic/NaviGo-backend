using MongoDB.Driver;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System.Collections.Generic;
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
			await _locationsCollection.InsertOneAsync(location);
		}

		public async Task DeleteAsync(int id)
		{
			await _locationsCollection.DeleteOneAsync(l => l.Id == id);
		}

		public async Task<IEnumerable<Location>> GetAllAsync()
		{
			return await _locationsCollection.Find(_ => true).ToListAsync();
		}

		public async Task<Location?> GetByIdAsync(int id)
		{
			return await _locationsCollection
				.Find(l => l.Id == id)
				.FirstOrDefaultAsync();
		}

		public async Task UpdateAsync(Location location)
		{
			await _locationsCollection.ReplaceOneAsync(l => l.Id == location.Id, location);
		}
	}
}
