using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Infrastructure.Postgresql.Repositories
{
	public class ContractRepository : IContractRepository
	{
		public Task AddAsync(Contract contract)
		{
			throw new NotImplementedException();
		}

		public void Delete(Contract contract)
		{
			throw new NotImplementedException();
		}

		public Task<IEnumerable<Contract>> GetAllAsync()
		{
			throw new NotImplementedException();
		}

		public Task<IEnumerable<Contract>> GetByClientIdAsync(int clientId)
		{
			throw new NotImplementedException();
		}

		public Task<IEnumerable<Contract>> GetByForwarderIdAsync(int forwarderId)
		{
			throw new NotImplementedException();
		}

		public Task<Contract?> GetByIdAsync(int id)
		{
			throw new NotImplementedException();
		}

		public void Update(Contract contract)
		{
			throw new NotImplementedException();
		}
	}
}
