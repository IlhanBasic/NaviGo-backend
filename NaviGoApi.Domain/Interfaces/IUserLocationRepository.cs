using NaviGoApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Domain.Interfaces
{
	public interface IUserLocationRepository
	{
		Task AddAsync(UserLocation location);
		Task<List<UserLocation>> GetRecentLocationsAsync(int userId, TimeSpan interval);
		Task SaveChangesAsync();
	}

}
