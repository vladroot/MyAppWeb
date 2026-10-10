using MongoDB.Driver;

namespace MyAppWeb
{
    public interface IUserRepository
    {
        Task<List<User>> GetAllAsync();
        Task<User?> GetByUniqueAsync(string unique);
        Task InsertOneAsync(User user);
    }

    public class UserRepository : IUserRepository
    {
        private readonly IMongoCollection<User> _usersCollection;

        public UserRepository(IMongoDatabase db)
        {
            _usersCollection = db.GetCollection<User>("users");
        }

        public async Task<List<User>> GetAllAsync()
        {
            return await _usersCollection.Find(_ => true).ToListAsync();
        }

        public async Task<User?> GetByUniqueAsync(string unique)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Unique, unique);
            return await _usersCollection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task InsertOneAsync(User user)
        {
            await _usersCollection.InsertOneAsync(user);
        }
    }
}