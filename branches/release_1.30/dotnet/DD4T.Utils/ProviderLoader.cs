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

        /// <summary>
        /// Load the best matching provider for the specified type param
        /// By default, the most recent Tridion version is used
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
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

        /// <summary>
        /// Load the best matching provider for the specified type param
        /// By default, the most recent Tridion version is used
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="publicationId">Publication ID to set on the provider</param>
        /// <returns></returns>
        public static IProvider LoadProvider<T>(int publicationId) where T : class
        {
            ProviderVersion version = ConfigurationHelper.ProviderVersion;
            if (version == ProviderVersion.Undefined)
            {
                version = ProviderAssemblyNames.DefaultProviderVersion;
            }
            string classIdentifier = ProviderAssemblyNames.GetProviderClassName(version);
            IProvider p = (IProvider)ClassLoader.Load<T>(classIdentifier);
            p.PublicationId = publicationId;
            return p;
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
