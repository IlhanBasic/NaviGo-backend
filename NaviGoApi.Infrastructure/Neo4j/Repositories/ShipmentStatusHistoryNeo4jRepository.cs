using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Neo4j.Driver;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;

namespace NaviGoApi.Infrastructure.Neo4j.Repositories
{
	public class ShipmentStatusHistoryNeo4jRepository : IShipmentStatusHistoryRepository
	{
		private readonly IDriver _driver;

		public ShipmentStatusHistoryNeo4jRepository(IDriver driver)
		{
			_driver = driver;
		}

		public Task AddAsync(ShipmentStatusHistory history)
		{
			throw new NotImplementedException();
		}

		public Task DeleteAsync(int id)
		{
			throw new NotImplementedException();
		}

		public Task<IEnumerable<ShipmentStatusHistory>> GetAllAsync()
		{
			throw new NotImplementedException();
		}

		public Task<ShipmentStatusHistory?> GetByIdAsync(int id)
		{
			throw new NotImplementedException();
		}

		public Task UpdateAsync(ShipmentStatusHistory history)
		{
			throw new NotImplementedException();
		}
	}
}
