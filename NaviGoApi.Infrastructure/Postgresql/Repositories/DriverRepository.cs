using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Infrastructure.Postgresql.Repositories
{
	public class DriverRepository : IDriverRepository
	{
		public Task AddAsync(Driver driver)
		{
			throw new NotImplementedException();
		}

		public void Delete(Driver driver)
		{
			throw new NotImplementedException();
		}

		public Task<IEnumerable<Driver>> GetAllAsync()
		{
			throw new NotImplementedException();
		}

		public Task<IEnumerable<Driver>> GetAvailableDriversAsync()
		{
			throw new NotImplementedException();
		}

		public Task<IEnumerable<Driver>> GetByCompanyIdAsync(int companyId)
		{
			throw new NotImplementedException();
		}

		public Task<Driver?> GetByIdAsync(int id)
		{
			throw new NotImplementedException();
		}

		public void Update(Driver driver)
		{
			throw new NotImplementedException();
		}
	}
}
