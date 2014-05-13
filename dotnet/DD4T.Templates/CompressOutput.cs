using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.XPath;
using System.Xml.Xsl;
using System.Xml;
using System.IO;
using System.Reflection;
using Tridion.ContentManager.Templating;
using Tridion.ContentManager.Templating.Assembly;
using DD4T.Templates.Base;
using DD4T.Templates.Base.Utils;
using DD4T.ContentModel;
using System.IO.Compression;

namespace DD4T.Templates.XML
{

    [TcmTemplateTitle("Compress Output")]
    public class CompressOutput : ITemplate
    {
        protected TemplatingLogger log = TemplatingLogger.GetLogger(typeof(ConvertXmlToJava));
        protected Package package;
        protected Engine engine;


        public void Transform(Engine engine, Package package)
        {
            this.package = package;
            this.engine = engine;

            Item outputItem = package.GetByName("Output");
            String inputValue = package.GetValue("Output");

            if (inputValue == null || inputValue.Length == 0)
            {
                log.Warning("Could not find 'Output' in the package, nothing to transform");
                return;
            }

            string outputValue = Compress(inputValue);           

            // replace the Output item in the package
            package.Remove(outputItem);
            outputItem.SetAsString(outputValue);
            package.PushItem("Output", outputItem);
        }



        public static string Compress(string s)
        {
            return CompressGzip(s);
        }
        private static string CompressGzip(string s)
        {
            var bytes = Encoding.Unicode.GetBytes(s);
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(mso, CompressionMode.Compress))
                {
                    msi.CopyTo(gs);
                }
                return Convert.ToBase64String(mso.ToArray());
            }
        }

        //private static string CompressLZ4(string s)
        //{
        //    byte[] compressed = LZ4. Lz4.CompressBytes(buffer, 0, buffer.Length, Lz4Net.Lz4Mode.Fast);
        //}

        public static string Decompress(string s)
        {
            return DecompressGzip(s);
        }
        private static string DecompressGzip(string s)
        {
            var bytes = Convert.FromBase64String(s);
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                {
                    gs.CopyTo(mso);
                }
                return Encoding.Unicode.GetString(mso.ToArray());
            }
        }
    }
}
