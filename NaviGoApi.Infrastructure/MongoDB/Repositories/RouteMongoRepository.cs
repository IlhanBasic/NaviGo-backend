using MongoDB.Bson;
using MongoDB.Driver;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
			var route = await _routesCollection.Find(r => r.Id == id).FirstOrDefaultAsync();
			if (route != null)
				await LoadNavigationPropertiesAsync(new List<Route> { route });
			return route;
		}

		public async Task<IEnumerable<Route>> GetAllAsync()
		{
			var routes = await _routesCollection.Find(_ => true).ToListAsync();
			return await LoadNavigationPropertiesAsync(routes);
		}

		public async Task<IEnumerable<Route>> GetActiveRoutesAsync()
		{
			var now = DateTime.UtcNow;
			var routes = await _routesCollection.Find(r =>
				r.IsActive && r.AvailableFrom <= now && r.AvailableTo >= now).ToListAsync();
			return await LoadNavigationPropertiesAsync(routes);
		}

		public async Task<IEnumerable<Route>> GetRoutesByCompanyIdAsync(int companyId)
		{
			var routes = await _routesCollection.Find(r => r.CompanyId == companyId).ToListAsync();
			return await LoadNavigationPropertiesAsync(routes);
		}

		public async Task<bool> ExistsAsync(Expression<Func<Route, bool>> predicate)
		{
			return await _routesCollection.Find(predicate).AnyAsync();
		}

		private async Task<IEnumerable<Route>> LoadNavigationPropertiesAsync(IEnumerable<Route> routes)
		{
			var list = routes.ToList();
			if (!list.Any()) return list;

			var db = _routesCollection.Database;

			var companiesCollection = db.GetCollection<Company>("Companies");
			var locationsCollection = db.GetCollection<Location>("Locations");

			foreach (var route in list)
			{
				if (route.CompanyId != 0)
					route.Company = await companiesCollection.Find(c => c.Id == route.CompanyId).FirstOrDefaultAsync();
				if (route.StartLocationId != 0)
					route.StartLocation = await locationsCollection.Find(l => l.Id == route.StartLocationId).FirstOrDefaultAsync();
				if (route.EndLocationId != 0)
					route.EndLocation = await locationsCollection.Find(l => l.Id == route.EndLocationId).FirstOrDefaultAsync();
			}

			return list;
		}

		public async Task<bool> DuplicateRoute(int companyId, int startLocationId, int endLocationId)
		{
			return await _routesCollection
				.Find(r => r.CompanyId == companyId
						 && r.StartLocationId == startLocationId
						 && r.EndLocationId == endLocationId)
				.AnyAsync();
		}

		public async Task<bool> DuplicateRouteUpdate(int companyId, int startLocationId, int endLocationId, int routeId)
		{
			return await _routesCollection
				.Find(r => r.CompanyId == companyId
						 && r.StartLocationId == startLocationId
						 && r.EndLocationId == endLocationId
						 && r.Id != routeId)
				.AnyAsync();
		}
	}
}
