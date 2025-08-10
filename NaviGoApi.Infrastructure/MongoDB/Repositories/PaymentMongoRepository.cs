using MongoDB.Bson;
using MongoDB.Driver;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NaviGoApi.Infrastructure.MongoDB.Repositories
{
	public class PaymentMongoRepository : IPaymentRepository
	{
		private readonly IMongoCollection<Payment> _paymentsCollection;

		public PaymentMongoRepository(IMongoDatabase database)
		{
			_paymentsCollection = database.GetCollection<Payment>("Payments");
		}

		public async Task AddAsync(Payment payment)
		{
			payment.Id = await GetNextIdAsync();
			await _paymentsCollection.InsertOneAsync(payment);
		}

		private async Task<int> GetNextIdAsync()
		{
			var counters = _paymentsCollection.Database.GetCollection<BsonDocument>("Counters");
			var filter = Builders<BsonDocument>.Filter.Eq("_id", "Payments");
			var update = Builders<BsonDocument>.Update.Inc("SequenceValue", 1);
			var options = new FindOneAndUpdateOptions<BsonDocument>
			{
				IsUpsert = true,
				ReturnDocument = ReturnDocument.After
			};

			var result = await counters.FindOneAndUpdateAsync(filter, update, options);
			return result["SequenceValue"].AsInt32;
		}

		public void Delete(Payment payment)
		{
			_paymentsCollection.DeleteOne(p => p.Id == payment.Id);
		}

		public async Task<IEnumerable<Payment>> GetAllAsync()
		{
			return await _paymentsCollection.Find(_ => true).ToListAsync();
		}

		public async Task<IEnumerable<Payment>> GetByClientIdAsync(int clientId)
		{
			return await _paymentsCollection
				.Find(p => p.ClientId == clientId)
				.ToListAsync();
		}

		public async Task<IEnumerable<Payment>> GetByContractIdAsync(int contractId)
		{
			return await _paymentsCollection
				.Find(p => p.ContractId == contractId)
				.ToListAsync();
		}

		public async Task<Payment?> GetByIdAsync(int id)
		{
			return await _paymentsCollection
				.Find(p => p.Id == id)
				.FirstOrDefaultAsync();
		}

		public async Task<IEnumerable<Payment>> GetPendingPaymentsAsync()
		{
			return await _paymentsCollection
				.Find(p => p.PaymentStatus == PaymentStatus.Pending)
				.ToListAsync();
		}

		public void Update(Payment payment)
		{
			_paymentsCollection.ReplaceOne(p => p.Id == payment.Id, payment);
		}
	}
}
