using MongoDB.Bson;
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
			contract.Id = await GetNextIdAsync();
			await _contractsCollection.InsertOneAsync(contract);
		}

		private async Task<int> GetNextIdAsync()
		{
			var counters = _contractsCollection.Database.GetCollection<BsonDocument>("Counters");
			var filter = Builders<BsonDocument>.Filter.Eq("_id", "Contracts");
			var update = Builders<BsonDocument>.Update.Inc("SequenceValue", 1);
			var options = new FindOneAndUpdateOptions<BsonDocument>
			{
				IsUpsert = true,
				ReturnDocument = ReturnDocument.After
			};

			var result = await counters.FindOneAndUpdateAsync(filter, update, options);
			return result["SequenceValue"].AsInt32;
		}

		public async Task DeleteAsync(Contract contract)
		{
			await _contractsCollection.DeleteOneAsync(c => c.Id == contract.Id);
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

		public async Task UpdateAsync(Contract contract)
		{
			await _contractsCollection.ReplaceOneAsync(c => c.Id == contract.Id, contract);
		}
	}
}
