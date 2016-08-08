using MongoDB.Driver;

namespace ShellRepo.Engine
{
    public interface IMongoDbClientFactory
    {
        IMongoCollection<T> GetMongoCollection<T>();
    }

    public class MongoDbClientFactory : IMongoDbClientFactory
    {
        private readonly IMongoDatabase database;

        public MongoDbClientFactory(IShellRepoConfiguration shellRepoConfiguration)
        {
            var mongoUrl = new MongoUrl(shellRepoConfiguration.MongoConnectionString);

            database = new MongoClient(mongoUrl).GetDatabase(mongoUrl.DatabaseName);
        }

        public IMongoCollection<T> GetMongoCollection<T>()
        {
            return database.GetCollection<T>(typeof (T).Name);
        }
    }
}