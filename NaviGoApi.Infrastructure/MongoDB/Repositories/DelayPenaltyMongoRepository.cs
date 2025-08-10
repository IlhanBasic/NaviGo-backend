using MongoDB.Bson;
using MongoDB.Driver;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NaviGoApi.Infrastructure.MongoDB.Repositories
{
	public class DelayPenaltyMongoRepository : IDelayPenaltyRepository
	{
		private readonly IMongoCollection<DelayPenalty> _delayPenaltiesCollection;

		public DelayPenaltyMongoRepository(IMongoDatabase database)
		{
			_delayPenaltiesCollection = database.GetCollection<DelayPenalty>("DelayPenalties");
		}

		public async Task AddAsync(DelayPenalty penalty)
		{
			penalty.Id = await GetNextIdAsync();
			await _delayPenaltiesCollection.InsertOneAsync(penalty);
		}

		private async Task<int> GetNextIdAsync()
		{
			var counters = _delayPenaltiesCollection.Database.GetCollection<BsonDocument>("Counters");
			var filter = Builders<BsonDocument>.Filter.Eq("_id", "DelayPenalties");
			var update = Builders<BsonDocument>.Update.Inc("SequenceValue", 1);
			var options = new FindOneAndUpdateOptions<BsonDocument>
			{
				IsUpsert = true,
				ReturnDocument = ReturnDocument.After
			};

			var result = await counters.FindOneAndUpdateAsync(filter, update, options);
			return result["SequenceValue"].AsInt32;
		}

		public async Task DeleteAsync(int id)
		{
			await _delayPenaltiesCollection.DeleteOneAsync(dp => dp.Id == id);
		}

		public async Task<IEnumerable<DelayPenalty>> GetAllAsync()
		{
			return await _delayPenaltiesCollection.Find(_ => true).ToListAsync();
		}

		public async Task<DelayPenalty?> GetByIdAsync(int id)
		{
			return await _delayPenaltiesCollection.Find(dp => dp.Id == id).FirstOrDefaultAsync();
		}

		public async Task UpdateAsync(DelayPenalty penalty)
		{
			await _delayPenaltiesCollection.ReplaceOneAsync(dp => dp.Id == penalty.Id, penalty);
		}
	}
}
