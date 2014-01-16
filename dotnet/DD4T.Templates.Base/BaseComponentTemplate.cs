using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using Tridion.ContentManager.ContentManagement;
using Tridion.ContentManager.Templating;
using DD4T.Templates.Base.Builder;
using DD4T.Templates.Base.Utils;
using Dynamic = DD4T.ContentModel;
using DD4T.Templates.Base.Serializing;

namespace DD4T.Templates.Base
{
    public abstract class BaseComponentTemplate : BaseTemplate
    {
        public int DefaultLinkLevels = 1;
        public bool DefaultResolveWidthAndHeight = false;
        public bool DefaultPublishEmptyFields = false;
        DateTime startTime = DateTime.Now;

        public BaseComponentTemplate() : base(TemplatingLogger.GetLogger(typeof(BaseComponentTemplate))) { }
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
            ISerializerService serializerService = new JSONSerializerService();

            Dynamic.Component component;
            bool hasOutput = HasPackageValue(package, "Output");
            if (hasOutput)
            {
                String inputValue = package.GetValue("Output");
                component = (Dynamic.Component)serializerService.Deserialize<Dynamic.Component>(inputValue);
            }
            else
            {
                component = GetDynamicComponent(Manager);
            }

            try
            {
                TransformComponent(component);
            }
            catch (StopChainException)
            {
                GeneralUtils.TimedLog("caught stopchainexception, will not write current component back to the package");
                return;
            }

            string outputValue = serializerService.Serialize<Dynamic.Component>(component);

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

            GeneralUtils.TimedLog("found component with title " + tcmComponent.Title + " and id " + tcmComponent.Id);
            GeneralUtils.TimedLog("constructing dynamic component, links are followed to level " + linkLevels + ", width and height are " + (resolveWidthAndHeight ? "" : "not ") + "resolved");

            GeneralUtils.TimedLog("start building dynamic component");
            Dynamic.Component component = manager.BuildComponent(tcmComponent, linkLevels, resolveWidthAndHeight,publishEmptyFields);
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