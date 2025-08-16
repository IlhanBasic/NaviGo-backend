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

		public Task<IEnumerable<VehicleMaintenance>> GetAllAsync(VehicleMaintenanceSearchDto vehicleMaintenanceSearch)
		{
			throw new NotImplementedException();
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
