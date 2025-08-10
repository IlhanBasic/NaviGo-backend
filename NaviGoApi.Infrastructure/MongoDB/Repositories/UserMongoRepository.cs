using MongoDB.Bson;
using MongoDB.Driver;
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
			return await _usersCollection.Find(u => u.RefreshTokens.Any(t => t.Token == token)).FirstOrDefaultAsync();
		}

		public async Task<IEnumerable<User>> GetAllAsync()
		{
			return await _usersCollection.Find(_ => true).ToListAsync();
		}

		public async Task<User?> GetByEmailAsync(string email)
		{
			return await _usersCollection.Find(u => u.Email == email).FirstOrDefaultAsync();
		}

		public async Task<User?> GetByPasswordResetTokenAsync(string token)
		{
			return await _usersCollection.Find(u => u.PasswordResetToken == token).FirstOrDefaultAsync();
		}

		public async Task<User?> GetByIdAsync(int id)
		{
			return await _usersCollection.Find(u => u.Id == id).FirstOrDefaultAsync();
		}

		public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
		{
			return await _refreshTokensCollection.Find(rt => rt.Token == token).FirstOrDefaultAsync();
		}

		public async Task<User?> GetByEmailVerificationTokenAsync(string token)
		{
			return await _usersCollection.Find(u => u.EmailVerificationToken == token).FirstOrDefaultAsync();
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
	}
}
