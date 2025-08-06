using NaviGoApi.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NaviGoApi.Domain.Interfaces
{
	public interface IDriverRepository
	{
		Task<Driver?> GetByIdAsync(int id);
		Task<IEnumerable<Driver>> GetAllAsync();
		Task AddAsync(Driver driver);
		void Update(Driver driver);
		void Delete(Driver driver);

		// Primer dodatnih metoda
		Task<IEnumerable<Driver>> GetByCompanyIdAsync(int companyId);
		Task<IEnumerable<Driver>> GetAvailableDriversAsync();
	}
}
