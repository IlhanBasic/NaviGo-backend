using NaviGoApi.Domain.Entities;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace NaviGoApi.Domain.Interfaces
{
	public interface ICargoTypeRepository
	{
		Task<IEnumerable<CargoType>> GetAllAsync();
		Task<CargoType?> GetByIdAsync(int id);
		Task AddAsync(CargoType cargoType);
		Task UpdateAsync(CargoType cargoType);
		Task DeleteAsync(CargoType cargoType);
		Task<bool> ExistsAsync(Expression<Func<CargoType, bool>> predicate);
		Task<CargoType?> GetByTypeName(string name);
	}
}
