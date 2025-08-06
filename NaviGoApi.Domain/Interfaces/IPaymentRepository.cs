using NaviGoApi.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NaviGoApi.Domain.Interfaces
{
	public interface IPaymentRepository
	{
		Task<Payment?> GetByIdAsync(int id);
		Task<IEnumerable<Payment>> GetAllAsync();
		Task AddAsync(Payment payment);
		void Update(Payment payment);
		void Delete(Payment payment);

		// Specifične metode
		Task<IEnumerable<Payment>> GetByContractIdAsync(int contractId);
		Task<IEnumerable<Payment>> GetByClientIdAsync(int clientId);
		Task<IEnumerable<Payment>> GetPendingPaymentsAsync();
	}
}
