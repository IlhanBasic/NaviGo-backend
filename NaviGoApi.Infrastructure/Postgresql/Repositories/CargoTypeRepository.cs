using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Infrastructure.Postgresql.Repositories
{
	public class CargoTypeRepository : ICargoTypeRepository
	{
		public Task AddAsync(CargoType cargoType)
		{
			throw new NotImplementedException();
		}

		public void Delete(CargoType cargoType)
		{
			throw new NotImplementedException();
		}

		public Task<IEnumerable<CargoType>> GetAllAsync()
		{
			throw new NotImplementedException();
		}

		public Task<CargoType?> GetByIdAsync(int id)
		{
			throw new NotImplementedException();
		}

		public void Update(CargoType cargoType)
		{
			throw new NotImplementedException();
		}
	}
}
