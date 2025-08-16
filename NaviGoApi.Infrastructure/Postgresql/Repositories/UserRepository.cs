
using NaviGoApi.Domain.Entities;
using NaviGoApi.Infrastructure.Postgresql.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NaviGoApi.Domain.Interfaces;
using NaviGoApi.Common.DTOs;
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
			return await _context.RefreshTokens
	.FirstOrDefaultAsync(rt => rt.Token == token && rt.Revoked == null && rt.Expires > DateTime.UtcNow);

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

		public async Task RevokeRefreshTokenAsync(string token, string ipAddress)
		{
			var refreshToken = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == token);
			if (refreshToken != null && refreshToken.IsActive)
			{
				refreshToken.Revoked = DateTime.UtcNow;
				refreshToken.RevokedByIp = ipAddress;

				await _context.SaveChangesAsync(); 
			}
		}

		public async Task<IEnumerable<User>> GetAllAsync(UserSearchDto userSearch)
		{
			var query = _context.Users.AsQueryable();

			if (!string.IsNullOrEmpty(userSearch.Email))
				query = query.Where(u => u.Email.Contains(userSearch.Email));
			if (!string.IsNullOrEmpty(userSearch.FirstName))
				query = query.Where(u => u.FirstName.Contains(userSearch.FirstName));
			if (!string.IsNullOrEmpty(userSearch.LastName))
				query = query.Where(u => u.LastName.Contains(userSearch.LastName));
			query = (userSearch.SortBy?.ToLower(), userSearch.SortDirection?.ToLower()) switch
			{
				("email", "desc") => query.OrderByDescending(u => u.Email),
				("email", _) => query.OrderBy(u => u.Email),
				("firstname", "desc") => query.OrderByDescending(u => u.FirstName),
				("firstname", _) => query.OrderBy(u => u.FirstName),
				("lastname", "desc") => query.OrderByDescending(u => u.LastName),
				("lastname", _) => query.OrderBy(u => u.LastName),
				("id", "desc") => query.OrderByDescending(u => u.Id),
				_ => query.OrderBy(u => u.Id),
			};
			var skip = (userSearch.Page - 1) * userSearch.PageSize;
			query = query.Skip(skip).Take(userSearch.PageSize);

			return await query
				.Include(u => u.Company)
				.Include(u => u.RefreshTokens)
				.Include(u => u.VehicleMaintenancesReported)
				.Include(u => u.ShipmentDocumentsVerified)
				.Include(u => u.ShipmentStatusChanges)
				.Include(u => u.UserLocations)
				.ToListAsync();
		}

	}
}
