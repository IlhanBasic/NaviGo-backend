using MongoDB.Driver;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;

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
			await _cargoTypesCollection.InsertOneAsync(cargoType);
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
