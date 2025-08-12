using Microsoft.EntityFrameworkCore;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using NaviGoApi.Infrastructure.Postgresql.Persistence;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace NaviGoApi.Infrastructure.Postgresql.Repositories
{
	public class ContractRepository : IContractRepository
	{
		private readonly ApplicationDbContext _context;

		public ContractRepository(ApplicationDbContext context)
		{
			_context = context;
		}

		public async Task AddAsync(Contract contract)
		{
			await _context.Contracts.AddAsync(contract);
		}

		public Task DeleteAsync(Contract contract)
		{
			_context.Contracts.Remove(contract);
			return Task.CompletedTask;
		}

		public async Task<bool> ExistsAsync(Expression<Func<Contract, bool>> predicate)
		{
			return await _context.Contracts.AnyAsync(predicate);
		}

		public async Task<IEnumerable<Contract>> GetAllAsync()
		{
			return await _context.Contracts
				.Include(c => c.Client)
				.Include(c => c.Payment)
				.Include(c => c.Forwarder)
				.Include(c => c.Route)
					.ThenInclude(r => r.StartLocation)
				.Include(c => c.Route)
					.ThenInclude(r => r.EndLocation)
				.ToListAsync();
		}

		public async Task<IEnumerable<Contract>> GetByClientIdAsync(int clientId)
		{
			return await _context.Contracts
				.Where(c => c.ClientId == clientId)
				.Include(c=>c.Payment)
				.Include(c => c.Client)
				.Include(c => c.Forwarder)
				.Include(c => c.Route)
					.ThenInclude(r => r.StartLocation)
				.Include(c => c.Route)
					.ThenInclude(r => r.EndLocation)
				.ToListAsync();
		}

		public async Task<IEnumerable<Contract>> GetByForwarderIdAsync(int forwarderId)
		{
			return await _context.Contracts
				.Where(c => c.ForwarderId == forwarderId)
				.Include(c => c.Client)
				.Include(c => c.Forwarder)
				.Include(c => c.Route)
					.ThenInclude(r => r.StartLocation)
				.Include(c => c.Route)
					.ThenInclude(r => r.EndLocation)
				.AsNoTracking()
				.ToListAsync();
		}

		public async Task<Contract?> GetByIdAsync(int id)
		{
			return await _context.Contracts
				.Include(c => c.Client)
				.Include(c => c.Forwarder)
				.Include(c => c.Route)
					.ThenInclude(r => r.StartLocation)
				.Include(c => c.Route)
					.ThenInclude(r => r.EndLocation)
				.AsNoTracking()
				.FirstOrDefaultAsync(c => c.Id == id);
		}

		public Task UpdateAsync(Contract contract)
		{
			_context.Contracts.Update(contract);
			return Task.CompletedTask;
		}
	}
}
