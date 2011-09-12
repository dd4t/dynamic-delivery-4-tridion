using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using Tridion.ContentManager.ContentManagement;
using Tridion.ContentManager.Templating;
using DD4T.Templates.Builder;
using DD4T.Templates.Utils;
using Dynamic = DD4T.ContentModel;

namespace DD4T.Templates
{
    public abstract class BaseComponentTemplate : BaseTemplate
    {
        public int DefaultLinkLevels = 1;
        public bool DefaultResolveWidthAndHeight = false;
        DateTime startTime = DateTime.Now;

        public BaseComponentTemplate() : base(TemplatingLogger.GetLogger(typeof(BaseComponentTemplate))) {}
        public BaseComponentTemplate(TemplatingLogger log) : base(log) { }

        /// <summary>
        /// Abstract method to be implemented by a subclass. The method takes a DynamicDelivery component and can add information to it (e.g. by searching in folders / structure groups / linked components, etc
        /// </summary>
        /// <param name="component">DynamicDelivery component </param>
        protected abstract void TransformComponent(Dynamic.Component component);


        public override void Transform(Engine engine, Package package)
        {
            this.Package = package;
            this.Engine = engine;
            XmlSerializer serializer;

            Dynamic.Component component;
            bool hasOutput = HasPackageValue(package, "Output");
            if (hasOutput)
            {
                GeneralUtils.TimedLog("start retrieving previous Output from package");
                String inputValue = package.GetValue("Output");
                GeneralUtils.TimedLog("start deserializing");
                TextReader tr = new StringReader(inputValue);
                GeneralUtils.TimedLog("start creating serializer");
                serializer = new XmlSerializerFactory().CreateSerializer(typeof(Dynamic.Component));
                GeneralUtils.TimedLog("finished creating serializer");
                component = (Dynamic.Component)serializer.Deserialize(tr);
                GeneralUtils.TimedLog("finished deserializing from package");
            }
            else
            {
                GeneralUtils.ResetLogTimer();
                GeneralUtils.TimedLog("Could not find 'Output' in the package");
                GeneralUtils.TimedLog("Start creating dynamic component from current component in the package");
                GeneralUtils.TimedLog("start creating serializer");
                serializer = new XmlSerializerFactory().CreateSerializer(typeof(Dynamic.Component));
                GeneralUtils.TimedLog("finished creating serializer");
                component = GetDynamicComponent(Manager);
                GeneralUtils.TimedLog("Finished creating dynamic component with title " + component.Title);
            }

            try
            {
                GeneralUtils.TimedLog("starting transformComponent");
                TransformComponent(component);
                GeneralUtils.TimedLog("finished transformComponent");
            }
            catch (StopChainException)
            {
                GeneralUtils.TimedLog("caught stopchainexception, will not write current component back to the package");
                return;
            }
            var sw = new StringWriter();
            var ms = new MemoryStream();
            XmlWriter writer = new XmlTextWriterFormattedNoDeclaration(ms, Encoding.UTF8);
            string outputValue;
            //Create our own namespaces for the output
            var ns = new XmlSerializerNamespaces();

            //Add an empty namespace and empty value
            ns.Add("", "");

            serializer.Serialize(writer, component, ns);
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

        private Dynamic.Component GetDynamicComponent(BuildManager manager)
        {
            GeneralUtils.TimedLog("start getting component from package");
            Item item = Package.GetByName(Package.ComponentName);
            GeneralUtils.TimedLog("finished getting component from package");
            if (item == null)
            {
                Log.Error("no component found (is this a page template?)");
                return null;
            }
            Component tcmComponent = (Component)Engine.GetObject(item.GetAsSource().GetValue("ID"));
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

            GeneralUtils.TimedLog("found component with title " + tcmComponent.Title + " and id " + tcmComponent.Id);
            GeneralUtils.TimedLog("constructing dynamic component, links are followed to level " + linkLevels + ", width and height are " + (resolveWidthAndHeight ? "" : "not ") + "resolved");

            GeneralUtils.TimedLog("start building dynamic component");
            Dynamic.Component component = manager.BuildComponent(tcmComponent, linkLevels, resolveWidthAndHeight);
            GeneralUtils.TimedLog("finished building dynamic component");
            return component;
        }

        protected Component GetTcmComponent()
        {
            Item item = Package.GetByName(Package.ComponentName);
            if (item == null)
            {
                Log.Error("no component found (is this a page template?)");
                return null;
            }
            return (Component)Engine.GetObject(item.GetAsSource().GetValue("ID"));
        }
    }
}