using Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Repositories
{
    public class MongoDBUserRepository : IMongoDBRepository
    {
        private readonly IMongoCollection<UserModel> UserCollection;

        public MongoDBUserRepository(IOptions<MongoDBSettings> mongoDBSettings)
        {
            // trækker connection string og database navn og collectionname fra program.cs aka fra terminalen ved export. Dette er en constructor injection.
            MongoClient client = new MongoClient(mongoDBSettings.Value.ConnectionURI);
            IMongoDatabase database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);
            UserCollection = database.GetCollection<UserModel>(mongoDBSettings.Value.CollectionName);
        }


        public async Task<bool> CheckIfUserExistsAsync(string Username)
        {
            // Finder en bruger med det indtastede brugernavn ved at lave en midligertidig instans af LoginModel (new BsonDocument("Username", Username) og derefter finde den første bruger med det brugernavn.
            var user = await UserCollection.Find(new BsonDocument("Username", Username)).FirstOrDefaultAsync();
            // Hvis brugeren findes returneres true, ellers false.
            return user != null;
        }

        public async Task AddUserAsync(UserModel login)
        {
            login.Role = "User";
            login.registrationDate = DateTime.UtcNow.AddHours(2);
            await UserCollection.InsertOneAsync(login);
        }

        public async Task<UserModel> FindUserAsync(Guid id)
        {
            return await UserCollection.Find(user => user.Id == id).FirstOrDefaultAsync();
        }

        public async Task UpdateUserAsync(UserModel login)
        {
            // Opret et filter baseret på user Id. Bruger Builder fra mongodb biblio. Eq står for equals og matcher id'erne med dem man taster ind fra parameteren.
            var filter = Builders<UserModel>.Filter.Eq(x => x.Id, login.Id);
            // Laver en opdatering baseret på de nye værdier af user og "replacer" dem.
            var update = Builders<UserModel>.Update
               .Set(x => x.Username, login.Username)
               .Set(x => x.Password, login.Password)
               .Set(x => x.Role, login.Role)
               .Set(x => x.Email, login.Email);

            // erstatter den gamle med det nye man har valgt (username, password).
            await UserCollection.ReplaceOneAsync(filter, login);
        }

        public async Task DeleteUserAsync(Guid id)
        {
            // note: skal laves om til .BsonDocument() da DeleteOneAsync() kun kan fjerne 1 dokument og ik objekt
            var UserDerSkalSlettes = UserCollection.Find(user => user.Id == id).FirstOrDefault().ToBsonDocument();
            await UserCollection.DeleteOneAsync(UserDerSkalSlettes);
        }

        public async Task<bool> CheckIfUserExistsWithPassword(string Username, string Password, string Role)
        {
            // Bruger find for at finde en bruger med det indtastede brugernavn og password og role. Hvis brugeren findes returneres den ellers null.
            var user = await UserCollection.Find(new BsonDocument("Username", Username).Add("Password", Password).Add("Role", Role)).FirstOrDefaultAsync();
            // Hvis brugeren findes returneres true, ellers false.
            return user != null;
        }

    }
}