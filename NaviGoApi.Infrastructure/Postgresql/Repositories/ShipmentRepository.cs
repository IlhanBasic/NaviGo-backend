using Microsoft.EntityFrameworkCore;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using NaviGoApi.Infrastructure.Postgresql.Persistence;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NaviGoApi.Infrastructure.Postgresql.Repositories
{
	public class ShipmentRepository : IShipmentRepository
	{
		private readonly ApplicationDbContext _context;

		public ShipmentRepository(ApplicationDbContext context)
		{
			_context = context;
		}

		public async Task AddAsync(Shipment shipment)
		{
			await _context.Shipments.AddAsync(shipment);
		}

		public Task DeleteAsync(Shipment shipment)
		{
			_context.Shipments.Remove(shipment);
			return Task.CompletedTask;
		}

		public async Task<IEnumerable<Shipment>> GetAllAsync()
		{
			return await _context.Shipments
				.Include(s => s.Contract)
				.Include(s => s.Vehicle)
				.Include(s => s.Driver)
				.Include(s => s.CargoType)
				.AsNoTracking()
				.ToListAsync();
		}

		public async Task<IEnumerable<Shipment>> GetByContractIdAsync(int contractId)
		{
			return await _context.Shipments
				.Where(s => s.ContractId == contractId)
				.Include(s => s.Contract)
				.Include(s => s.Vehicle)
				.Include(s => s.Driver)
				.Include(s => s.CargoType)
				.AsNoTracking()
				.ToListAsync();
		}

		public async Task<Shipment?> GetByIdAsync(int id)
		{
			return await _context.Shipments
				.Include(s => s.Contract)
					.ThenInclude(c => c.Route)
						.ThenInclude(r => r.RoutePrices) 
				.Include(s => s.Vehicle)
				.Include(s => s.Driver)
				.Include(s => s.CargoType)
				.FirstOrDefaultAsync(s => s.Id == id);
		}



		public async Task<IEnumerable<Shipment>> GetByStatusAsync(ShipmentStatus status)
		{
			return await _context.Shipments
				.Where(s => s.Status == status)
				.Include(s => s.Contract)
				.Include(s => s.Vehicle)
				.Include(s => s.Driver)
				.Include(s => s.CargoType)
				.ToListAsync();
		}

		public Task UpdateAsync(Shipment shipment)
		{
			_context.Shipments.Update(shipment);
			return Task.CompletedTask;
		}
	}
}
