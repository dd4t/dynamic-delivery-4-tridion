using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Unity;
using DD4T.ContentModel;
using DD4T.Factories;
using DD4T.ContentModel.Factories;
using DD4T.Examples.Unity;

namespace DD4T.Examples.Helpers
{
    public static class LinkHelper
    {

        private static ILinkFactory _linkFactory = null;
        private static ILinkFactory LinkFactory
        {
            get
            {
                if (_linkFactory == null)
                    _linkFactory = UnityHelper.Container.Resolve<ILinkFactory>();

                return _linkFactory;
            }
        }



        /// <summary>
        /// resolve the url for the current component via dynamic linking, if component is a multimedia component, then Multimedia.Url will be returned.
        /// </summary>
        /// <param name="component">the component to call the method on</param>
        /// <returns>resolved url</returns>
        public static string GetResolvedUrl(this IComponent component)
        {
            return GetResolvedUrl(component, null, null);
        }

        /// <summary>
        /// resolve the url for the current component via dynamic linking, if component is a multimedia component, then Multimedia.Url will be returned.
        /// </summary>
        /// <param name="component">the component to call the method on</param>        
        /// <param name="sourcePageUri">source page uri</param>
        /// <param name="excludeComponentTemplateUri">component template uri to exclude</param>
        /// <returns>resolved url</returns>
        public static string GetResolvedUrl(this IComponent component, string sourcePageUri, string excludeComponentTemplateUri)
        {
            string link;
            if (component.Multimedia != null)
            {
                link = component.Multimedia.Url;
            }
            else
            {
                if (string.IsNullOrEmpty(sourcePageUri) && string.IsNullOrEmpty(excludeComponentTemplateUri))
                {
                    link = LinkFactory.ResolveLink(component.Id);
                }
                else
                {
                    link = LinkFactory.ResolveLink(sourcePageUri, component.Id, excludeComponentTemplateUri);
                }
            }
            return link;
        }
    }
}
