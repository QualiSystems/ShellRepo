using System.Web.Configuration;

namespace ShellRepo.Controllers
{
    public interface IShellRepoConfiguration
    {
        string MongoConnectionString { get; }
    }

    public class ShellRepoConfiguration : IShellRepoConfiguration
    {
        public string MongoConnectionString
        {
            get { return WebConfigurationManager.AppSettings["MONGOLAB_URI"]; }
        }
    }
}