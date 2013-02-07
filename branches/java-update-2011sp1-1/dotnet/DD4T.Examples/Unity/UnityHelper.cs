using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Unity.Configuration;
using System.Configuration;
using Microsoft.Practices.Unity;

namespace DD4T.Examples.Unity
{
    public static class UnityHelper
    {
        private static UnityContainer _container = null;

        private static object lock1 = new object();
        public static UnityContainer Container
        {
            get
            {
                if (_container != null)
                    return _container;
                lock (lock1)
                {
                    if (_container == null)
                    {
                        _container = new UnityContainer();
                        UnityConfigurationSection section
                          = (UnityConfigurationSection)ConfigurationManager.GetSection("unity");
                        section.Configure(_container, "main");
                    }
                }
                return _container;
            }
        }
    }
}
