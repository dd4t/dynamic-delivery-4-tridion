using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DD4T.ContentModel;

namespace DD4T.Templates.Base.Serializing
{
    public interface ISerializerService
    {
        string Serialize<T>(T input) where T : RepositoryLocalItem;
        T Deserialize<T>(string input);
    }
}
