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

		public void Delete(Shipment shipment)
		{
			_context.Shipments.Remove(shipment);
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
				.Include(s => s.Vehicle)
				.Include(s => s.Driver)
				.Include(s => s.CargoType)
				.AsNoTracking()
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
				.AsNoTracking()
				.ToListAsync();
		}

		public void Update(Shipment shipment)
		{
			_context.Shipments.Update(shipment);
		}
	}
}
