using Microsoft.EntityFrameworkCore;
using NaviGoApi.Common.DTOs;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using NaviGoApi.Infrastructure.Postgresql.Persistence;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NaviGoApi.Infrastructure.Postgresql.Repositories
{
	public class VehicleMaintenanceRepository : IVehicleMaintenanceRepository
	{
		private readonly ApplicationDbContext _context;

		public VehicleMaintenanceRepository(ApplicationDbContext context)
		{
			_context = context;
		}

		public async Task AddAsync(VehicleMaintenance maintenance)
		{
			await _context.VehicleMaintenances.AddAsync(maintenance);
		}

		public async Task DeleteAsync(int id)
		{
			var entity = await _context.VehicleMaintenances.FindAsync(id);
			if (entity != null)
			{
				_context.VehicleMaintenances.Remove(entity);
			}
		}

		public async Task<IEnumerable<VehicleMaintenance>> GetAllAsync()
		{
			return await _context.VehicleMaintenances
				.Include(vm => vm.Vehicle)          
				.Include(vm => vm.ReportedByUser)     
				.ToListAsync();
		}

		public async Task<IEnumerable<VehicleMaintenance>> GetAllAsync(VehicleMaintenanceSearchDto search)
		{
			var query = _context.VehicleMaintenances
				.Include(vm => vm.Vehicle)
				.Include(vm => vm.ReportedByUser)
				.AsQueryable();
			query = (search.SortBy?.ToLower()) switch
			{
				"reportedat" => search.SortDirection.ToLower() == "desc"
					? query.OrderByDescending(vm => vm.ReportedAt)
					: query.OrderBy(vm => vm.ReportedAt),
				"resolvedat" => search.SortDirection.ToLower() == "desc"
					? query.OrderByDescending(vm => vm.ResolvedAt)
					: query.OrderBy(vm => vm.ResolvedAt),
				"severity" => search.SortDirection.ToLower() == "desc"
					? query.OrderByDescending(vm => vm.Severity)
					: query.OrderBy(vm => vm.Severity),
				_ => search.SortDirection.ToLower() == "desc"
					? query.OrderByDescending(vm => vm.Id)
					: query.OrderBy(vm => vm.Id)
			};
			int skip = (search.Page - 1) * search.PageSize;
			query = query.Skip(skip).Take(search.PageSize);

			return await query.ToListAsync();
		}


		public async Task<VehicleMaintenance?> GetByIdAsync(int id)
		{
			return await _context.VehicleMaintenances
				.Include(vm => vm.Vehicle)            
				.Include(vm => vm.ReportedByUser)     
				.FirstOrDefaultAsync(vm => vm.Id == id);
		}

		public async Task UpdateAsync(VehicleMaintenance maintenance)
		{
			_context.VehicleMaintenances.Update(maintenance);
		}
	}
}
