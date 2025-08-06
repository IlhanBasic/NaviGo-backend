using NaviGoApi.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NaviGoApi.Domain.Interfaces
{
	public interface IVehicleRepository
	{
		Task<Vehicle?> GetByIdAsync(int id);
		Task<IEnumerable<Vehicle>> GetAllAsync();
		Task AddAsync(Vehicle vehicle);
		void Update(Vehicle vehicle);
		void Delete(Vehicle vehicle);

		// Eventualno dodatne metode specifične za Vehicle
		Task<IEnumerable<Vehicle>> GetByCompanyIdAsync(int companyId);
		Task<IEnumerable<Vehicle>> GetAvailableVehiclesAsync();
	}
}
