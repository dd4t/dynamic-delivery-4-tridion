using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Unity.Configuration;
using System.Configuration;
using Microsoft.Practices.Unity;

namespace Sample.Web.Mvc.Unity
{
    public static class UnityHelper
    {
        private static IUnityContainer container = null;

        public static T GetInstance<T>()
        {
            if (container == null)
            {
                container = new UnityContainer();
                UnityConfigurationSection section
                  = (UnityConfigurationSection)ConfigurationManager.GetSection("unity");
                section.Configure(container, "main");
            }
            return container.Resolve<T>();
        }
    }
}
