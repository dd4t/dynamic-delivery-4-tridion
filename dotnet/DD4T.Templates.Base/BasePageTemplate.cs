using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using Tridion.ContentManager.CommunicationManagement;
using Tridion.ContentManager.Templating;
using DD4T.Templates.Base.Builder;
using DD4T.Templates.Base.Utils;
using Dynamic = DD4T.ContentModel;
using System.Configuration;
using DD4T.Templates.Base.Serializing;

namespace DD4T.Templates.Base
{
    public abstract class BasePageTemplate : BaseTemplate
    {
        public BasePageTemplate() : base(TemplatingLogger.GetLogger(typeof(BasePageTemplate))) { }
        public BasePageTemplate(TemplatingLogger log) : base(log) { }

        public int DefaultLinkLevels = 1;
        public bool DefaultResolveWidthAndHeight = false;
        public bool DefaultPublishEmptyFields = false;
        public static string VariableNameCalledFromDynamicDelivery = "CalledFromDynamicDelivery";
        public static string VariableValueCalledFromDynamicDelivery = "true";

        /// <summary>
        /// Abstract method to be implemented by a subclass. The method takes a DynamicDelivery page and can add information to it (e.g. by searching in folders / structure groups / linked components, etc
        /// </summary>
        /// <param name="page">DynamicDelivery page</param>
        protected abstract void TransformPage(Dynamic.Page page);

        public override void Transform(Engine engine, Package package)
        {
            GeneralUtils.TimedLog("start Transform");
            Package = package;
            Engine = engine;
            ISerializerService serializerService = new XmlSerializerService();

            Dynamic.Page page;
            bool hasOutput = HasPackageValue(package, "Output");
            if (hasOutput)
            {
                String inputValue = package.GetValue("Output");
                page = (Dynamic.Page)serializerService.Deserialize<Dynamic.Page>(inputValue);
            }
            else
            {
                page = GetDynamicPage(Manager);
            }

            try
            {
                TransformPage(page);
            }
            catch (StopChainException)
            {
                GeneralUtils.TimedLog("caught stopchainexception, will not write current page back to the package");
                return;
            }

            string outputValue = serializerService.Serialize<Dynamic.Page>(page);

            if (hasOutput)
            {
                Item outputItem = package.GetByName("Output");
                package.Remove(outputItem);
                package.PushItem(Package.OutputName, package.CreateStringItem(ContentType.Xml, outputValue));
            }
            else
            {
                package.PushItem(Package.OutputName, package.CreateStringItem(ContentType.Xml, outputValue));
            }

            GeneralUtils.TimedLog("finished Transform");

        }

        public Dynamic.Page GetDynamicPage(BuildManager manager)
        {
            Item item = Package.GetByName(Package.PageName);
            if (item == null)
            {
                Log.Error("no page found (is this a component template?)");
                return null;
            }
            Page tcmPage = (Page)Engine.GetObject(item.GetAsSource().GetValue("ID"));
            int linkLevels;
            if (HasPackageValue(Package, "LinkLevels"))
            {
                linkLevels = Convert.ToInt32(Package.GetValue("LinkLevels"));
            }
            else
            {
                GeneralUtils.TimedLog("no link levels configured, using default level " + this.DefaultLinkLevels);
                linkLevels = this.DefaultLinkLevels;
            }
            bool resolveWidthAndHeight;
            if (HasPackageValue(Package, "ResolveWidthAndHeight"))
            {
                resolveWidthAndHeight = Package.GetValue("ResolveWidthAndHeight").ToLower().Equals("yes");
            }
            else
            {
                GeneralUtils.TimedLog("no ResolveWidthAndHeight configured, using default value " + this.DefaultResolveWidthAndHeight);
                resolveWidthAndHeight = this.DefaultResolveWidthAndHeight;
            }
            bool publishEmptyFields;
            if (HasPackageValue(Package, "PublishEmptyFields"))
            {
                publishEmptyFields = Package.GetValue("PublishEmptyFields").ToLower().Equals("yes");

            }
            else
            {
                GeneralUtils.TimedLog("no PublishEmptyFields configured, using default value " + this.DefaultResolveWidthAndHeight);
                publishEmptyFields = this.DefaultPublishEmptyFields;
            }

            Log.Debug("found page with title " + tcmPage.Title + " and id " + tcmPage.Id);
            Log.Debug("constructing dynamic page, links are followed to level " + linkLevels + ", width and height are " + (resolveWidthAndHeight ? "" : "not ") + "resolved");
            Dynamic.Page page = manager.BuildPage(tcmPage, Engine, linkLevels, resolveWidthAndHeight,publishEmptyFields);
            return page;
        }

        protected Page GetTcmPage()
        {
            Item item = Package.GetByName(Package.PageName);
            if (item == null)
            {
                Log.Error("no page found (is this a component template?)");
                return null;
            }
            return (Page)Engine.GetObject(item.GetAsSource().GetValue("ID"));
        }
    }
}