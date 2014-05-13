using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DD4T.ContentModel;

namespace DD4T.ContentModel.Contracts.Serializing
{
    public interface ISerializerService
    {
        string Serialize<T>(T input) where T : IRepositoryLocal;
        T Deserialize<T>(string input) where T : IRepositoryLocal;
        bool IsAvailable();
    }
}
