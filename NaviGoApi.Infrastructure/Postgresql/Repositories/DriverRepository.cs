using Microsoft.EntityFrameworkCore;
using NaviGoApi.Common.DTOs;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using NaviGoApi.Infrastructure.Postgresql.Persistence;

public class DriverRepository : IDriverRepository
{
	private readonly ApplicationDbContext _context;

	public DriverRepository(ApplicationDbContext context)
	{
		_context = context;
	}

	public async Task AddAsync(Driver driver)
	{
		await _context.Drivers.AddAsync(driver);
	}

	public Task DeleteAsync(Driver driver)
	{
		_context.Drivers.Remove(driver);
		return Task.CompletedTask;
	}

	public async Task<IEnumerable<Driver>> GetAllAsync()
	{
		return await _context.Drivers.ToListAsync();
	}

	public async Task<IEnumerable<Driver>> GetAllAsync(DriverSearchDto driverSearch)
	{
		var query = _context.Drivers.AsQueryable();

		// Filteri
		if (!string.IsNullOrWhiteSpace(driverSearch.FirstName))
			query = query.Where(d => d.FirstName.Contains(driverSearch.FirstName));

		if (!string.IsNullOrWhiteSpace(driverSearch.LastName))
			query = query.Where(d => d.LastName.Contains(driverSearch.LastName));

		// Sortiranje
		query = driverSearch.SortBy?.ToLower() switch
		{
			"firstname" => driverSearch.SortDirection.ToLower() == "desc"
				? query.OrderByDescending(d => d.FirstName)
				: query.OrderBy(d => d.FirstName),

			"lastname" => driverSearch.SortDirection.ToLower() == "desc"
				? query.OrderByDescending(d => d.LastName)
				: query.OrderBy(d => d.LastName),

			_ => driverSearch.SortDirection.ToLower() == "desc"
				? query.OrderByDescending(d => d.Id)
				: query.OrderBy(d => d.Id),
		};

		// Paging
		var skip = (driverSearch.Page - 1) * driverSearch.PageSize;
		query = query.Skip(skip).Take(driverSearch.PageSize);

		return await query.ToListAsync();
	}


	public async Task<IEnumerable<Driver>> GetAvailableDriversAsync(int companyId)
	{
		return await _context.Drivers.Where(d=>d.DriverStatus == DriverStatus.Available && d.CompanyId==companyId).ToListAsync();
	}

	public async Task<IEnumerable<Driver>> GetByCompanyIdAsync(int companyId)
	{
		return await _context.Drivers
			.Where(d => d.CompanyId == companyId)
			.ToListAsync();
	}

	public async Task<Driver?> GetByIdAsync(int id)
	{
		return await _context.Drivers.FindAsync(id);
	}

	public Task UpdateAsync(Driver driver)
	{
		_context.Drivers.Update(driver);
		return Task.CompletedTask;
	}
}
