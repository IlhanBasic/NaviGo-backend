using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using NaviGoApi.Infrastructure.Postgresql.Persistence; 
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;
using NaviGoApi.Common.DTOs;

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

		public Task DeleteAsync(Vehicle vehicle)
		{
			_context.Vehicles.Remove(vehicle);
			return Task.CompletedTask;
		}

		public async Task<IEnumerable<Vehicle>> GetAllAsync()
		{
			return await _context.Vehicles.ToListAsync();
		}

		public async Task<IEnumerable<Vehicle>> GetAllAsync(VehicleSearchDto vehicleSearch)
		{
			IQueryable<Vehicle> query = _context.Vehicles.AsNoTracking();

			// Filtriranje po brendu
			if (!string.IsNullOrWhiteSpace(vehicleSearch.Brand))
				query = query.Where(v => v.Brand.Contains(vehicleSearch.Brand));

			// Dinamičko sortiranje bez reflection-a
			if (!string.IsNullOrWhiteSpace(vehicleSearch.SortBy))
			{
				bool descending = vehicleSearch.SortDirection?.ToLower() == "desc";

				query = vehicleSearch.SortBy.ToLower() switch
				{
					"brand" => descending ? query.OrderByDescending(v => v.Brand) : query.OrderBy(v => v.Brand),
					"model" => descending ? query.OrderByDescending(v => v.Model) : query.OrderBy(v => v.Model),
					"year" => descending ? query.OrderByDescending(v => v.ManufactureYear) : query.OrderBy(v => v.ManufactureYear),
					"id" => descending ? query.OrderByDescending(v => v.Id) : query.OrderBy(v => v.Id),
					"registrationnumber" => descending ? query.OrderByDescending(v => v.RegistrationNumber) : query.OrderBy(v => v.RegistrationNumber),
					_ => query.OrderBy(v => v.Id) // fallback ako polje nije poznato
				};
			}
			else
			{
				query = query.OrderBy(v => v.Id); // default sortiranje
			}

			// Paginacija
			int skip = (vehicleSearch.Page - 1) * vehicleSearch.PageSize;
			query = query.Skip(skip).Take(vehicleSearch.PageSize);

			return await query.ToListAsync();
		}


		public async Task<IEnumerable<Vehicle>> GetAvailableVehiclesAsync(int companyId)
		{
			return await _context.Vehicles
				.Where(v => v.VehicleStatus==VehicleStatus.Free && v.CompanyId == companyId)
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

		public async Task<Vehicle?> GetByRegistrationNumberAsync(string registrationNumber)
		{
			return await _context.Vehicles
				.FirstOrDefaultAsync(v => v.RegistrationNumber == registrationNumber);
		}

		public Task UpdateAsync(Vehicle vehicle)
		{
			_context.Vehicles.Update(vehicle);
			return Task.CompletedTask;
		}
	}
}
