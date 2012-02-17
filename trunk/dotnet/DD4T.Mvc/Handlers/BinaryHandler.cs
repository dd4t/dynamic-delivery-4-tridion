namespace DD4T.Mvc.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Globalization;
    using System.Linq;
    using System.Web;
    using System.Web.Caching;
    using DD4T.ContentModel;
    using DD4T.ContentModel.Factories;
	using DD4T.Mvc.Database;
    using DD4T.Factories;
    using DD4T.Utils;

    public class BinaryHandler : IHttpHandler
    {
        #region static members
        private const string BinaryHandlerCachingKey = "BinaryHandlerCaching";

        #endregion

        private static IDictionary<string, object> locks = new Dictionary<string, object>();

        public void ProcessRequest(HttpContext context)
        {
            IBinary binary = null;

            string url = context.Request.Path;
            Cache cache = HttpContext.Current.Cache;

            if (cache[url] != null)
            {
                binary = (IBinary)cache[url];
            }
            else
            {
                binary = BinaryFactory.FindBinary(context.Request.Path);
                int cacheSetting = ConfigurationHelper.BinaryHandlerCacheExpiration;
                cache.Insert(url, binary, null, DateTime.Now.AddSeconds(cacheSetting), TimeSpan.Zero);
            }

            string etagDate = "\"" + binary.LastPublishedDate.ToString("s", DateTimeFormatInfo.InvariantInfo) + "\"";
            string incomingEtag = context.Request.Headers["If-None-Match"];
            context.Response.Cache.SetETag(etagDate);
            context.Response.Cache.SetCacheability(HttpCacheability.Public);

            if (etagDate.Equals(incomingEtag))
            {
                context.Response.StatusCode = 304;
                return;
            }

            var localPath = ConvertUrl(context.Request.Path, context);

            if (binary == null)
            {
                // there appears to be no binary present in Tridion
                // now we must check if there is still an (old) cached copy on the local file system
                object fileLock = LockFile(binary.Url);
                lock (fileLock)
                {
                    RemoveFromDB(binary);

                    lock (locks)
                    {
                        locks.Remove(binary.Url);
                    }
                }

                // that's all for us, the file is not on the FS, so 
                // the default FileHandler will cause a 404 exception
                context.Response.StatusCode = 404;
                context.Response.End();
                return;
            }

            using (var db = new BinariesEntities())
            {
                bool exists = false;
                var binData = db.Binaries.Where(bin => bin.ComponentUri == binary.Id).FirstOrDefault();
                TimeSpan ts = TimeSpan.Zero;
                if (binData != null)
                {
                    ts = binary.LastPublishedDate.Subtract(binData.LastPublishedDate);
                    exists = true;
                }

                if (!exists || ts.TotalMilliseconds > 0)
                {
                    WriteToDb(binary);
                }
            }

            FillResponse(context.Response, binary);
        }

        private void FillResponse(HttpResponse response, IBinary binary)
        {
            response.Clear();

            byte[] imageData = null;
            using (var db = new BinariesEntities())
            {
                var binData = db.Binaries.Where(bin => bin.ComponentUri == binary.Id).FirstOrDefault();
                if (binData != null)
                {
                    imageData = binData.Content;
                    response.ContentType = GetContentType(binData.Path);
                    response.Cache.SetLastModified(binData.LastPublishedDate);
                }
            }

            response.BinaryWrite(imageData);
        }

        public bool IsReusable
        {
            get { return false; }
        }

        #region private/protected
        private IBinaryFactory binaryFactory = null;
        protected IBinaryFactory BinaryFactory
        {
            get
            {
                if (binaryFactory == null)
                {
                    binaryFactory = new BinaryFactory();
                }
                return binaryFactory;
            }
        }

        private string ConvertUrl(string url, HttpContext context)
        {
            return context.Server.MapPath(url);
        }

        private void WriteToDb(IBinary binary)
        {
            object fileLock = LockFile(binary.Url);

            using (var db = new BinariesEntities())
            {
                lock (fileLock)
                {
                    var binData = db.Binaries
                        .Where(bin => bin.ComponentUri == binary.Id).FirstOrDefault();
                    if (binData == null)
                    {
                        binData = new Binaries();
                        db.Binaries.Add(binData);
                    }

                    binData.ComponentUri = binary.Id;
                    binData.Path = binary.Url;
                    binData.LastPublishedDate = binary.LastPublishedDate;

                    binData.Content = binary.BinaryData;

                    db.SaveChanges();
                }
            }

        }

        private object LockFile(string p)
        {
            object fileLock;
            lock (locks)
            {
                if (locks.ContainsKey(p))
                {
                    return locks[p];
                }
                else
                {
                    fileLock = new object();
                    locks.Add(p, fileLock);
                    return fileLock;
                }

            }
        }

        private void RemoveFromDB(IBinary binary)
        {
            using (var db = new BinariesEntities())
            {
                var binData = db.Binaries
                    .Where<Binaries>(bin => bin.Path.Equals(binary.Url)).FirstOrDefault();
                if (binData != null)
                {
                    db.Binaries.Remove(binData);
                    db.SaveChanges();
                }
            }
        }

        private string GetContentType(string urlPath)
        {
            string contentType = "application/octetstream";
            string ext = urlPath.Substring(urlPath.LastIndexOf('.')).ToLower();

            Microsoft.Win32.RegistryKey registryKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);

            if (registryKey != null && registryKey.GetValue("Content Type") != null)
                contentType = registryKey.GetValue("Content Type").ToString();

            return contentType;
        }

        #endregion
    }
}
