using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DD4T.Extensions.DynamicDelivery.ContentModel.Factories
{
    public interface IComponentFactory
    {
        bool TryGetComponent(string componentUri, out IComponent component);
        IComponent GetComponent(string componentUri);
        IList<IComponent> FindComponents(string schemaUri);
        IList<IComponent> FindComponents(string[] schemaUris);
        IList<IComponent> FindComponents(string[] schemaUris, DateTime sinceLastPublished);
		IList<string> FindComponents(ExtendedQueryParameters queryParameters);
        IDictionary<string, IComponentMeta> FindComponentMetas(string[] schemaUri);
        IDictionary<string, IComponentMeta> FindComponentMetas(string[] schemaUri, DateTime sinceLastPublished);
        IComponent GetLastPublishedComponent(string schemaUri);
        DateTime LastPublished(string[] schemaUris);
    }
}
