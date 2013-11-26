using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Ninject;

namespace DD4T.Web.Mvc.Ninject
{
    ///<summary>
    /// NinjectDependencyResolver
    /// 2012-10-22
    /// Author: Robert Stevenson-Leggett
    ///</summary>
    public class NinjectDependencyResolver : IDependencyResolver
    {
        private readonly IKernel _kernel;

        public NinjectDependencyResolver(IKernel kernel)
        {
            _kernel = kernel;
        }

        public object GetService(Type serviceType)
        {
            return _kernel.TryGet(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return _kernel.GetAll(serviceType);
        }
    }
}
