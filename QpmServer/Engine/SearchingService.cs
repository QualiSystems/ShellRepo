using System;
using System.Collections.Generic;
using System.Linq;
using Nest;
using ShellRepo.Models;

namespace ShellRepo.Engine
{
    public interface ISearchingService
    {
        List<ShellEntity> Search(string text);
    }

    public class SearchingService : ISearchingService
    {
        private readonly ElasticClient client;

        public SearchingService(IShellRepoConfiguration shellRepoConfiguration)
        {
            var settings = new ConnectionSettings(new Uri(shellRepoConfiguration.ElasticSearchUrl));
            settings.DefaultIndex("shell");

            client = new ElasticClient(settings);
        }

        public List<ShellEntity> Search(string text)
        {
            return
                client.Search<ShellEntity>(_ => _.From(0).Size(10).Query(q => q.Term(s => s.Name, text)))
                    .Documents.ToList();
        }
    }
}