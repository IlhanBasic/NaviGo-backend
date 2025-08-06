using NaviGoApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnjizaraApi.Domain.Interfaces
{
	public interface IUserRepository
	{
		Task<IEnumerable<User>> GetAllAsync();
		Task<User?> GetByIdAsync(int id);
		Task AddAsync(User user);
		void Update(User user);
		void Remove(User user);
		Task<User?> GetByEmailAsync(string email);
		Task<RefreshToken?> GetRefreshTokenAsync(string token);
		Task AddRefreshTokenAsync(RefreshToken token);
		Task UpdateRefreshTokenAsync(RefreshToken token);
		Task<User> FindByRefreshTokenAsync(string token);
		IQueryable<User> Query();
	}
}
