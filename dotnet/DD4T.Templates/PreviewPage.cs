using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tridion.ContentManager.Templating.Assembly;
using Tridion.ContentManager.CommunicationManagement;
using Dynamic = DD4T.ContentModel;
using DD4T.Templates;
using System.Net;
using System.IO;
using Tridion.ContentManager.Templating;
using Tridion.ContentManager.Publishing;
using DD4T.Templates.Base.Utils;
using Tcm = Tridion.ContentManager;


namespace DD4T.Templates
{
   [TcmTemplateTitle("Preview page")]
    public class PreviewPage : ITemplate
    {
        protected TemplatingLogger log = TemplatingLogger.GetLogger(typeof(PreviewPage));
        protected Package package;
        protected Engine engine;

        public void Transform(Engine engine, Package package)
        {

            // do NOT execute this logic when we are actually publishing! (similair for fast track publishing)
            if (engine.RenderMode == RenderMode.Publish || (engine.PublishingContext.PublicationTarget != null && !Tcm.TcmUri.IsNullOrUriNull(engine.PublishingContext.PublicationTarget.Id)))
            {
                return;
            }

            Item outputItem = package.GetByName("Output");
            String inputValue = package.GetValue("Output");

            if (string.IsNullOrEmpty(inputValue))
            {
                log.Warning("Could not find 'Output' in the package, nothing to preview");
                return;
            }

            // read staging url from configuration
            string stagingUrl = TridionConfigurationManager.GetInstance(engine, package).AppSettings["StagingUrl"];
            string outputValue = HttpPost(stagingUrl, inputValue);
            if (string.IsNullOrEmpty(outputValue))
            {
                outputValue = "<h2>There was an error while generating the preview.</h2>";
            }

            // replace the Output item in the package
            package.Remove(outputItem);
            package.PushItem("Output", package.CreateStringItem(ContentType.Html, outputValue));
        }


       string HttpPost(string uri, string parameters)
       {

#if DEBUG
           log.Debug("About to post to " + uri);
#endif
           WebRequest webRequest = WebRequest.Create(uri);
           //string ProxyString = 
           //   System.Configuration.ConfigurationManager.AppSettings
           //   [GetConfigKey("proxy")];
           //webRequest.Proxy = new WebProxy (ProxyString, true);
           //Commenting out above required change to App.Config
           webRequest.ContentType = "application/x-www-form-urlencoded";
           webRequest.Method = "POST";
           byte[] bytes = Encoding.UTF8.GetBytes(parameters);
           Stream os = null;
           try
           { // send the Post
               webRequest.ContentLength = bytes.Length;   //Count bytes to send
#if DEBUG
               log.Debug("content length " + bytes.Length);
#endif
               os = webRequest.GetRequestStream();
               os.Write(bytes, 0, bytes.Length);         //Send it
           }
           catch (WebException ex)
           {
               log.Error("HTTP Response error", ex);
           }
           finally
           {
               if (os != null)
               {
                   os.Close();
               }
           }

           StreamReader sr = null;
           string result = null;
           try
           { // get the response
               WebResponse webResponse = webRequest.GetResponse();
               if (webResponse == null)
               { return null; }
               sr = new StreamReader(webResponse.GetResponseStream());
               result = sr.ReadToEnd().Trim();
           }
           catch (WebException ex)
           {
               log.Error("HTTP Response error", ex);
           }
           finally
           {
               if (sr != null)
                   sr.Close();
           }
           return result;
       }
    }
}
