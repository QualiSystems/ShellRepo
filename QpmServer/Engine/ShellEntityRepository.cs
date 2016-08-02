using System;
using System.Collections.Generic;
using MongoDB.Driver;

namespace ShellRepo.Controllers
{
    public interface IShellEntityRepository
    {
        void Add(ShellEntity shellEntity);
        List<ShellEntity> Find(string shellName);
    }

    public class ShellEntityRepository : IShellEntityRepository
    {
        private const string DatabaseName = "ShellRepo";
        private const string CollectionName = "ShellEntity";
        private readonly IMongoDatabase database;

        public ShellEntityRepository(IShellRepoConfiguration shellRepoConfiguration)
        {
            var client = new MongoClient(shellRepoConfiguration.MongoConnectionString);
            database = client.GetDatabase(DatabaseName);
        }

        public void Add(ShellEntity shellEntity)
        {
            var collection = GetMongoCollection();

            collection.InsertOneAsync(shellEntity);
        }

        public List<ShellEntity> Find(string shellName)
        {
            return
                GetMongoCollection()
                    .Find(a => string.Compare(a.Name, shellName, StringComparison.InvariantCultureIgnoreCase) == 0)
                    .ToList();
        }

        private IMongoCollection<ShellEntity> GetMongoCollection()
        {
            return database.GetCollection<ShellEntity>(CollectionName);
        }
    }
}