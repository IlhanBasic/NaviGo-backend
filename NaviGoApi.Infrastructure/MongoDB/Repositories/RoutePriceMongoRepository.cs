using MongoDB.Driver;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NaviGoApi.Infrastructure.MongoDB.Repositories
{
	public class RoutePriceMongoRepository : IRoutePriceRepository
	{
		private readonly IMongoCollection<RoutePrice> _routePricesCollection;

		public RoutePriceMongoRepository(IMongoDatabase database)
		{
			_routePricesCollection = database.GetCollection<RoutePrice>("RoutePrices");
		}

		public async Task AddAsync(RoutePrice price)
		{
			await _routePricesCollection.InsertOneAsync(price);
		}

		public async Task DeleteAsync(int id)
		{
			await _routePricesCollection.DeleteOneAsync(rp => rp.Id == id);
		}

		public async Task<IEnumerable<RoutePrice>> GetAllAsync()
		{
			// MongoDB ne podržava Include, za VehicleType moraš posebno dohvatiti ako ti treba
			return await _routePricesCollection.Find(_ => true).ToListAsync();
		}

		public async Task<RoutePrice?> GetByIdAsync(int id)
		{
			return await _routePricesCollection.Find(rp => rp.Id == id).FirstOrDefaultAsync();
		}

		public Task UpdateAsync(RoutePrice price)
		{
			// MongoDB ne podržava pravi async update osim ReplaceOneAsync, ali možeš koristiti ovako:
			var result = _routePricesCollection.ReplaceOneAsync(rp => rp.Id == price.Id, price);
			return result.IsCompletedSuccessfully ? Task.CompletedTask : result;
		}
	}
}
