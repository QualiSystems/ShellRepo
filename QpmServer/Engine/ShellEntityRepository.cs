using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using ShellRepo.Controllers;
using ShellRepo.Models;

namespace ShellRepo.Engine
{
    public interface IShellEntityRepository
    {
        Task Add(ShellContentEntity shellEntity);
        List<ShellContentEntity> Find(string shellName);
    }

    public class ShellEntityRepository : IShellEntityRepository
    {
        private const string CollectionName = "ShellEntity";
        private readonly IMongoDatabase database;

        public ShellEntityRepository(IShellRepoConfiguration shellRepoConfiguration)
        {
            var mongoUrl = new MongoUrl(shellRepoConfiguration.MongoConnectionString);
            database = new MongoClient(mongoUrl).GetDatabase(mongoUrl.DatabaseName);
        }

        public async Task Add(ShellContentEntity shellEntity)
        {
            var collection = GetMongoCollection();

            await collection.InsertOneAsync(shellEntity);
        }

        public List<ShellContentEntity> Find(string shellName)
        {
            return
                GetMongoCollection().FindSync(new FilterDefinitionBuilder<ShellContentEntity>().Where(s=>s.Name == shellName))
                    .ToList();
        }

        private IMongoCollection<ShellContentEntity> GetMongoCollection()
        {
            return database.GetCollection<ShellContentEntity>(CollectionName);
        }
    }
}