using System;
using System.Collections.Generic;
using MongoDB.Driver;
using ShellRepo.Controllers;
using ShellRepo.Models;

namespace ShellRepo.Engine
{
    public interface IShellEntityRepository
    {
        void Add(ShellContentEntity shellEntity);
        List<ShellContentEntity> Find(string shellName);
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

        public void Add(ShellContentEntity shellEntity)
        {
            var collection = GetMongoCollection();

            collection.InsertOneAsync(shellEntity);
        }

        public List<ShellContentEntity> Find(string shellName)
        {
            return
                GetMongoCollection()
                    .Find(a => a.Name == shellName)
                    .ToList();
        }

        private IMongoCollection<ShellContentEntity> GetMongoCollection()
        {
            return database.GetCollection<ShellContentEntity>(CollectionName);
        }
    }
}