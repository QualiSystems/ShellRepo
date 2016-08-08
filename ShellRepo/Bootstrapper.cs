using System.Reflection;
using Autofac;
using Autofac.Integration.WebApi;

namespace ShellRepo
{
    public class Bootstrapper
    {
        private readonly IContainer container;

        public Bootstrapper()
        {
            var builder = new ContainerBuilder();

            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

            builder.RegisterAssemblyTypes(typeof (Bootstrapper).Assembly)
                .AsImplementedInterfaces();

            container = builder.Build();
        }

        public IContainer GetContainer()
        {
            return container;
        }

        public T Get<T>()
        {
            return container.Resolve<T>();
        }

        public AutofacWebApiDependencyResolver GetDependencyResolver()
        {
            return new AutofacWebApiDependencyResolver(container);
        }
    }
}