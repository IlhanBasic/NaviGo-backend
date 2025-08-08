using Microsoft.EntityFrameworkCore;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using NaviGoApi.Infrastructure.Postgresql.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NaviGoApi.Infrastructure.Postgresql.Repositories
{
	public class RouteRepository : IRouteRepository
	{
		private readonly ApplicationDbContext _context;

		public RouteRepository(ApplicationDbContext context)
		{
			_context = context;
		}

		public async Task AddAsync(Route route)
		{
			await _context.Routes.AddAsync(route);
		}

		public void Update(Route route)
		{
			_context.Routes.Update(route);
		}

		public void Delete(Route route)
		{
			_context.Routes.Remove(route);
		}

		public async Task<Route?> GetByIdAsync(int id)
		{
			return await _context.Routes
				.Include(r => r.Company)
				.Include(r => r.StartLocation)
				.Include(r => r.EndLocation)
				.FirstOrDefaultAsync(r => r.Id == id);
		}

		public async Task<IEnumerable<Route>> GetAllAsync()
		{
			return await _context.Routes
				.Include(r => r.Company)
				.Include(r => r.StartLocation)
				.Include(r => r.EndLocation)
				.ToListAsync();
		}

		public async Task<IEnumerable<Route>> GetActiveRoutesAsync()
		{
			return await _context.Routes
				.Where(r => r.IsActive && r.AvailableFrom <= DateTime.UtcNow && r.AvailableTo >= DateTime.UtcNow)
				.Include(r => r.Company)
				.Include(r => r.StartLocation)
				.Include(r => r.EndLocation)
				.ToListAsync();
		}

		public async Task<IEnumerable<Route>> GetRoutesByCompanyIdAsync(int companyId)
		{
			return await _context.Routes
				.Where(r => r.CompanyId == companyId)
				.Include(r => r.Company)
				.Include(r => r.StartLocation)
				.Include(r => r.EndLocation)
				.ToListAsync();
		}
	}
}
