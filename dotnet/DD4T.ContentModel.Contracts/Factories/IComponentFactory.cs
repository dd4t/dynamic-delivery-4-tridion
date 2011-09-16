using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DD4T.ContentModel.Factories
{
    public interface IComponentFactory
    {
        bool TryGetComponent(string componentUri, out IComponent component);
        IComponent GetComponent(string componentUri);
        List<IComponent> GetComponents(string[] componentUris);
		IList<string> FindComponents(ExtendedQueryParameters queryParameters);
        IComponent GetIComponentObject(string componentStringContent);
    }
}
