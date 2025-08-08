using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using NaviGoApi.Infrastructure.Postgresql.Persistence; 
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace NaviGoApi.Infrastructure.Postgresql.Repositories
{
	public class VehicleRepository : IVehicleRepository
	{
		private readonly ApplicationDbContext _context;

		public VehicleRepository(ApplicationDbContext context)
		{
			_context = context;
		}

		public async Task AddAsync(Vehicle vehicle)
		{
			await _context.Vehicles.AddAsync(vehicle);
		}

		public void Delete(Vehicle vehicle)
		{
			_context.Vehicles.Remove(vehicle);
		}

		public async Task<IEnumerable<Vehicle>> GetAllAsync()
		{
			// Bez Include, vraća osnovne podatke o vozilima
			return await _context.Vehicles.AsNoTracking().ToListAsync();
		}

		public async Task<IEnumerable<Vehicle>> GetAvailableVehiclesAsync()
		{
			return await _context.Vehicles
				.Where(v => v.VehicleStatus==VehicleStatus.Free)
				.AsNoTracking()
				.ToListAsync();
		}

		public async Task<IEnumerable<Vehicle>> GetByCompanyIdAsync(int companyId)
		{
			return await _context.Vehicles
				.Where(v => v.CompanyId == companyId)
				.AsNoTracking()
				.ToListAsync();
		}

		public async Task<Vehicle?> GetByIdAsync(int id)
		{
			return await _context.Vehicles
				.AsNoTracking()
				.FirstOrDefaultAsync(v => v.Id == id);
		}

		public void Update(Vehicle vehicle)
		{
			_context.Vehicles.Update(vehicle);
		}
	}
}
