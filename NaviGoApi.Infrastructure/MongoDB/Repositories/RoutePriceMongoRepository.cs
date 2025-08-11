using MongoDB.Bson;
using MongoDB.Driver;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System.Collections.Generic;
using System.Linq.Expressions;
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
			price.Id = await GetNextIdAsync();
			await _routePricesCollection.InsertOneAsync(price);
		}

		private async Task<int> GetNextIdAsync()
		{
			var counters = _routePricesCollection.Database.GetCollection<BsonDocument>("Counters");

			var filter = Builders<BsonDocument>.Filter.Eq("_id", "RoutePrices");
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
			await _routePricesCollection.DeleteOneAsync(rp => rp.Id == id);
		}

		public async Task<IEnumerable<RoutePrice>> GetAllAsync()
		{
			return await _routePricesCollection.Find(_ => true).ToListAsync();
		}

		public async Task<RoutePrice?> GetByIdAsync(int id)
		{
			return await _routePricesCollection.Find(rp => rp.Id == id).FirstOrDefaultAsync();
		}

		public Task UpdateAsync(RoutePrice price)
		{
			var result = _routePricesCollection.ReplaceOneAsync(rp => rp.Id == price.Id, price);
			return result.IsCompletedSuccessfully ? Task.CompletedTask : result;
		}

		public Task<bool> ExistsAsync(Expression<Func<RoutePrice, bool>> predicate)
		{
			throw new NotImplementedException();
		}
	}
}
