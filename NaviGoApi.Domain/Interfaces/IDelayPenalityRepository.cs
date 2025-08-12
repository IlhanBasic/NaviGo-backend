using NaviGoApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Domain.Interfaces
{
	public interface IDelayPenaltyRepository
	{
		Task<IEnumerable<DelayPenalty>> GetAllAsync();
		Task<DelayPenalty?> GetByIdAsync(int id);
		Task AddAsync(DelayPenalty penalty);
		Task UpdateAsync(DelayPenalty penalty);
		Task DeleteAsync(int id);
		Task<IEnumerable<DelayPenalty>> GetByContractIdAsync(int contractId);
		Task<DelayPenalty?> GetByShipmentIdAsync(int  shipmentId);

	}
}
