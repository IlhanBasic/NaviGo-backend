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
	public class CompanyMongoRepository : ICompanyRepository
	{
		private readonly IMongoCollection<Company> _companiesCollection;
		private readonly IMongoCollection<BsonDocument> _countersCollection;

		public CompanyMongoRepository(IMongoDatabase database)
		{
			_companiesCollection = database.GetCollection<Company>("Companies");
			_countersCollection = database.GetCollection<BsonDocument>("Counters");
		}

		public async Task AddAsync(Company company)
		{
			company.Id = await GetNextIdAsync();
			await _companiesCollection.InsertOneAsync(company);
		}

		private async Task<int> GetNextIdAsync()
		{
			var filter = Builders<BsonDocument>.Filter.Eq("_id", "Companies");
			var update = Builders<BsonDocument>.Update.Inc("SequenceValue", 1);
			var options = new FindOneAndUpdateOptions<BsonDocument>
			{
				IsUpsert = true,
				ReturnDocument = ReturnDocument.After
			};

			var result = await _countersCollection.FindOneAndUpdateAsync(filter, update, options);
			return result["SequenceValue"].AsInt32;
		}

		public async Task DeleteAsync(Company company)
		{
			var result = await _companiesCollection.DeleteOneAsync(c => c.Id == company.Id);
			if (result.DeletedCount == 0)
			{
				throw new ValidationException($"Company with Id {company.Id} not found for deletion.");
			}
		}

		public async Task<IEnumerable<Company>> GetAllAsync()
		{
			return await _companiesCollection.Find(_ => true).ToListAsync();
		}

		public async Task<Company?> GetByIdAsync(int id)
		{
			var company = await _companiesCollection.Find(c => c.Id == id).FirstOrDefaultAsync();
			if (company == null)
				return null;

			// Učitavamo drivere za tu kompaniju
			var driversCollection = _companiesCollection.Database.GetCollection<Driver>("Drivers");
			var drivers = await driversCollection.Find(d => d.CompanyId == id).ToListAsync();
			company.Drivers = drivers;

			return company;
		}


		public async Task<Company?> GetByNameAsync(string name)
		{
			return await _companiesCollection.Find(c => c.CompanyName == name).FirstOrDefaultAsync();
		}

		public async Task<Company?> GetByPibAsync(string pib)
		{
			return await _companiesCollection.Find(c => c.PIB == pib).FirstOrDefaultAsync();
		}

		public async Task UpdateAsync(Company company)
		{
			var result = await _companiesCollection.ReplaceOneAsync(c => c.Id == company.Id, company);
			if (result.MatchedCount == 0)
			{
				throw new ValidationException($"Company with Id {company.Id} not found for update.");
			}
		}

		public async Task<bool> ExistsAsync(Expression<Func<Company, bool>> predicate)
		{
			return await _companiesCollection.Find(predicate).AnyAsync();
		}

		public Task<IEnumerable<Company>> GetAllAsync(CompanySearchDto companySearch)
		{
			throw new NotImplementedException();
		}
	}
}
