using NaviGoApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Domain.Interfaces
{
	public interface IVehicleTypeRepository
	{
		Task<IEnumerable<VehicleType>> GetAllAsync();
		Task<VehicleType?> GetByIdAsync(int id);
		Task AddAsync(VehicleType vehicleType);
		Task UpdateAsync(VehicleType vehicleType);
		Task DeleteAsync(int id);
		Task<bool> ExistsAsync(Expression<Func<VehicleType, bool>> predicate);
		Task<VehicleType?> GetByTypeName(string typeName);



	}
}
