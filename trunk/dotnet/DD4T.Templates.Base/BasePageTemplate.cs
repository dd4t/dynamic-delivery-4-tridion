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

namespace DD4T.Templates.Base
{
    public abstract class BasePageTemplate : BaseTemplate
    {
        public BasePageTemplate() : base(TemplatingLogger.GetLogger(typeof(BasePageTemplate))) { }
        public BasePageTemplate(TemplatingLogger log) : base(log) { }

        public int DefaultLinkLevels = 1;
        public bool DefaultResolveWidthAndHeight = false;
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
            XmlSerializer serializer;

            Dynamic.Page page;
            bool hasOutput = HasPackageValue(package, "Output");
            if (hasOutput)
            {
                GeneralUtils.TimedLog("start retrieving previous Output from package");
                String inputValue = package.GetValue("Output");
                GeneralUtils.TimedLog("start creating serializer");
                serializer = new XmlSerializerFactory().CreateSerializer(typeof(Dynamic.Page));
                GeneralUtils.TimedLog("finished creating serializer");
                TextReader tr = new StringReader(inputValue);
                GeneralUtils.TimedLog("start deserializing from package");
                page = (Dynamic.Page)serializer.Deserialize(tr);
                GeneralUtils.TimedLog("finished deserializing from package");
            }
            else
            {
                GeneralUtils.ResetLogTimer();
                GeneralUtils.TimedLog("Could not find 'Output' in the package");
                GeneralUtils.TimedLog("Start creating dynamic page from current page in the package");
                page = GetDynamicPage(Manager);
                GeneralUtils.TimedLog("Finished creating dynamic page with title " + page.Title);
                GeneralUtils.TimedLog("start creating serializer");
                serializer = new XmlSerializerFactory().CreateSerializer(typeof(Dynamic.Page));
                GeneralUtils.TimedLog("finished creating serializer");
            }

            try
            {
                GeneralUtils.TimedLog("starting transformPage");
                TransformPage(page);
                GeneralUtils.TimedLog("finished transformPage");
            }
            catch (StopChainException)
            {
                GeneralUtils.TimedLog("caught stopchainexception, will not write current page back to the package");
                return;
            }
            StringWriter sw = new StringWriter();
            MemoryStream ms = new MemoryStream();
            XmlWriter writer = new XmlTextWriterFormattedNoDeclaration(ms, Encoding.UTF8);
            string outputValue;
            //Create our own namespaces for the output
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();

            //Add an empty namespace and empty value
            ns.Add("", "");

            serializer.Serialize(writer, page, ns);
            outputValue = Encoding.UTF8.GetString(ms.ToArray());

            // for some reason, the .NET serializer leaves an invalid character at the start of the string
            // we will remove everything up to the first < so that the XML can be deserialized later!
            Regex re = new Regex("^[^<]+");
            outputValue = re.Replace(outputValue, "");

            if (hasOutput)
            {
                Item outputItem = package.GetByName("Output");
                package.Remove(outputItem);
                outputItem.SetAsString(outputValue);
                package.PushItem("Output", outputItem);
            }
            else
            {
                package.PushItem(Package.OutputName, package.CreateStringItem(ContentType.Text, outputValue));
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
            Log.Debug("found page with title " + tcmPage.Title + " and id " + tcmPage.Id);
            Log.Debug("constructing dynamic page, links are followed to level " + linkLevels + ", width and height are " + (resolveWidthAndHeight ? "" : "not ") + "resolved");
            return manager.BuildPage(tcmPage, Engine, linkLevels, resolveWidthAndHeight);
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