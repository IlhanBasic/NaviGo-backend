using MongoDB.Bson;
using MongoDB.Driver;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NaviGoApi.Infrastructure.MongoDB.Repositories
{
	public class RouteMongoRepository : IRouteRepository
	{
		private readonly IMongoCollection<Route> _routesCollection;

		public RouteMongoRepository(IMongoDatabase database)
		{
			_routesCollection = database.GetCollection<Route>("Routes");
		}

		public async Task AddAsync(Route route)
		{
			route.Id = await GetNextIdAsync();
			await _routesCollection.InsertOneAsync(route);
		}

		private async Task<int> GetNextIdAsync()
		{
			var counters = _routesCollection.Database.GetCollection<BsonDocument>("Counters");

			var filter = Builders<BsonDocument>.Filter.Eq("_id", "Routes");
			var update = Builders<BsonDocument>.Update.Inc("SequenceValue", 1);
			var options = new FindOneAndUpdateOptions<BsonDocument>
			{
				IsUpsert = true,
				ReturnDocument = ReturnDocument.After
			};

			var result = await counters.FindOneAndUpdateAsync(filter, update, options);
			return result["SequenceValue"].AsInt32;
		}

		public async Task UpdateAsync(Route route)
		{
			await _routesCollection.ReplaceOneAsync(r => r.Id == route.Id, route);
		}

		public async Task DeleteAsync(Route route)
		{
			await _routesCollection.DeleteOneAsync(r => r.Id == route.Id);
		}

		public async Task<Route?> GetByIdAsync(int id)
		{
			return await _routesCollection.Find(r => r.Id == id).FirstOrDefaultAsync();
		}

		public async Task<IEnumerable<Route>> GetAllAsync()
		{
			return await _routesCollection.Find(_ => true).ToListAsync();
		}

		public async Task<IEnumerable<Route>> GetActiveRoutesAsync()
		{
			var now = DateTime.UtcNow;
			return await _routesCollection.Find(r =>
				r.IsActive && r.AvailableFrom <= now && r.AvailableTo >= now)
				.ToListAsync();
		}

		public async Task<IEnumerable<Route>> GetRoutesByCompanyIdAsync(int companyId)
		{
			return await _routesCollection.Find(r => r.CompanyId == companyId).ToListAsync();
		}
	}
}
