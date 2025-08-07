using Microsoft.EntityFrameworkCore;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using NaviGoApi.Infrastructure.Postgresql.Persistence; 
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NaviGoApi.Infrastructure.Postgresql.Repositories
{
	public class CompanyRepository : ICompanyRepository
	{
		private readonly ApplicationDbContext _context;

		public CompanyRepository(ApplicationDbContext context)
		{
			_context = context;
		}

		public async Task AddAsync(Company company)
		{
			await _context.Companies.AddAsync(company);
		}

		public void Delete(Company company)
		{
			_context.Companies.Remove(company);
		}

		public async Task<IEnumerable<Company>> GetAllAsync()
		{
			return await _context.Companies.ToListAsync();
		}

		public async Task<Company?> GetByIdAsync(int id)
		{
			return await _context.Companies.FindAsync(id);
		}

		public async Task<Company?> GetByNameAsync(string name)
		{
			return await _context.Companies.FirstOrDefaultAsync(c => c.CompanyName == name);
		}

		public void Update(Company company)
		{
			_context.Companies.Update(company);
		}
	}
}
