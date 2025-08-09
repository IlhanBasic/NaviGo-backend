using NaviGoApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Domain.Interfaces
{
	public interface IPickupChangeRepository
	{
		Task<IEnumerable<PickupChange>> GetAllAsync();
		Task<PickupChange?> GetByIdAsync(int id);
		Task AddAsync(PickupChange change);
		Task UpdateAsync(PickupChange change);
		Task DeleteAsync(int id);
	}
}
