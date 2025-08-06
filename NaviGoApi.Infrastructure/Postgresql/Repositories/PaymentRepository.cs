using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Infrastructure.Postgresql.Repositories
{
	public class PaymentRepository : IPaymentRepository
	{
		public Task AddAsync(Payment payment)
		{
			throw new NotImplementedException();
		}

		public void Delete(Payment payment)
		{
			throw new NotImplementedException();
		}

		public Task<IEnumerable<Payment>> GetAllAsync()
		{
			throw new NotImplementedException();
		}

		public Task<IEnumerable<Payment>> GetByClientIdAsync(int clientId)
		{
			throw new NotImplementedException();
		}

		public Task<IEnumerable<Payment>> GetByContractIdAsync(int contractId)
		{
			throw new NotImplementedException();
		}

		public Task<Payment?> GetByIdAsync(int id)
		{
			throw new NotImplementedException();
		}

		public Task<IEnumerable<Payment>> GetPendingPaymentsAsync()
		{
			throw new NotImplementedException();
		}

		public void Update(Payment payment)
		{
			throw new NotImplementedException();
		}
	}
}
