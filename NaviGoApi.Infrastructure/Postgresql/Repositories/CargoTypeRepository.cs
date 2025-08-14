using Microsoft.EntityFrameworkCore;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using NaviGoApi.Infrastructure.Postgresql.Persistence;
using System.Linq.Expressions;

namespace NaviGoApi.Infrastructure.Postgresql.Repositories
{
	public class CargoTypeRepository : ICargoTypeRepository
	{
		private readonly ApplicationDbContext _context;

		public CargoTypeRepository(ApplicationDbContext context)
		{
			_context = context;
		}

		public async Task AddAsync(CargoType cargoType)
		{
			await _context.CargoTypes.AddAsync(cargoType);
		}

		public Task DeleteAsync(CargoType cargoType)
		{
			_context.CargoTypes.Remove(cargoType);
			return Task.CompletedTask;
		}

		public async Task<bool> ExistsAsync(Expression<Func<CargoType, bool>> predicate)
		{
			return await _context.CargoTypes.AnyAsync(predicate);
		}

		public async Task<IEnumerable<CargoType>> GetAllAsync()
		{
			return await _context.CargoTypes.ToListAsync();
		}

		public Task<CargoType> GetByTypeName(string name)
		{
			throw new NotImplementedException();
		}

		public async Task<CargoType?> GetByIdAsync(int id)
		{
			return await _context.CargoTypes.FindAsync(id);
		}

		public Task UpdateAsync(CargoType cargoType)
		{
			_context.CargoTypes.Update(cargoType);
			return Task.CompletedTask;
		}
	}

}
