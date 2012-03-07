using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tridion.Extensions.DynamicDelivery.ContentModel.Factories;
using System.ComponentModel.Composition;
using Tridion.Extensions.DynamicDelivery.ContentModel;

namespace Tridion.Extensions.DynamicDelivery.Factories.LocalDatabase
{
    [Export(typeof(IComponentFactory))]
    public class ComponentFactory : IComponentFactory
    {
        #region IComponentFactory Members

        public bool TryGetComponent(string componentUri, out IComponent component)
        {
            throw new NotImplementedException();
        }

        public IComponent GetComponent(string componentUri)
        {
            throw new NotImplementedException();
        }

        public IList<IComponent> FindComponents(string schemaUri)
        {
            throw new NotImplementedException();
        }

        public IList<IComponent> FindComponents(string[] schemaUris)
        {
            throw new NotImplementedException();
        }

        public IList<IComponent> FindComponents(string[] schemaUris, DateTime sinceLastPublished)
        {
            throw new NotImplementedException();
        }

        public IDictionary<string, IComponentMeta> FindComponentMetas(string[] schemaUri)
        {
            throw new NotImplementedException();
        }

        public IDictionary<string, IComponentMeta> FindComponentMetas(string[] schemaUri, DateTime sinceLastPublished)
        {
            throw new NotImplementedException();
        }

        public IComponent GetLastPublishedComponent(string schemaUri)
        {
            throw new NotImplementedException();
        }

        public DateTime LastPublished(string[] schemaUris)
        {
            throw new NotImplementedException();
        }

        #endregion

		#region IComponentFactory Members


		public IList<string> FindComponents(ExtendedQueryParameters queryParameters) {
			throw new NotImplementedException();
		}

		#endregion
	}
}
