using Microsoft.EntityFrameworkCore;
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

	public void Delete(Driver driver)
	{
		_context.Drivers.Remove(driver);
	}

	public async Task<IEnumerable<Driver>> GetAllAsync()
	{
		return await _context.Drivers.ToListAsync();
	}

	public async Task<IEnumerable<Driver>> GetAvailableDriversAsync()
	{
		return await _context.Drivers.ToListAsync();
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

	public void Update(Driver driver)
	{
		_context.Drivers.Update(driver);
	}
}
