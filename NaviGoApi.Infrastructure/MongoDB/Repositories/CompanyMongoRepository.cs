using MongoDB.Bson;
using MongoDB.Driver;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NaviGoApi.Infrastructure.MongoDB.Repositories
{
	public class CompanyMongoRepository : ICompanyRepository
	{
		private readonly IMongoCollection<Company> _companiesCollection;

		public CompanyMongoRepository(IMongoDatabase database)
		{
			_companiesCollection = database.GetCollection<Company>("Companies");
		}

		public async Task AddAsync(Company company)
		{
			company.Id = await GetNextIdAsync();
			await _companiesCollection.InsertOneAsync(company);
		}

		private async Task<int> GetNextIdAsync()
		{
			var counters = _companiesCollection.Database.GetCollection<BsonDocument>("Counters");
			var filter = Builders<BsonDocument>.Filter.Eq("_id", "Companies");
			var update = Builders<BsonDocument>.Update.Inc("SequenceValue", 1);
			var options = new FindOneAndUpdateOptions<BsonDocument>
			{
				IsUpsert = true,
				ReturnDocument = ReturnDocument.After
			};

			var result = await counters.FindOneAndUpdateAsync(filter, update, options);
			return result["SequenceValue"].AsInt32;
		}

		public void Delete(Company company)
		{
			_companiesCollection.DeleteOne(c => c.Id == company.Id);
		}

		public async Task<IEnumerable<Company>> GetAllAsync()
		{
			return await _companiesCollection.Find(_ => true).ToListAsync();
		}

		public async Task<Company?> GetByIdAsync(int id)
		{
			return await _companiesCollection.Find(c => c.Id == id).FirstOrDefaultAsync();
		}

		public async Task<Company?> GetByNameAsync(string name)
		{
			return await _companiesCollection.Find(c => c.CompanyName == name).FirstOrDefaultAsync();
		}

		public async Task<Company?> GetByPibAsync(string pib)
		{
			return await _companiesCollection.Find(c => c.PIB == pib).FirstOrDefaultAsync();
		}

		public void Update(Company company)
		{
			_companiesCollection.ReplaceOne(c => c.Id == company.Id, company);
		}
	}
}
