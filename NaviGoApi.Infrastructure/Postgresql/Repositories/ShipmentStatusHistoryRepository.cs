using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Infrastructure.Postgresql.Repositories
{
	public class ShipmentStatusHistoryRepository : IShipmentStatusHistoryRepository
	{
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

		public Task<ShipmentStatusHistory?> GetByIdAsync(Guid id)
		{
			throw new NotImplementedException();
		}

		public Task UpdateAsync(ShipmentStatusHistory history)
		{
			throw new NotImplementedException();
		}
	}
}
