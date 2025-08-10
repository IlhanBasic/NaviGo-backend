using MongoDB.Bson;
using MongoDB.Driver;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NaviGoApi.Infrastructure.MongoDB.Repositories
{
	public class CargoTypeMongoRepository : ICargoTypeRepository
	{
		private readonly IMongoCollection<CargoType> _cargoTypesCollection;

		public CargoTypeMongoRepository(IMongoDatabase database)
		{
			_cargoTypesCollection = database.GetCollection<CargoType>("CargoTypes");
		}

		public async Task AddAsync(CargoType cargoType)
		{
			cargoType.Id = await GetNextIdAsync();
			await _cargoTypesCollection.InsertOneAsync(cargoType);
		}

		private async Task<int> GetNextIdAsync()
		{
			var counters = _cargoTypesCollection.Database.GetCollection<BsonDocument>("Counters");
			var filter = Builders<BsonDocument>.Filter.Eq("_id", "CargoTypes");
			var update = Builders<BsonDocument>.Update.Inc("SequenceValue", 1);
			var options = new FindOneAndUpdateOptions<BsonDocument>
			{
				IsUpsert = true,
				ReturnDocument = ReturnDocument.After
			};

			var result = await counters.FindOneAndUpdateAsync(filter, update, options);
			return result["SequenceValue"].AsInt32;
		}

		public void Delete(CargoType cargoType)
		{
			_cargoTypesCollection.DeleteOne(ct => ct.Id == cargoType.Id);
		}

		public async Task<IEnumerable<CargoType>> GetAllAsync()
		{
			return await _cargoTypesCollection.Find(_ => true).ToListAsync();
		}

		public async Task<CargoType?> GetByIdAsync(int id)
		{
			return await _cargoTypesCollection.Find(ct => ct.Id == id).FirstOrDefaultAsync();
		}

		public void Update(CargoType cargoType)
		{
			_cargoTypesCollection.ReplaceOne(ct => ct.Id == cargoType.Id, cargoType);
		}
	}
}
