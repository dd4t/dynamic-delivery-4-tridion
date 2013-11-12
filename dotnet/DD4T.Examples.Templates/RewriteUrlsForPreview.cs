using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Text.RegularExpressions;
using Tridion.ContentManager.CommunicationManagement;
using Tridion.ContentManager.Templating;
using Tridion.ContentManager.ContentManagement;
using Tridion.ContentManager.Templating.Assembly;
using Tridion.ContentManager.ContentManagement.Fields;
using Tridion.ContentManager.Publishing;
using DD4T.Templates.Base.Utils;
using DD4T.Examples.Templates.ExtensionMethods;

namespace DD4T.Examples.Templating
{
    [TcmTemplateTitle("Rewrite URLs for preview")]
    public class RewriteUrlsForPreview : ExtendibleTemplate
    {
        private readonly Regex _reCss = new Regex("( |:)url\\(/([^\\)]*)\\)");
        private readonly Regex _reRegular = new Regex(" (src|href)=\"/([^\"]*)\"");
        private readonly Regex _rePreview = new Regex(" (src|href)=\"(/Preview|http)");
        private readonly Regex _rePreviewReverse = new Regex(" \\!(src|href)\\!=\"(/Preview|http)");
        private Engine engine;
        private Package package;
        private string stagingUrl = null;
        
        private string StagingUrl
        {
            get
            {
                if (stagingUrl == null)
                {
                    stagingUrl = TridionConfigurationManager.GetInstance(engine, package).AppSettings["StagingBaseUrl"];
                    if (!stagingUrl.EndsWith("/"))
                        stagingUrl = stagingUrl + "/";
                }
                return stagingUrl;
            }

        }

        public override void Transform(Engine engine, Package package)
        {
            this.engine = engine;
            this.package = package;

            // do NOT execute this logic when we are actually publishing!
            if (engine.RenderMode == RenderMode.Publish)
            {
                return;
            }

            this.Initialize(engine, package);
            Item item = package.GetByName("Output"); 
            string html= item.GetAsString();
            
            Logger.Info("base staging url : " + stagingUrl);
            html = _rePreview.Replace(html, " !$1!=\"$2");
            html = _reRegular.Replace(html, string.Format(" $1=\"{0}$2\"", StagingUrl));
            html = _rePreviewReverse.Replace(html, " $1=\"$2");
            html = _reCss.Replace(html, string.Format("$1url({0}$2)", StagingUrl));
            item.SetAsString(html);
            

        }
    }
}
