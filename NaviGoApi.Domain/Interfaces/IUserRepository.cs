using NaviGoApi.Common.DTOs;
using NaviGoApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Domain.Interfaces
{
	public interface IUserRepository
	{
		Task<IEnumerable<User>> GetAllAsync();
		Task<IEnumerable<User>> GetAllAsync(UserSearchDto userSearch);
		Task<User?> GetByIdAsync(int id);
		Task<User?> GetByPasswordResetTokenAsync(string token);
		Task AddAsync(User user);
		Task UpdateAsync(User user);
		Task DeleteAsync(User user);
		Task<User?> GetByEmailAsync(string email);
		Task<RefreshToken?> GetRefreshTokenAsync(string token);
		Task AddRefreshTokenAsync(RefreshToken token);
		Task UpdateRefreshTokenAsync(RefreshToken token);
		Task<User> FindByRefreshTokenAsync(string token);
		Task<User?> GetByEmailVerificationTokenAsync(string token);
		public Task RevokeRefreshTokenAsync(string token, string ipAddress);
		IQueryable<User> Query();
	}
}
