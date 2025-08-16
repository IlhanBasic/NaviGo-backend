using NaviGoApi.Common.DTOs;
using NaviGoApi.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NaviGoApi.Domain.Interfaces
{
	public interface IDriverRepository
	{
		Task<Driver?> GetByIdAsync(int id);
		Task<IEnumerable<Driver>> GetAllAsync();
		Task<IEnumerable<Driver>> GetAllAsync(DriverSearchDto driverSearch);

		Task AddAsync(Driver driver);
		Task UpdateAsync(Driver driver);
		Task DeleteAsync(Driver driver);

		// Primer dodatnih metoda
		Task<IEnumerable<Driver>> GetByCompanyIdAsync(int companyId);
		Task<IEnumerable<Driver>> GetAvailableDriversAsync();
	}
}
