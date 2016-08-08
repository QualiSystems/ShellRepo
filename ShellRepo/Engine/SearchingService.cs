using System.Collections.Generic;
using System.Linq;
using ShellRepo.Models;

namespace ShellRepo.Engine
{
    public interface ISearchingService
    {
        List<ShellEntity> Search(string text);
        void Index(ShellEntity shellEntity);
    }

    public class SearchingService : ISearchingService
    {
        private readonly IElasticClientFactory elasticClientFactory;

        public SearchingService(IElasticClientFactory elasticClientFactory)
        {
            this.elasticClientFactory = elasticClientFactory;
        }

        public List<ShellEntity> Search(string text)
        {
            return
                elasticClientFactory.CreateElasticClient()
                    .Search<ShellEntity>(_ => _.From(0).Size(10).Query(q => q.Term(s => s.Name, text)))
                    .Documents.ToList();
        }

        public void Index(ShellEntity shellEntity)
        {
            elasticClientFactory.CreateElasticClient().Index(shellEntity);
        }
    }
}