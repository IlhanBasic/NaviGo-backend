using MongoDB.Driver;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NaviGoApi.Infrastructure.MongoDB.Repositories
{
	public class DriverMongoRepository : IDriverRepository
	{
		private readonly IMongoCollection<Driver> _driversCollection;

		public DriverMongoRepository(IMongoDatabase database)
		{
			_driversCollection = database.GetCollection<Driver>("Drivers");
		}

		public async Task AddAsync(Driver driver)
		{
			await _driversCollection.InsertOneAsync(driver);
		}

		public void Delete(Driver driver)
		{
			_driversCollection.DeleteOne(d => d.Id == driver.Id);
		}

		public async Task<IEnumerable<Driver>> GetAllAsync()
		{
			return await _driversCollection.Find(_ => true).ToListAsync();
		}

		public async Task<IEnumerable<Driver>> GetAvailableDriversAsync()
		{
			return await _driversCollection
				.Find(d => d.DriverStatus == DriverStatus.Available)
				.ToListAsync();
		}

		public async Task<IEnumerable<Driver>> GetByCompanyIdAsync(int companyId)
		{
			return await _driversCollection
				.Find(d => d.CompanyId == companyId)
				.ToListAsync();
		}

		public async Task<Driver?> GetByIdAsync(int id)
		{
			return await _driversCollection.Find(d => d.Id == id).FirstOrDefaultAsync();
		}

		public void Update(Driver driver)
		{
			_driversCollection.ReplaceOne(d => d.Id == driver.Id, driver);
		}
	}
}
