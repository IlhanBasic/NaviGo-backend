using KnjizaraApi.Domain.Interfaces;
using NaviGoApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Infrastructure.Postgresql.Repositories
{
	public class UserRepository : IUserRepository
	{
		public Task AddAsync(User user)
		{
			throw new NotImplementedException();
		}

		public Task AddRefreshTokenAsync(RefreshToken token)
		{
			throw new NotImplementedException();
		}

		public Task<User> FindByRefreshTokenAsync(string token)
		{
			throw new NotImplementedException();
		}

		public Task<IEnumerable<User>> GetAllAsync()
		{
			throw new NotImplementedException();
		}

		public Task<User?> GetByEmailAsync(string email)
		{
			throw new NotImplementedException();
		}

		public Task<User?> GetByIdAsync(int id)
		{
			throw new NotImplementedException();
		}

		public Task<RefreshToken?> GetRefreshTokenAsync(string token)
		{
			throw new NotImplementedException();
		}

		public IQueryable<User> Query()
		{
			throw new NotImplementedException();
		}

		public void Remove(User user)
		{
			throw new NotImplementedException();
		}

		public void Update(User user)
		{
			throw new NotImplementedException();
		}

		public Task UpdateRefreshTokenAsync(RefreshToken token)
		{
			throw new NotImplementedException();
		}
	}
}
