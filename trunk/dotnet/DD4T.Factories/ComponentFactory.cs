using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DD4T.ContentModel.Factories;
using DD4T.ContentModel;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;
using DD4T.ContentModel.Exceptions;
using System.IO;
using System.Configuration;
using DD4T.ContentModel.Contracts.Providers;
using DD4T.Providers.SDLTridion2011;

namespace DD4T.Factories
{
    /// <summary>
    /// Factory for the creation of IComponents
    /// </summary>
    public class ComponentFactory : FactoryBase, IComponentFactory
    {
        private IComponentProvider componentProvider = null;
        public IComponentProvider ComponentProvider
        {
            get
            {
                // TODO: implement DI
                if (componentProvider == null)
                {
                    componentProvider = new TridionComponentProvider();
                    componentProvider.PublicationId = this.PublicationId;
                }
                return componentProvider;
            }
            set
            {
                componentProvider = value;
            }
        }

        #region IComponentFactory members
        public bool TryGetComponent(string componentUri, out IComponent component)
        {
            component = null;

            string content = componentProvider.GetContent(componentUri);

            if (string.IsNullOrEmpty(content))
            {
                return false;
            }

            component = GetIComponentObject(content);
            return true;

        }




        public IComponent GetComponent(string componentUri)
        {
            IComponent component;
            if (!TryGetComponent(componentUri, out component))
            {
                throw new ComponentNotFoundException();
            }

            return component;
        }


        /// <summary>
        /// Create IComponent from a string representing that IComponent (XML)
        /// </summary>
        /// <param name="componentStringContent">XML content to deserialize into an IComponent</param>
        /// <returns></returns>
        public IComponent GetIComponentObject(string componentStringContent)
        {
            XmlDocument componentContent = new XmlDocument();
            componentContent.LoadXml(componentStringContent);
            var serializer = new XmlSerializer(typeof(Component));
            IComponent component = null;
            using (var reader = new XmlNodeReader(componentContent.DocumentElement))
            {
                component = (IComponent)serializer.Deserialize(reader);
            }
            return component;
        }


        /// <summary>
        /// Returns the Component contents which could be found. Components that couldn't be found don't appear in the list. 
        /// </summary>
        /// <param name="componentUris"></param>
        /// <returns></returns>
        public IList<IComponent> GetComponents(string[] componentUris)
        {
            List<IComponent> components = new List<IComponent>();
            foreach (string content in ComponentProvider.GetContentMultiple(componentUris))
            {
                components.Add(GetIComponentObject(content));
            }
            return components;
        }

        public IList<IComponent> FindComponents(ExtendedQueryParameters queryParameters)
        {
            var results = ComponentProvider.FindComponents(queryParameters)
                .Select(c => GetComponent(c))
                .ToList();
            return results;
        }

		#endregion
	}
}
