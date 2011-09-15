using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tridion.Extensions.DynamicDelivery.ContentModel.Factories;
using System.ComponentModel.Composition;

namespace Tridion.Extensions.DynamicDelivery.Factories.LocalDatabase
{
    [Export(typeof(ILinkFactory))]
    public class LinkFactory : ILinkFactory
    {
        #region ILinkFactory Members

        public string ResolveLink(string componentUri)
        {
            throw new NotImplementedException();
        }

        public string ResolveLink(string sourcePageUri, string componentUri, string excludeComponentTemplateUri)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
