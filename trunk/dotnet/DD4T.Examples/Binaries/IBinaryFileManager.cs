using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using DD4T.ContentModel;

namespace DD4T.Examples.Binaries
{
    public interface IBinaryFileManager
    {
        bool ProcessRequest(HttpRequest request);
        TcmUri GetBinaryUri(HttpRequest request);
    }
}
