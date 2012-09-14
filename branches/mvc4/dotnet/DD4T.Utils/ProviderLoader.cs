using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DD4T.ContentModel.Contracts.Providers;
using DD4T.ContentModel.Factories;
using System.Reflection;

namespace DD4T.Utils
{
    public static class ProviderLoader
    {
        //IPageFactory forPageFactory = null;
        //IComponentFactory forComponentFactory = null;
        //IBinaryFactory forBinaryFactory = null;
        //ITaxonomyFactory forTaxonomyFactory = null;
        //ILinkFactory forLinkFactory = null;

        //public ProviderLoader(IPageFactory factory)
        //{
        //    forPageFactory = factory;
        //}
        //public ProviderLoader(IComponentFactory factory)
        //{
        //    forComponentFactory = factory;
        //}
        //public ProviderLoader(IBinaryFactory factory)
        //{
        //    forBinaryFactory = factory;
        //}
        //public ProviderLoader(ITaxonomyFactory factory)
        //{
        //    forTaxonomyFactory = factory;
        //}
        //public ProviderLoader(ILinkFactory factory)
        //{
        //    forLinkFactory = factory;
        //}


        public static IProvider LoadProvider<T>() where T : class
        {
            ProviderVersion version = ConfigurationHelper.ProviderVersion;
            if (version == ProviderVersion.Undefined)
            {
                version = ProviderAssemblyNames.DefaultProviderVersion;
            }
            string classIdentifier = ProviderAssemblyNames.GetProviderClassName(version);
            return (IProvider)ClassLoader.Load<T>(classIdentifier);
        }

        //private Assembly GetCurrentProviderAssembly()
        //{
        //    if (forPageFactory != null)
        //        return forPageFactory.PageProvider.GetType().Assembly;
        //    if (forComponentFactory != null)
        //        return forComponentFactory.ComponentProvider.GetType().Assembly;
        //    if (forBinaryFactory != null)
        //        return forBinaryFactory.BinaryProvider.GetType().Assembly;
        //    if (forTaxonomyFactory != null)
        //        return forTaxonomyFactory.TaxonomyProvider.GetType().Assembly;
        //    if (forLinkFactory != null)
        //        return forLinkFactory.LinkProvider.GetType().Assembly;
        //    return null;
        //}
    }
}
