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

		public Task DeleteRange(IEnumerable<Shipment> shipments)
		{
			_context.Shipments.RemoveRange(shipments);
			return Task.CompletedTask;
		}

		public async Task<IEnumerable<Shipment>> GetAllAsync()
		{
			return await _context.Shipments
				.Include(s => s.Contract)
				.Include(s => s.Vehicle)
				.Include(s => s.Driver)
				.Include(s => s.CargoType)
				.ToListAsync();
		}

		public async Task<IEnumerable<Shipment>> GetAllAsync(ShipmentSearchDto shipmentSearch)
		{
			var query = _context.Shipments
				.Include(s => s.Contract)
				.Include(s => s.Vehicle)
				.Include(s => s.Driver)
				.Include(s => s.CargoType)
				.AsQueryable();

			query = (shipmentSearch.SortBy?.ToLower(), shipmentSearch.SortDirection.ToLower()) switch
			{
				("status", "asc") => query.OrderBy(s => s.Status),
				("status", "desc") => query.OrderByDescending(s => s.Status),
				("scheduleddeparture", "asc") => query.OrderBy(s => s.ScheduledDeparture),
				("scheduleddeparture", "desc") => query.OrderByDescending(s => s.ScheduledDeparture),
				("scheduledarrival", "asc") => query.OrderBy(s => s.ScheduledArrival),
				("scheduledarrival", "desc") => query.OrderByDescending(s => s.ScheduledArrival),
				("priority", "asc") => query.OrderBy(s => s.Priority),
				("priority", "desc") => query.OrderByDescending(s => s.Priority),
				_ => query.OrderBy(s => s.Id) 
			};
			int skip = (shipmentSearch.Page - 1) * shipmentSearch.PageSize;
			query = query.Skip(skip).Take(shipmentSearch.PageSize);

			return await query.ToListAsync();
		}


		public async Task<IEnumerable<Shipment>> GetByContractIdAsync(int contractId)
		{
			Console.WriteLine($"Pretraga pošiljaka za ContractId: {contractId}");
			var shipments = await _context.Shipments
	.Where(s => s.ContractId == contractId)
	.Include(s => s.CargoType)
	.Select(s => new Shipment
	{
		Id = s.Id,
		ContractId = s.ContractId,
		DriverId = s.DriverId,
		VehicleId = s.VehicleId,
		CargoTypeId = s.CargoTypeId,
		WeightKg = s.WeightKg,
		Priority = s.Priority,
		Status = s.Status,
		ScheduledDeparture = s.ScheduledDeparture,
		ScheduledArrival = s.ScheduledArrival,
		Contract = s.Contract,
		CargoType = s.CargoType,
		Driver = s.Driver,   // EF Core LEFT JOIN automatski
		Vehicle = s.Vehicle
	})
	.ToListAsync();

			Console.WriteLine($"Pronađeno {shipments.Count} pošiljaka.");
			return shipments;

		}

		public async Task<Shipment?> GetByIdAsync(int id)
		{
			return await _context.Shipments
				.Include(s => s.Contract)
					.ThenInclude(c => c.Forwarder)
				.Include(s => s.Contract)
					.ThenInclude(c => c.Route)
				.Include(s => s.Contract)
					.ThenInclude(c => c.Client)
				.Include(s => s.Contract)
					.ThenInclude(c => c.Payment)
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
