using NaviGoApi.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NaviGoApi.Domain.Interfaces
{
	public interface ICompanyRepository
	{
		Task<Company?> GetByIdAsync(int id);
		Task<IEnumerable<Company>> GetAllAsync();
		Task AddAsync(Company company);
		Task UpdateAsync(Company company);
		Task DeleteAsync(Company company);
		Task<Company?> GetByNameAsync(string name);
		Task<Company?> GetByPibAsync(string pib);

	}
}
