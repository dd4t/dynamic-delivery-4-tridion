using DD4T.WebApplication.DependencyInjection;
using Ninject;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace DD4T.WebApplication
{
    public static class DependencyInjectionConfig
    {
        public static IKernel Configure()
        {
            int publicationId = 0;
            if(!int.TryParse(ConfigurationManager.AppSettings["DD4T.PublicationId"], out publicationId))
            {
                throw new ConfigurationErrorsException("Missing or invalid DD4T.PublicationId, please ensure this exists in the appSettings element in your web.config and contains an integer value");
            }
            
            IKernel kernel = new StandardKernel(new DD4TNinjectModule(publicationId));
            BindServices(kernel);
            return kernel;
        }

        /// <summary>
        /// Bind your own services here
        /// </summary>
        private static void BindServices(IKernel kernel)
        {
            // TODO: Inject services for your controllers
        }
    }
}