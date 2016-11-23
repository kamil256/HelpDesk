using System;
using System.Collections.Generic;
using System.Web.Http.Dependencies;
using Ninject;
using Ninject.Extensions.ChildKernel;
using HelpDesk.DAL.Abstract;
using HelpDesk.DAL.Concrete;
using Ninject.Web.WebApi.Filter;
using System.Linq;

namespace HelpDesk.UI.Infrastructure
{
    // TODO: not working
    public class NinjectResolver : IDependencyResolver
    {
        private IKernel kernel;
        public NinjectResolver() : this(new StandardKernel()) { }
public NinjectResolver(IKernel ninjectKernel, bool scope = false)
        {
            kernel = ninjectKernel;
            if (!scope)
            {
                AddBindings(kernel);
            }
        }
        public IDependencyScope BeginScope()
        {
            return new NinjectResolver(AddRequestBindings(
            new ChildKernel(kernel)), true);
        }
        public object GetService(Type serviceType)
        {
            return kernel.TryGet(serviceType);
        }
        public IEnumerable<object> GetServices(Type serviceType)
        {
            return kernel.GetAll(serviceType);
        }
        public void Dispose()
        {
            // do nothing
        }
        private void AddBindings(IKernel kernel)
        {
            // singleton and transient bindings go here
        }
        private IKernel AddRequestBindings(IKernel kernel)
        {
            kernel.Bind<DefaultModelValidatorProviders>().ToConstant(new DefaultModelValidatorProviders(GetServices(typeof(System.Web.Http.Validation.ModelValidatorProvider)).Cast<System.Web.Http.Validation.ModelValidatorProvider>()));
            kernel.Bind<DefaultFilterProviders>().ToConstant(new DefaultFilterProviders(new[] { new NinjectFilterProvider(kernel) }.AsEnumerable()));
            kernel.Bind<IUnitOfWork>().To<UnitOfWork>().InSingletonScope();
            return kernel;
        }
    }
}