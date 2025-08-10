using MongoDB.Driver;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NaviGoApi.Infrastructure.MongoDB.Repositories
{
	public class ContractMongoRepository : IContractRepository
	{
		private readonly IMongoCollection<Contract> _contractsCollection;

		public ContractMongoRepository(IMongoDatabase database)
		{
			_contractsCollection = database.GetCollection<Contract>("Contracts");
		}

		public async Task AddAsync(Contract contract)
		{
			await _contractsCollection.InsertOneAsync(contract);
		}

		public void Delete(Contract contract)
		{
			_contractsCollection.DeleteOne(c => c.Id == contract.Id);
		}

		public async Task<IEnumerable<Contract>> GetAllAsync()
		{
			return await _contractsCollection.Find(_ => true).ToListAsync();
		}

		public async Task<IEnumerable<Contract>> GetByClientIdAsync(int clientId)
		{
			return await _contractsCollection.Find(c => c.ClientId == clientId).ToListAsync();
		}

		public async Task<IEnumerable<Contract>> GetByForwarderIdAsync(int forwarderId)
		{
			return await _contractsCollection.Find(c => c.ForwarderId == forwarderId).ToListAsync();
		}

		public async Task<Contract?> GetByIdAsync(int id)
		{
			return await _contractsCollection.Find(c => c.Id == id).FirstOrDefaultAsync();
		}

		public void Update(Contract contract)
		{
			_contractsCollection.ReplaceOne(c => c.Id == contract.Id, contract);
		}
	}
}
