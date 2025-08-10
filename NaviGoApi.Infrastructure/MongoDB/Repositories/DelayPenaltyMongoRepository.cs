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
			await _delayPenaltiesCollection.InsertOneAsync(penalty);
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
