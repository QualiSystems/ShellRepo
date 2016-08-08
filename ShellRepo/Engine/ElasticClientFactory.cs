using System;
using Nest;

namespace ShellRepo.Engine
{
    public interface IElasticClientFactory
    {
        IElasticClient CreateElasticClient();
    }

    public class ElasticClientFactory : IElasticClientFactory
    {
        private readonly IShellRepoConfiguration shellRepoConfiguration;

        public ElasticClientFactory(IShellRepoConfiguration shellRepoConfiguration)
        {
            this.shellRepoConfiguration = shellRepoConfiguration;
        }

        public IElasticClient CreateElasticClient()
        {
            var settings = new ConnectionSettings(new Uri(shellRepoConfiguration.ElasticSearchUrl));
            settings.DefaultIndex("shell");
            return new ElasticClient(settings);
        }
    }
}