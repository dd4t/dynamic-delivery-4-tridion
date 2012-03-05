using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tridion.Extensions.DynamicDelivery.ContentModel.Factories;
using Tridion.Extensions.DynamicDelivery.ContentModel;
using System.ComponentModel.Composition;

namespace Tridion.Extensions.DynamicDelivery.Factories.LocalDatabase
{
    [Export(typeof(IBinaryFactory))]
    public class BinaryFactory : IBinaryFactory
    {
        #region IBinaryFactory Members

        public bool TryFindBinary(string url, out IBinary binary)
        {
            throw new NotImplementedException();
        }

        public IBinary FindBinary(string url)
        {
            throw new NotImplementedException();
        }

        public bool TryGetBinary(string tcmUri, out IBinary binary)
        {
            throw new NotImplementedException();
        }

        public IBinary GetBinary(string tcmUri)
        {
            throw new NotImplementedException();
        }

        public bool TryFindBinaryContent(string url, out byte[] bytes)
        {
            throw new NotImplementedException();
        }

        public byte[] FindBinaryContent(string url)
        {
            throw new NotImplementedException();
        }

        public bool TryGetBinaryContent(string tcmUri, out byte[] bytes)
        {
            throw new NotImplementedException();
        }

        public byte[] GetBinaryContent(string tcmUri)
        {
            throw new NotImplementedException();
        }

        public bool HasBinaryChanged(string url)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
