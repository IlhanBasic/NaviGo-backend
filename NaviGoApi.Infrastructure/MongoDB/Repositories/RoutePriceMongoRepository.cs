using MongoDB.Bson;
using MongoDB.Driver;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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
			var result = await _routePricesCollection.DeleteOneAsync(rp => rp.Id == id);
			if (result.DeletedCount == 0)
				throw new ValidationException($"RoutePrice with Id {id} not found for deletion.");
		}

		public async Task<IEnumerable<RoutePrice>> GetAllAsync()
		{
			var prices = await _routePricesCollection.Find(_ => true).ToListAsync();
			return await LoadNavigationPropertiesAsync(prices);
		}

		public async Task<RoutePrice?> GetByIdAsync(int id)
		{
			var price = await _routePricesCollection.Find(rp => rp.Id == id).FirstOrDefaultAsync();
			if (price == null) return null;

			var list = await LoadNavigationPropertiesAsync(new List<RoutePrice> { price });
			return list.FirstOrDefault();
		}

		public async Task UpdateAsync(RoutePrice price)
		{
			var result = await _routePricesCollection.ReplaceOneAsync(rp => rp.Id == price.Id, price);
			if (result.MatchedCount == 0)
				throw new ValidationException($"RoutePrice with Id {price.Id} not found for update.");
		}

		public async Task<bool> ExistsAsync(Expression<Func<RoutePrice, bool>> predicate)
		{
			return await _routePricesCollection.Find(predicate).AnyAsync();
		}

		private async Task<IEnumerable<RoutePrice>> LoadNavigationPropertiesAsync(IEnumerable<RoutePrice> prices)
		{
			var list = prices.ToList();
			if (!list.Any()) return list;

			var db = _routePricesCollection.Database;
			var vehicleTypesCollection = db.GetCollection<VehicleType>("VehicleTypes");

			foreach (var price in list)
			{
				if (price.VehicleTypeId != 0)
					price.VehicleType = await vehicleTypesCollection.Find(vt => vt.Id == price.VehicleTypeId).FirstOrDefaultAsync();
			}

			return list;
		}

		public async Task<RoutePrice?> DuplicateRoutePrice(int routeId, int vehicleTypeId)
		{
			return await _routePricesCollection
				.Find(rp => rp.RouteId == routeId && rp.VehicleTypeId == vehicleTypeId)
				.FirstOrDefaultAsync();
		}

		public Task<IEnumerable<RoutePrice>> GetByRouteIdAsync(int routeId)
		{
			throw new NotImplementedException();
		}
	}
}
