
using NaviGoApi.Domain.Entities;
using NaviGoApi.Infrastructure.Postgresql.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NaviGoApi.Domain.Interfaces;
namespace NaviGoApi.Infrastructure.Postgresql.Repositories
{
	public class UserRepository : IUserRepository
	{
		private readonly ApplicationDbContext _context;

		public UserRepository(ApplicationDbContext context)
		{
			_context = context;
		}
		public async Task AddAsync(User user)
		{
			await _context.Users.AddAsync(user);
		}

		public async Task AddRefreshTokenAsync(RefreshToken token)
		{
			await _context.RefreshTokens.AddAsync(token);
		}

		public async Task<User> FindByRefreshTokenAsync(string token)
		{
			return await _context.Users
				.Include(u => u.RefreshTokens)
				.FirstOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == token));
		}

		public async Task<IEnumerable<User>> GetAllAsync()
		{
			return await _context.Users.ToListAsync();
		}

		public async Task<User?> GetByEmailAsync(string email)
		{
			return await _context.Users
				.Include(u => u.Company)
				.FirstOrDefaultAsync(u => u.Email == email);
		}
		public async Task<User?> GetByPasswordResetTokenAsync(string token)
		{
			return await _context.Users
				.FirstOrDefaultAsync(u => u.PasswordResetToken == token);
		}
		public async Task<User?> GetByIdAsync(int id)
		{
			return await _context.Users
				.Include(u => u.Company)
				.FirstOrDefaultAsync(u => u.Id == id);
		}

		public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
		{
			return await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == token);
		}
		public async Task<User?> GetByEmailVerificationTokenAsync(string token)
		{
			return await _context.Users
				.FirstOrDefaultAsync(u => u.EmailVerificationToken == token);
		}
		public IQueryable<User> Query()
		{
			return _context.Users.AsQueryable();
		}

		public Task DeleteAsync(User user)
		{
			_context.Users.Remove(user);
			return Task.CompletedTask;
		}

		public Task UpdateAsync(User user)
		{
			_context.Users.Update(user);
			return Task.CompletedTask;
		}

		public async Task UpdateRefreshTokenAsync(RefreshToken token)
		{
			_context.RefreshTokens.Update(token);
		}
	}
}
