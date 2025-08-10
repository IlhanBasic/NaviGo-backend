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
			await _companiesCollection.InsertOneAsync(company);
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
