using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition.Hosting;

namespace DD4T.Extensions.DynamicDelivery.Mvc
{
    public static class ServiceLocator
    {

        private static CompositionContainer container = null;
        private static AggregateCatalog catalog = null;

        public static void Initialize()
        {
            catalog = new AggregateCatalog(new DirectoryCatalog("bin"));
            container = new CompositionContainer(catalog, true);
        }

        public static T GetInstance<T>()
        {
            if (container == null) Initialize();
            return container.GetExportedValue<T>();
        }
    }
}
