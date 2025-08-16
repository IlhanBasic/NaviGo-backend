using MongoDB.Bson;
using MongoDB.Driver;
using NaviGoApi.Common.DTOs;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace NaviGoApi.Infrastructure.MongoDB.Repositories
{
	public class DriverMongoRepository : IDriverRepository
	{
		private readonly IMongoCollection<Driver> _driversCollection;
		private readonly IMongoCollection<BsonDocument> _countersCollection;

		public DriverMongoRepository(IMongoDatabase database)
		{
			_driversCollection = database.GetCollection<Driver>("Drivers");
			_countersCollection = database.GetCollection<BsonDocument>("Counters");
		}

		public async Task AddAsync(Driver driver)
		{
			driver.Id = await GetNextIdAsync();
			await _driversCollection.InsertOneAsync(driver);
		}

		private async Task<int> GetNextIdAsync()
		{
			var filter = Builders<BsonDocument>.Filter.Eq("_id", "Drivers");
			var update = Builders<BsonDocument>.Update.Inc("SequenceValue", 1);
			var options = new FindOneAndUpdateOptions<BsonDocument>
			{
				IsUpsert = true,
				ReturnDocument = ReturnDocument.After
			};

			var result = await _countersCollection.FindOneAndUpdateAsync(filter, update, options);
			return result["SequenceValue"].AsInt32;
		}

		public async Task DeleteAsync(Driver driver)
		{
			var result = await _driversCollection.DeleteOneAsync(d => d.Id == driver.Id);
			if (result.DeletedCount == 0)
				throw new ValidationException($"Driver with Id {driver.Id} not found for deletion.");
		}

		public async Task<IEnumerable<Driver>> GetAllAsync()
		{
			return await _driversCollection.Find(_ => true).ToListAsync();
		}

		public async Task<IEnumerable<Driver>> GetAvailableDriversAsync()
		{
			return await _driversCollection
				.Find(d => d.DriverStatus == DriverStatus.Available)
				.ToListAsync();
		}

		public async Task<IEnumerable<Driver>> GetByCompanyIdAsync(int companyId)
		{
			return await _driversCollection
				.Find(d => d.CompanyId == companyId)
				.ToListAsync();
		}

		public async Task<Driver?> GetByIdAsync(int id)
		{
			return await _driversCollection.Find(d => d.Id == id).FirstOrDefaultAsync();
		}

		public async Task UpdateAsync(Driver driver)
		{
			var result = await _driversCollection.ReplaceOneAsync(d => d.Id == driver.Id, driver);
			if (result.MatchedCount == 0)
				throw new ValidationException($"Driver with Id {driver.Id} not found for update.");
		}

		public async Task<bool> ExistsAsync(Expression<System.Func<Driver, bool>> predicate)
		{
			return await _driversCollection.Find(predicate).AnyAsync();
		}

		public async Task<IEnumerable<Driver>> GetAllAsync(DriverSearchDto driverSearch)
		{
			var filterBuilder = Builders<Driver>.Filter;
			var filter = filterBuilder.Empty;

			if (!string.IsNullOrWhiteSpace(driverSearch.FirstName))
				filter &= filterBuilder.Regex(d => d.FirstName, new BsonRegularExpression(driverSearch.FirstName, "i"));

			if (!string.IsNullOrWhiteSpace(driverSearch.LastName))
				filter &= filterBuilder.Regex(d => d.LastName, new BsonRegularExpression(driverSearch.LastName, "i"));

			// Sortiranje
			var sortBuilder = Builders<Driver>.Sort;
			var sort = driverSearch.SortBy?.ToLower() switch
			{
				"firstname" => driverSearch.SortDirection.ToLower() == "desc" ? sortBuilder.Descending(d => d.FirstName) : sortBuilder.Ascending(d => d.FirstName),
				"lastname" => driverSearch.SortDirection.ToLower() == "desc" ? sortBuilder.Descending(d => d.LastName) : sortBuilder.Ascending(d => d.LastName),
				_ => driverSearch.SortDirection.ToLower() == "desc" ? sortBuilder.Descending(d => d.Id) : sortBuilder.Ascending(d => d.Id)
			};

			int skip = (driverSearch.Page - 1) * driverSearch.PageSize;
			int limit = driverSearch.PageSize;

			var drivers = await _driversCollection
				.Find(filter)
				.Sort(sort)
				.Skip(skip)
				.Limit(limit)
				.ToListAsync();

			return drivers;
		}

	}
}
