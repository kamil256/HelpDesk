using System;
using System.Collections.Generic;
using System.Web.Http.Dependencies;
using Ninject;
using Ninject.Web.Common;
using HelpDesk.DAL.Abstract;
using HelpDesk.DAL.Concrete;
using HelpDesk.UI.Infrastructure.Abstract;
using HelpDesk.UI.Infrastructure.Concrete;
using HelpDesk.BLL.Concrete;
using HelpDesk.BLL.Abstract;

namespace HelpDesk.UI.Infrastructure
{
    public class NinjectResolver : System.Web.Http.Dependencies.IDependencyResolver, System.Web.Mvc.IDependencyResolver
    {
        private IKernel kernel;

        public NinjectResolver() : this(new StandardKernel())
        {
        }

        public NinjectResolver(IKernel ninjectKernel)
        {
            kernel = ninjectKernel;
            AddBindings(kernel);
        }
        public IDependencyScope BeginScope()
        {
            return this;
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
            kernel.Bind<IUnitOfWork>().To<UnitOfWork>().InRequestScope();
            kernel.Bind<IUserService>().To<UserService>().InRequestScope();
            kernel.Bind<IRoleService>().To<RoleService>().InRequestScope();
            kernel.Bind<IIdentityHelper>().To<IdentityHelper>().InRequestScope();
            kernel.Bind<IEmailSender>().To<EmailSender>().InRequestScope();
        }        
    }
}