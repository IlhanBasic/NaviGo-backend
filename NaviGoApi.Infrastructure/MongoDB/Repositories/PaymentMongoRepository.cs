using MongoDB.Bson;
using MongoDB.Driver;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NaviGoApi.Infrastructure.MongoDB.Repositories
{
	public class PaymentMongoRepository : IPaymentRepository
	{
		private readonly IMongoCollection<Payment> _paymentsCollection;
		private readonly IMongoCollection<BsonDocument> _countersCollection;

		public PaymentMongoRepository(IMongoDatabase database)
		{
			_paymentsCollection = database.GetCollection<Payment>("Payments");
			_countersCollection = database.GetCollection<BsonDocument>("Counters");
		}

		public async Task AddAsync(Payment payment)
		{
			payment.Id = await GetNextIdAsync();
			await _paymentsCollection.InsertOneAsync(payment);
		}

		private async Task<int> GetNextIdAsync()
		{
			var filter = Builders<BsonDocument>.Filter.Eq("_id", "Payments");
			var update = Builders<BsonDocument>.Update.Inc("SequenceValue", 1);
			var options = new FindOneAndUpdateOptions<BsonDocument>
			{
				IsUpsert = true,
				ReturnDocument = ReturnDocument.After
			};

			var result = await _countersCollection.FindOneAndUpdateAsync(filter, update, options);
			return result["SequenceValue"].AsInt32;
		}

		public async Task DeleteAsync(Payment payment)
		{
			var result = await _paymentsCollection.DeleteOneAsync(p => p.Id == payment.Id);
			if (result.DeletedCount == 0)
				throw new ValidationException($"Payment with Id {payment.Id} not found for deletion.");
		}

		public async Task<IEnumerable<Payment>> GetAllAsync()
		{
			var payments = await _paymentsCollection.Find(_ => true).ToListAsync();
			return await PopulateRelationsAsync(payments);
		}

		public async Task<Payment?> GetByIdAsync(int id)
		{
			var payment = await _paymentsCollection.Find(p => p.Id == id).FirstOrDefaultAsync();
			if (payment == null) return null;

			var list = await PopulateRelationsAsync(new List<Payment> { payment });
			return list.FirstOrDefault();
		}

		public async Task<IEnumerable<Payment>> GetByClientIdAsync(int clientId)
		{
			var payments = await _paymentsCollection.Find(p => p.ClientId == clientId).ToListAsync();
			return await PopulateRelationsAsync(payments);
		}

		public async Task<IEnumerable<Payment>> GetByContractIdAsync(int contractId)
		{
			var payments = await _paymentsCollection.Find(p => p.ContractId == contractId).ToListAsync();
			return await PopulateRelationsAsync(payments);
		}

		public async Task<IEnumerable<Payment>> GetPendingPaymentsAsync()
		{
			var payments = await _paymentsCollection.Find(p => p.PaymentStatus == PaymentStatus.Pending).ToListAsync();
			return await PopulateRelationsAsync(payments);
		}

		public async Task UpdateAsync(Payment payment)
		{
			var result = await _paymentsCollection.ReplaceOneAsync(p => p.Id == payment.Id, payment);
			if (result.MatchedCount == 0)
				throw new ValidationException($"Payment with Id {payment.Id} not found for update.");
		}

		// Helper za učitavanje navigacionih property-ja
		private async Task<IEnumerable<Payment>> PopulateRelationsAsync(IEnumerable<Payment> payments)
		{
			var list = payments.ToList();
			if (!list.Any()) return list;

			var db = _paymentsCollection.Database;
			var usersCollection = db.GetCollection<User>("Users");
			var contractsCollection = db.GetCollection<Contract>("Contracts");

			foreach (var payment in list)
			{
				if (payment.ClientId != 0)
					payment.Client = await usersCollection.Find(u => u.Id == payment.ClientId).FirstOrDefaultAsync();

				if (payment.ContractId != 0)
					payment.Contract = await contractsCollection.Find(c => c.Id == payment.ContractId).FirstOrDefaultAsync();
			}

			return list;
		}
	}
}
