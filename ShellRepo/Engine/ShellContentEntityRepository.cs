using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using ShellRepo.Models;

namespace ShellRepo.Engine
{
    public interface IShellContentEntityRepository
    {
        Task Add(ShellContentEntity shellEntity);
        List<ShellContentEntity> Find(string shellName, Version version = null);
        void IncrementDownloadCount(ObjectId shellContentEntityId);
        List<ShellContentEntity> GetShellContentEntities(string shellName);
    }

    public class ShellContentEntityRepository : IShellContentEntityRepository
    {
        private readonly IMongoDbClientFactory mongoDbClientFactory;

        public ShellContentEntityRepository(IMongoDbClientFactory mongoDbClientFactory)
        {
            this.mongoDbClientFactory = mongoDbClientFactory;
        }

        public async Task Add(ShellContentEntity shellEntity)
        {
            var collection = mongoDbClientFactory.GetMongoCollection<ShellContentEntity>();

            await collection.InsertOneAsync(shellEntity);
        }

        public List<ShellContentEntity> Find(string shellName, Version version = null)
        {
            Expression<Func<ShellContentEntity, bool>> expression;
            if (version == null)
            {
                expression = s => s.Name == shellName;
            }
            else
            {
                expression = s => s.Name == shellName && s.Version == version;
            }
            return mongoDbClientFactory.GetMongoCollection<ShellContentEntity>()
                .FindSync(new FilterDefinitionBuilder<ShellContentEntity>()
                    .Where(expression))
                .ToList();
        }

        public List<ShellContentEntity> GetShellContentEntities(string shellName)
        {
            return mongoDbClientFactory.GetMongoCollection<ShellContentEntity>()
                .FindSync(new FilterDefinitionBuilder<ShellContentEntity>()
                    .Where(s => s.Name == shellName))
                .ToList();
        }

        public void IncrementDownloadCount(ObjectId shellContentEntityId)
        {
            mongoDbClientFactory.GetMongoCollection<ShellContentEntity>()
                .FindOneAndUpdate(
                    new FilterDefinitionBuilder<ShellContentEntity>().Where(s => s.Id == shellContentEntityId),
                    Builders<ShellContentEntity>.Update.Inc(s => s.DownloadCount, 1));
        }
    }
}