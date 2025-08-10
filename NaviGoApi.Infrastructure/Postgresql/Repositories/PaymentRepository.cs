using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using NaviGoApi.Infrastructure.Postgresql.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NaviGoApi.Infrastructure.Postgresql.Repositories
{
	public class PaymentRepository : IPaymentRepository
	{
		private readonly ApplicationDbContext _context;

		public PaymentRepository(ApplicationDbContext context)
		{
			_context = context;
		}

		public async Task AddAsync(Payment payment)
		{
			await _context.Payments.AddAsync(payment);
		}

		public Task DeleteAsync(Payment payment)
		{
			_context.Payments.Remove(payment);
			return Task.CompletedTask;
		}

		public async Task<IEnumerable<Payment>> GetAllAsync()
		{
			return await _context.Payments
				.Include(p => p.Client)
				.Include(p => p.Contract)
				.ToListAsync();
		}

		public async Task<IEnumerable<Payment>> GetByClientIdAsync(int clientId)
		{
			return await _context.Payments
				.Where(p => p.ClientId == clientId)
				.Include(p => p.Client)
				.Include(p => p.Contract)
				.ToListAsync();
		}

		public async Task<IEnumerable<Payment>> GetByContractIdAsync(int contractId)
		{
			return await _context.Payments
				.Where(p => p.ContractId == contractId)
				.Include(p => p.Client)
				.Include(p => p.Contract)
				.ToListAsync();
		}

		public async Task<Payment?> GetByIdAsync(int id)
		{
			return await _context.Payments
				.Include(p => p.Client)
				.Include(p => p.Contract)
				.FirstOrDefaultAsync(p => p.Id == id);
		}

		public async Task<IEnumerable<Payment>> GetPendingPaymentsAsync()
		{
			return await _context.Payments
				.Where(p => p.PaymentStatus == PaymentStatus.Pending)
				.Include(p => p.Client)
				.Include(p => p.Contract)
				.ToListAsync();
		}

		public Task UpdateAsync(Payment payment)
		{
			_context.Payments.Update(payment);
			return Task.CompletedTask;
		}
	}
}
