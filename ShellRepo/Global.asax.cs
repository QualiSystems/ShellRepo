using System.Web;
using System.Web.Http;

namespace ShellRepo
{
    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);

            GlobalConfiguration.Configuration.DependencyResolver = new Bootstrapper().GetDependencyResolver();
        }
    }
}