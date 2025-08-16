using MongoDB.Bson;
using MongoDB.Driver;
using NaviGoApi.Common.DTOs;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NaviGoApi.Infrastructure.MongoDB.Repositories
{
	public class UserMongoRepository : IUserRepository
	{
		private readonly IMongoCollection<User> _usersCollection;
		private readonly IMongoCollection<RefreshToken> _refreshTokensCollection;

		public UserMongoRepository(IMongoDatabase database)
		{
			_usersCollection = database.GetCollection<User>("Users");
			_refreshTokensCollection = database.GetCollection<RefreshToken>("RefreshTokens");
		}

		public async Task AddAsync(User user)
		{
			user.Id = await GetNextIdAsync("Users");
			await _usersCollection.InsertOneAsync(user);
		}

		public async Task AddRefreshTokenAsync(RefreshToken token)
		{
			token.Id = await GetNextIdAsync("RefreshTokens");
			await _refreshTokensCollection.InsertOneAsync(token);

			var filter = Builders<User>.Filter.Eq(u => u.Id, token.UserId);
			var update = Builders<User>.Update.Push(u => u.RefreshTokens, token);
			await _usersCollection.UpdateOneAsync(filter, update);
		}

		public async Task RevokeRefreshTokenAsync(string token, string ipAddress)
		{
			var filter = Builders<RefreshToken>.Filter.Eq(t => t.Token, token);
			var update = Builders<RefreshToken>.Update
				.Set(t => t.Revoked, DateTime.UtcNow)
				.Set(t => t.RevokedByIp, ipAddress);

			await _refreshTokensCollection.UpdateOneAsync(filter, update);
		}

		private async Task<int> GetNextIdAsync(string collectionName)
		{
			var counters = _usersCollection.Database.GetCollection<BsonDocument>("Counters");

			var filter = Builders<BsonDocument>.Filter.Eq("_id", collectionName);
			var update = Builders<BsonDocument>.Update.Inc("SequenceValue", 1);

			var options = new FindOneAndUpdateOptions<BsonDocument>
			{
				IsUpsert = true,
				ReturnDocument = ReturnDocument.After
			};

			var result = await counters.FindOneAndUpdateAsync(filter, update, options);
			return result["SequenceValue"].AsInt32;
		}

		public async Task<User?> FindByRefreshTokenAsync(string token)
		{
			var user = await _usersCollection.Find(u => u.RefreshTokens.Any(t => t.Token == token)).FirstOrDefaultAsync();
			if (user != null)
			{
				await LoadCompanyAsync(user);
			}
			return user;
		}

		public async Task<IEnumerable<User>> GetAllAsync()
		{
			var users = await _usersCollection.Find(_ => true).ToListAsync();
			foreach (var user in users)
				await LoadCompanyAsync(user);

			return users;
		}

		public async Task<User?> GetByEmailAsync(string email)
		{
			var user = await _usersCollection.Find(u => u.Email == email).FirstOrDefaultAsync();
			await LoadCompanyAsync(user);
			return user;
		}

		public async Task<User?> GetByPasswordResetTokenAsync(string token)
		{
			return await _usersCollection.Find(u => u.PasswordResetToken == token).FirstOrDefaultAsync();
		}

		public async Task<User?> GetByIdAsync(int id)
		{
			var user = await _usersCollection.Find(u => u.Id == id).FirstOrDefaultAsync();
			await LoadCompanyAsync(user);
			return user;
		}

		public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
		{
			return await _refreshTokensCollection.Find(rt => rt.Token == token).FirstOrDefaultAsync();
		}

		public async Task<User?> GetByEmailVerificationTokenAsync(string token)
		{
			var user = await _usersCollection.Find(u => u.EmailVerificationToken == token).FirstOrDefaultAsync();
			await LoadCompanyAsync(user);
			return user;
		}

		public IQueryable<User> Query()
		{
			return _usersCollection.AsQueryable();
		}

		public async Task DeleteAsync(User user)
		{
			await _usersCollection.DeleteOneAsync(u => u.Id == user.Id);
		}

		public async Task UpdateAsync(User user)
		{
			await _usersCollection.ReplaceOneAsync(u => u.Id == user.Id, user);
		}

		public async Task UpdateRefreshTokenAsync(RefreshToken token)
		{
			await _refreshTokensCollection.ReplaceOneAsync(rt => rt.Id == token.Id, token);
		}
		private async Task LoadCompanyAsync(User? user)
		{
			if (user == null) return;
			if (user.CompanyId != 0)
			{
				var companiesCollection = _usersCollection.Database.GetCollection<Company>("Companies");
				user.Company = await companiesCollection.Find(c => c.Id == user.CompanyId).FirstOrDefaultAsync();
			}
		}

		public async Task<IEnumerable<User>> GetAllAsync(UserSearchDto userSearch)
		{
			var filterBuilder = Builders<User>.Filter;
			var filter = filterBuilder.Empty;

			if (!string.IsNullOrWhiteSpace(userSearch.Email))
				filter &= filterBuilder.Regex(u => u.Email, new BsonRegularExpression(userSearch.Email, "i"));

			if (!string.IsNullOrWhiteSpace(userSearch.FirstName))
				filter &= filterBuilder.Regex(u => u.FirstName, new BsonRegularExpression(userSearch.FirstName, "i"));

			if (!string.IsNullOrWhiteSpace(userSearch.LastName))
				filter &= filterBuilder.Regex(u => u.LastName, new BsonRegularExpression(userSearch.LastName, "i"));
			var sortDefinition = userSearch.SortBy?.ToLower() switch
			{
				"email" => userSearch.SortDirection.ToLower() == "desc"
					? Builders<User>.Sort.Descending(u => u.Email)
					: Builders<User>.Sort.Ascending(u => u.Email),
				"firstname" => userSearch.SortDirection.ToLower() == "desc"
					? Builders<User>.Sort.Descending(u => u.FirstName)
					: Builders<User>.Sort.Ascending(u => u.FirstName),
				"lastname" => userSearch.SortDirection.ToLower() == "desc"
					? Builders<User>.Sort.Descending(u => u.LastName)
					: Builders<User>.Sort.Ascending(u => u.LastName),
				_ => userSearch.SortDirection.ToLower() == "desc"
					? Builders<User>.Sort.Descending(u => u.Id)
					: Builders<User>.Sort.Ascending(u => u.Id)
			};

			var skip = (userSearch.Page - 1) * userSearch.PageSize;

			var users = await _usersCollection
				.Find(filter)
				.Sort(sortDefinition)
				.Skip(skip)
				.Limit(userSearch.PageSize)
				.ToListAsync();

			foreach (var user in users)
				await LoadCompanyAsync(user);

			return users;
		}

	}
}
