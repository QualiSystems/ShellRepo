using System.Web.Configuration;

namespace ShellRepo.Engine
{
    public interface IShellRepoConfiguration
    {
        string MongoConnectionString { get; }
        string ElasticSearchUrl { get; }
        string ElasticSearchSecureUrl { get; }
    }

    public class ShellRepoConfiguration : IShellRepoConfiguration
    {
        public string MongoConnectionString
        {
            get { return WebConfigurationManager.AppSettings["MONGOLAB_URI"]; }
        }
        public string ElasticSearchUrl
        {
            get { return WebConfigurationManager.AppSettings["SEARCHBOX_URL"]; }
        }
        public string ElasticSearchSecureUrl
        {
            get { return WebConfigurationManager.AppSettings["SEARCHBOX_SSL_URL"]; }
        }
    }
}