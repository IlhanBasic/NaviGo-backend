using Microsoft.EntityFrameworkCore;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using NaviGoApi.Infrastructure.Postgresql.Persistence; 

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

		public void Delete(CargoType cargoType)
		{
			_context.CargoTypes.Remove(cargoType);
		}

		public async Task<IEnumerable<CargoType>> GetAllAsync()
		{
			return await _context.CargoTypes.ToListAsync();
		}

		public async Task<CargoType?> GetByIdAsync(int id)
		{
			return await _context.CargoTypes.FindAsync(id);
		}

		public void Update(CargoType cargoType)
		{
			_context.CargoTypes.Update(cargoType);
		}
	}
}
