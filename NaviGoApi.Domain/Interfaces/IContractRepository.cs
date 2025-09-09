using NaviGoApi.Common.DTOs;
using NaviGoApi.Domain.Entities;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace NaviGoApi.Domain.Interfaces
{
	public interface IContractRepository
	{
		Task<Contract?> GetByIdAsync(int id);
		Task<IEnumerable<Contract>> GetAllAsync();
		Task<IEnumerable<Contract>> GetAllAsync(ContractSearchDto contractSearch);

		Task AddAsync(Contract contract);
		Task UpdateAsync(Contract contract);
		Task DeleteAsync(Contract contract);

		// Primer specifičnih metoda:
		Task<IEnumerable<Contract>> GetByClientIdAsync(int clientId);
		Task<IEnumerable<Contract>> GetByForwarderIdAsync(int forwarderId);
		Task<bool> DuplicateContract(string contractNumber);
	}
}
