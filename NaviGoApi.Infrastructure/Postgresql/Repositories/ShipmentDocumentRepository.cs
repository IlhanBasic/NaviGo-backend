using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Infrastructure.Postgresql.Repositories
{
	public class ShipmentDocumentRepository : IShipmentDocumentRepository
	{
		public Task AddAsync(ShipmentDocument document)
		{
			throw new NotImplementedException();
		}

		public Task DeleteAsync(int id)
		{
			throw new NotImplementedException();
		}

		public Task<IEnumerable<ShipmentDocument>> GetAllAsync()
		{
			throw new NotImplementedException();
		}

		public Task<ShipmentDocument?> GetByIdAsync(Guid id)
		{
			throw new NotImplementedException();
		}

		public Task UpdateAsync(ShipmentDocument document)
		{
			throw new NotImplementedException();
		}
	}
}
