using Microsoft.EntityFrameworkCore;
using NaviGoApi.Common.DTOs;
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

		public Task DeleteAsync(Company company)
		{
			_context.Companies.Remove(company);
			return Task.CompletedTask;
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

		public Task UpdateAsync(Company company)
		{
			_context.Companies.Update(company);
			return Task.CompletedTask;
		}
		public async Task<Company?> GetByPibAsync(string pib)
		{
			return await _context.Companies.FirstOrDefaultAsync(c => c.PIB == pib);
		}

		public async Task<IEnumerable<Company>> GetAllAsync(CompanySearchDto companySearch)
		{
			IQueryable<Company> query = _context.Companies;
			if (!string.IsNullOrWhiteSpace(companySearch.Pib))
			{
				query = query.Where(c => c.PIB.Contains(companySearch.Pib));
			}
			if (!string.IsNullOrWhiteSpace(companySearch.CompanyName))
			{
				query = query.Where(c => c.CompanyName.Contains(companySearch.CompanyName));
			}

			var sortDirection = companySearch.SortDirection?.ToLower() == "desc" ? false : true;
			query = companySearch.SortBy?.ToLower() switch
			{
				"companyname" => sortDirection ? query.OrderBy(c => c.CompanyName) : query.OrderByDescending(c => c.CompanyName),
				"pib" => sortDirection ? query.OrderBy(c => c.PIB) : query.OrderByDescending(c => c.PIB),
				"createdat" => sortDirection ? query.OrderBy(c => c.CreatedAt) : query.OrderByDescending(c => c.CreatedAt),
				_ => sortDirection ? query.OrderBy(c => c.Id) : query.OrderByDescending(c => c.Id)
			};
			var skip = (companySearch.Page - 1) * companySearch.PageSize;
			query = query.Skip(skip).Take(companySearch.PageSize);
			return await query.ToListAsync();
		}

	}
}
