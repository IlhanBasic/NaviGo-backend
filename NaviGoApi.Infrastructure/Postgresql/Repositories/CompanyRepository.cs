using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Infrastructure.Postgresql.Repositories
{
	public class CompanyRepository : ICompanyRepository
	{
		public Task AddAsync(Company company)
		{
			throw new NotImplementedException();
		}

		public void Delete(Company company)
		{
			throw new NotImplementedException();
		}

		public Task<IEnumerable<Company>> GetAllAsync()
		{
			throw new NotImplementedException();
		}

		public Task<Company?> GetByIdAsync(int id)
		{
			throw new NotImplementedException();
		}

		public Task<Company?> GetByNameAsync(string name)
		{
			throw new NotImplementedException();
		}

		public void Update(Company company)
		{
			throw new NotImplementedException();
		}
	}
}
