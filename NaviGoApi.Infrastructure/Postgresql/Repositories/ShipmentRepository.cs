using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Infrastructure.Postgresql.Repositories
{
	public class ShipmentRepository : IShipmentRepository
	{
		public Task AddAsync(Shipment shipment)
		{
			throw new NotImplementedException();
		}

		public void Delete(Shipment shipment)
		{
			throw new NotImplementedException();
		}

		public Task<IEnumerable<Shipment>> GetAllAsync()
		{
			throw new NotImplementedException();
		}

		public Task<IEnumerable<Shipment>> GetByContractIdAsync(int contractId)
		{
			throw new NotImplementedException();
		}

		public Task<Shipment?> GetByIdAsync(int id)
		{
			throw new NotImplementedException();
		}

		public Task<IEnumerable<Shipment>> GetByStatusAsync(ShipmentStatus status)
		{
			throw new NotImplementedException();
		}

		public void Update(Shipment shipment)
		{
			throw new NotImplementedException();
		}
	}
}
