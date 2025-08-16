using MongoDB.Bson;
using MongoDB.Driver;
using NaviGoApi.Common.DTOs;
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
	public class ContractMongoRepository : IContractRepository
	{
		private readonly IMongoCollection<Contract> _contractsCollection;
		private readonly IMongoCollection<BsonDocument> _countersCollection;

		public ContractMongoRepository(IMongoDatabase database)
		{
			_contractsCollection = database.GetCollection<Contract>("Contracts");
			_countersCollection = database.GetCollection<BsonDocument>("Counters");
		}

		public async Task AddAsync(Contract contract)
		{
			contract.Id = await GetNextIdAsync();
			await _contractsCollection.InsertOneAsync(contract);
		}

		private async Task<int> GetNextIdAsync()
		{
			var filter = Builders<BsonDocument>.Filter.Eq("_id", "Contracts");
			var update = Builders<BsonDocument>.Update.Inc("SequenceValue", 1);
			var options = new FindOneAndUpdateOptions<BsonDocument>
			{
				IsUpsert = true,
				ReturnDocument = ReturnDocument.After
			};

			var result = await _countersCollection.FindOneAndUpdateAsync(filter, update, options);
			return result["SequenceValue"].AsInt32;
		}

		public async Task DeleteAsync(Contract contract)
		{
			var result = await _contractsCollection.DeleteOneAsync(c => c.Id == contract.Id);
			if (result.DeletedCount == 0)
			{
				throw new ValidationException($"Contract with Id {contract.Id} not found for deletion.");
			}
		}

		public async Task<IEnumerable<Contract>> GetAllAsync()
		{
			var contracts = await _contractsCollection.Find(_ => true).ToListAsync();
			return await PopulateRelationsAsync(contracts);
		}

		public async Task<IEnumerable<Contract>> GetByClientIdAsync(int clientId)
		{
			var contracts = await _contractsCollection.Find(c => c.ClientId == clientId).ToListAsync();
			return await PopulateRelationsAsync(contracts);
		}

		public async Task<IEnumerable<Contract>> GetByForwarderIdAsync(int forwarderId)
		{
			var contracts = await _contractsCollection.Find(c => c.ForwarderId == forwarderId).ToListAsync();
			return await PopulateRelationsAsync(contracts);
		}

		public async Task<Contract?> GetByIdAsync(int id)
		{
			var contract = await _contractsCollection.Find(c => c.Id == id).FirstOrDefaultAsync();
			if (contract == null) return null;

			var populated = await PopulateRelationsAsync(new List<Contract> { contract });
			return populated.FirstOrDefault();
		}

		public async Task UpdateAsync(Contract contract)
		{
			var result = await _contractsCollection.ReplaceOneAsync(c => c.Id == contract.Id, contract);
			if (result.MatchedCount == 0)
			{
				throw new ValidationException($"Contract with Id {contract.Id} not found for update.");
			}
		}

		public async Task<bool> ExistsAsync(Expression<Func<Contract, bool>> predicate)
		{
			return await _contractsCollection.Find(predicate).AnyAsync();
		}

		// Helper method da popuni sve relacije
		private async Task<IEnumerable<Contract>> PopulateRelationsAsync(IEnumerable<Contract> contracts)
		{
			var contractList = contracts.ToList();
			if (!contractList.Any()) return contractList;

			var db = _contractsCollection.Database;

			var usersCollection = db.GetCollection<User>("Users");
			var companiesCollection = db.GetCollection<Company>("Companies");
			var paymentsCollection = db.GetCollection<Payment>("Payments");
			var routesCollection = db.GetCollection<Route>("Routes");
			var locationsCollection = db.GetCollection<Location>("Locations");
			var routePricesCollection = db.GetCollection<RoutePrice>("RoutePrices");

			foreach (var contract in contractList)
			{
				if (contract.ClientId != 0)
					contract.Client = await usersCollection.Find(u => u.Id == contract.ClientId).FirstOrDefaultAsync();

				if (contract.ForwarderId != 0)
					contract.Forwarder = await companiesCollection.Find(f => f.Id == contract.ForwarderId).FirstOrDefaultAsync();

				contract.Payment = await paymentsCollection.Find(p => p.ContractId == contract.Id).FirstOrDefaultAsync();

				if (contract.RouteId != 0)
				{
					contract.Route = await routesCollection.Find(r => r.Id == contract.RouteId).FirstOrDefaultAsync();
					if (contract.Route != null)
					{
						contract.Route.StartLocation = await locationsCollection.Find(l => l.Id == contract.Route.StartLocationId).FirstOrDefaultAsync();
						contract.Route.EndLocation = await locationsCollection.Find(l => l.Id == contract.Route.EndLocationId).FirstOrDefaultAsync();

						contract.Route.RoutePrices = await routePricesCollection
							.Find(rp => rp.RouteId == contract.Route.Id)
							.ToListAsync();
					}
				}
			}

			return contractList;
		}

		public async Task<bool> DuplicateContract(string contractNumber)
		{
			return await _contractsCollection
				.Find(c => c.ContractNumber == contractNumber)
				.AnyAsync();
		}

		public Task<IEnumerable<Contract>> GetAllAsync(ContractSearchDto contractSearch)
		{
			throw new NotImplementedException();
		}
	}
}
