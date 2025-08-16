using Microsoft.EntityFrameworkCore;
using NaviGoApi.Common.DTOs;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using NaviGoApi.Infrastructure.Postgresql.Persistence;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NaviGoApi.Infrastructure.Postgresql.Repositories
{
	public class ShipmentDocumentRepository : IShipmentDocumentRepository
	{
		private readonly ApplicationDbContext _context;
		public ShipmentDocumentRepository(ApplicationDbContext context)
		{
			_context = context;
		}

		public async Task AddAsync(ShipmentDocument document)
		{
			await _context.ShipmentDocuments.AddAsync(document);
		}

		public async Task DeleteAsync(int id)
		{
			var entity = await _context.ShipmentDocuments.FindAsync(id);
			if (entity != null)
			{
				_context.ShipmentDocuments.Remove(entity);
			}
		}

		public async Task<IEnumerable<ShipmentDocument>> GetAllAsync()
		{
			return await _context.ShipmentDocuments.AsNoTracking().ToListAsync();
		}

		public async Task<IEnumerable<ShipmentDocument>> GetAllAsync(ShipmentDocumentSearchDto shipmentDocumentSearch)
		{
			string sortBy = string.IsNullOrEmpty(shipmentDocumentSearch.SortBy) ? "Id" : shipmentDocumentSearch.SortBy;
			bool descending = shipmentDocumentSearch.SortDirection?.ToLower() == "desc";
			int skip = (shipmentDocumentSearch.Page - 1) * shipmentDocumentSearch.PageSize;
			int take = shipmentDocumentSearch.PageSize;

			var query = _context.ShipmentDocuments.AsQueryable();

			query = descending
				? query.OrderByDescending(e => EF.Property<object>(e, sortBy))
				: query.OrderBy(e => EF.Property<object>(e, sortBy));
			query = query.Skip(skip).Take(take);

			return await query.AsNoTracking().ToListAsync();
		}


		public async Task<ShipmentDocument?> GetByIdAsync(int id)
		{
			return await _context.ShipmentDocuments.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
		}

		public async Task UpdateAsync(ShipmentDocument document)
		{
			_context.ShipmentDocuments.Update(document);
		}
	}
}
