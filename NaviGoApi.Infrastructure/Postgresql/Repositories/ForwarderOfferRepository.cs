using Microsoft.EntityFrameworkCore;
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
			await _context.SaveChangesAsync();
		}

		public void Delete(ForwarderOffer offer)
		{
			_context.ForwarderOffers.Remove(offer);
			_context.SaveChanges();
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

		public void Update(ForwarderOffer offer)
		{
			_context.ForwarderOffers.Update(offer);
			_context.SaveChanges();
		}
	}
}
