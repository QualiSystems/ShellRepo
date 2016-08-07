using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using ShellRepo.Exceptions;
using ShellRepo.Models;

namespace ShellRepo.Engine
{
    public interface IShellEntityRepository
    {
        ShellEntity Get(ObjectId shellEntityId);
        void IncrementDownloadCount(ObjectId objectId);
        ShellEntity Find(string shellName);
        void Insert(ShellEntity shellEntity);
        void Update(ShellEntity shellEntity);
        List<ShellEntity> GetAll();
    }

    public class ShellEntityRepository : IShellEntityRepository
    {
        private readonly IMongoDbClientFactory mongoDbClientFactory;

        public ShellEntityRepository(IMongoDbClientFactory mongoDbClientFactory)
        {
            this.mongoDbClientFactory = mongoDbClientFactory;
        }


        public ShellEntity Get(ObjectId shellEntityId)
        {
            var shellEntities = mongoDbClientFactory.GetMongoCollection<ShellEntity>()
                .FindSync(new FilterDefinitionBuilder<ShellEntity>()
                    .Where(s => s.Id == shellEntityId))
                .ToList();
            if (!shellEntities.Any())
            {
                throw new ShellNotFoundException(string.Format("Shell entity with Id {0} not found", shellEntityId));
            }
            return shellEntities.Single();
        }

        public void IncrementDownloadCount(ObjectId objectId)
        {
            mongoDbClientFactory.GetMongoCollection<ShellEntity>()
                .FindOneAndUpdate(
                    new FilterDefinitionBuilder<ShellEntity>().Where(s => s.Id == objectId),
                    Builders<ShellEntity>.Update.Inc(s => s.DownloadCount, 1));
        }

        public ShellEntity Find(string shellName)
        {
            var shellEntities = mongoDbClientFactory.GetMongoCollection<ShellEntity>()
                .FindSync(new FilterDefinitionBuilder<ShellEntity>()
                    .Where(s => s.Name == shellName))
                .ToList();

            return !shellEntities.Any() ? null : shellEntities.Single();
        }

        public void Insert(ShellEntity shellEntity)
        {
            mongoDbClientFactory.GetMongoCollection<ShellEntity>().InsertOne(shellEntity);
        }

        public async void Update(ShellEntity shellEntity)
        {
            await mongoDbClientFactory.GetMongoCollection<ShellEntity>()
                .ReplaceOneAsync(new FilterDefinitionBuilder<ShellEntity>().Where(a => a.Id == shellEntity.Id),
                    shellEntity);
        }

        public List<ShellEntity> GetAll()
        {
            return mongoDbClientFactory.GetMongoCollection<ShellEntity>()
                .FindSync(new FilterDefinitionBuilder<ShellEntity>().Empty)
                .ToList();
        }
    }
}