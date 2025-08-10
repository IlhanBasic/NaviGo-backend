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
			await _routesCollection.InsertOneAsync(route);
		}

		// Ovde je sinhroni potpis metode, ali se asinhroni poziv "blokira"
		public void Update(Route route)
		{
			_routesCollection.ReplaceOneAsync(r => r.Id == route.Id, route)
				.GetAwaiter().GetResult();
		}

		public void Delete(Route route)
		{
			_routesCollection.DeleteOneAsync(r => r.Id == route.Id)
				.GetAwaiter().GetResult();
		}

		public async Task<Route?> GetByIdAsync(int id)
		{
			// MongoDB nema Include, ako ti treba Company itd. moraš posebno dohvatiti
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
