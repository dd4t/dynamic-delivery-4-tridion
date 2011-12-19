using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DD4T.ContentModel.Contracts.Providers;
using DD4T.ContentModel.Querying;

namespace DD4T.ContentModel.Factories
{
    public interface IComponentFactory
    {
        IComponentProvider ComponentProvider { get; set; }
        bool TryGetComponent(string componentUri, out IComponent component);
        IComponent GetComponent(string componentUri);
        IList<IComponent> GetComponents(string[] componentUris);
		IList<IComponent> FindComponents(IQuery queryParameters);
        IList<IComponent> FindComponents(IQuery queryParameters, int pageIndex, int pageSize, out int totalCount);
        IComponent GetIComponentObject(string componentStringContent);
    }
}
