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
	public class ForwarderOfferRepository : IForwarderOfferRepository
	{
		private readonly ApplicationDbContext _context;

		public ForwarderOfferRepository(ApplicationDbContext context)
		{
			_context = context;
		}

		public async Task AddAsync(ForwarderOffer offer)
		{
			await _context.ForwarderOffers.AddAsync(offer);
		}

		public Task DeleteAsync(ForwarderOffer offer)
		{
			_context.ForwarderOffers.Remove(offer);
			return Task.CompletedTask;
		}

		public async Task<IEnumerable<ForwarderOffer>> GetActiveOffersAsync()
		{
			var now = DateTime.UtcNow;
			return await _context.ForwarderOffers
				.Where(o => o.ForwarderOfferStatus == ForwarderOfferStatus.Pending
						 && o.ExpiresAt > now)
				.Include(o => o.Route)
				.Include(o => o.Forwarder)
				.ToListAsync();
		}

		public async Task<IEnumerable<ForwarderOffer>> GetAllAsync()
		{
			return await _context.ForwarderOffers
				.Include(o => o.Route)
				.Include(o => o.Forwarder)
				.ToListAsync();
		}

		public async Task<IEnumerable<ForwarderOffer>> GetAllAsync(ForwarderOfferSearchDto forwarderOfferSearch)
		{
			var query = _context.ForwarderOffers
				.Include(o => o.Route)
				.Include(o => o.Forwarder)
				.AsQueryable();
			if (!string.IsNullOrWhiteSpace(forwarderOfferSearch.SortBy))
			{
				query = (forwarderOfferSearch.SortBy.ToLower(), forwarderOfferSearch.SortDirection.ToLower()) switch
				{
					("id", "asc") => query.OrderBy(o => o.Id),
					("id", "desc") => query.OrderByDescending(o => o.Id),
					("createdat", "asc") => query.OrderBy(o => o.CreatedAt),
					("createdat", "desc") => query.OrderByDescending(o => o.CreatedAt),
					("expiresat", "asc") => query.OrderBy(o => o.ExpiresAt),
					("expiresat", "desc") => query.OrderByDescending(o => o.ExpiresAt),
					("commissionrate", "asc") => query.OrderBy(o => o.CommissionRate),
					("commissionrate", "desc") => query.OrderByDescending(o => o.CommissionRate),
					_ => query.OrderBy(o => o.Id)
				};
			}
			var skip = (forwarderOfferSearch.Page - 1) * forwarderOfferSearch.PageSize;
			query = query.Skip(skip).Take(forwarderOfferSearch.PageSize);

			return await query.ToListAsync();
		}


		public async Task<IEnumerable<ForwarderOffer>> GetByForwarderIdAsync(int forwarderId)
		{
			return await _context.ForwarderOffers
				.Where(o => o.ForwarderId == forwarderId)
				.Include(o => o.Route)
				.Include(o => o.Forwarder)
				.ToListAsync();
		}

		public async Task<ForwarderOffer?> GetByIdAsync(int id)
		{
			return await _context.ForwarderOffers
				.Include(o => o.Route)
				.Include(o => o.Forwarder)
				.FirstOrDefaultAsync(o => o.Id == id);
		}

		public async Task<IEnumerable<ForwarderOffer>> GetByRouteIdAsync(int routeId)
		{
			return await _context.ForwarderOffers
				.Where(o => o.RouteId == routeId)
				.Include(o => o.Route)
				.Include(o => o.Forwarder)
				.ToListAsync();
		}

		public Task UpdateAsync(ForwarderOffer offer)
		{
			_context.ForwarderOffers.Update(offer);
			return Task.CompletedTask;
		}
	}
}
