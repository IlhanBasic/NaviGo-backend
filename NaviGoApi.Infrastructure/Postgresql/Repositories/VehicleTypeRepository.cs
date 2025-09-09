using Microsoft.EntityFrameworkCore;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using NaviGoApi.Infrastructure.Postgresql.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace NaviGoApi.Infrastructure.Postgresql.Repositories
{
	public class VehicleTypeRepository : IVehicleTypeRepository
	{
		private readonly ApplicationDbContext _context;

		public VehicleTypeRepository(ApplicationDbContext context)
		{
			_context = context;
		}

		public async Task AddAsync(VehicleType vehicleType)
		{
			await _context.VehicleTypes.AddAsync(vehicleType);
		}

		public async Task DeleteAsync(int id)
		{
			var entity = await _context.VehicleTypes.FindAsync(id);
			if (entity != null)
			{
				_context.VehicleTypes.Remove(entity);
			}
		}

		public async Task<IEnumerable<VehicleType>> GetAllAsync()
		{
			return await _context.VehicleTypes.ToListAsync();
		}

		public async Task<VehicleType?> GetByIdAsync(int id)
		{
			return await _context.VehicleTypes.FindAsync(id);
		}

		public Task UpdateAsync(VehicleType vehicleType)
		{
			_context.VehicleTypes.Update(vehicleType);
			return Task.CompletedTask;
		}

		public async Task<VehicleType?> GetByTypeName(string typeName)
		{
			return await _context.VehicleTypes.FirstOrDefaultAsync(x=>x.TypeName== typeName);
		}
	}
}
