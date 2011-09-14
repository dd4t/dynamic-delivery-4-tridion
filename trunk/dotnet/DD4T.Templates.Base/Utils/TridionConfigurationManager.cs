using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Configuration;
using Tridion.ContentManager.ContentManagement;
using Tridion.ContentManager.Publishing.Rendering;
using Tridion.ContentManager.CommunicationManagement;
using Tridion.ContentManager.ContentManagement.Fields;
using Tridion.ContentManager;
using Tridion.ContentManager.Templating;
using System.IO;

namespace DD4T.Templates.Base.Utils
{
    /// <summary>
    /// Offers access to template configuration. The requested setting is searched in the following order:
    /// 1. AppSettings section in DD4T.Templates.Base.dll.config
    /// 2. Metadata field of the component
    /// 3. Field in a configuration component.
    /// 
    /// Configuration components are components that are linked from a metadata field on the current publication. 
    /// The name of field linking to the configuration component(s), defaults to "ConfigurationComponents",
    /// but it can be overridden by including an AppSettings key in DD4T.Templates.Base.dll.config with the 
    /// name "ConfigurationComponentsFieldName".
    /// </summary>
    public class TridionConfigurationManager
    {
        #region static
        public static string ConfigFilePath
        {
            get
            {
                Uri p = new Uri(System.Reflection.Assembly.GetEntryAssembly().CodeBase);
                string tridionBaseDir = Path.GetDirectoryName(p.LocalPath);
                return Path.Combine(tridionBaseDir, @"..\config\DD4T.config");
            }
        }

        private static Dictionary<TcmUri, TridionConfigurationManager> instances = new Dictionary<TcmUri, TridionConfigurationManager>();

        public static TridionConfigurationManager GetInstance(Publication publication)
        {
            if (!instances.ContainsKey(publication.Id))
            {
                instances.Add(publication.Id, new TridionConfigurationManager(publication));
            }
            return instances[publication.Id];
        }
        public static TridionConfigurationManager GetInstance(Engine engine, Package package)
        {
            Publication publication = GetPublication(engine, package);
            if (!instances.ContainsKey(publication.Id))
            {
                instances.Add(publication.Id, new TridionConfigurationManager(publication));
            }
            return instances[publication.Id];
        }
        #endregion

        #region constructors
        private TridionConfigurationManager(Publication publication)
        {
            nvc = new TridionNameValueCollection(publication);
        }
        #endregion

        #region private
        private TridionNameValueCollection nvc = null;

        private static Publication GetPublication(Engine engine, Package package)
        {
            RepositoryLocalObject pubItem = null;
            Repository repository = null;

            if (package.GetByType(ContentType.Page) != null)
                pubItem = GetPage(engine, package);
            else
                pubItem = GetComponent(engine, package);

            if (pubItem != null) repository = pubItem.ContextRepository;
            return repository as Publication;
        }

        private static Component GetComponent(Engine engine, Package package)
        {
            Item component = package.GetByType(ContentType.Component);
            return (Component)engine.GetObject(component.GetAsSource().GetValue("ID"));
        }

        private static Page GetPage(Engine engine, Package package)
        {
            Item pageItem = package.GetByType(ContentType.Page);
            if (pageItem != null)
            {
                return engine.GetObject(pageItem.GetAsSource().GetValue("ID")) as Page;
            }

            Page page = engine.PublishingContext.RenderContext.ContextItem as Page;
            return page;
        }

        #endregion

        #region public
        // Summary:
        //     Gets application settings from Tridion
        //
        // Returns:
        //     A System.Collections.Specialized.NameValueCollection object that contains
        //     the settings....
        //
        // Exceptions:
        //   System.Configuration.ConfigurationErrorsException:
        //     A valid System.Collections.Specialized.NameValueCollection object could not
        //     be retrieved with the application settings data.
        public TridionNameValueCollection AppSettings
        {
            get
            {
                return nvc;
            }
        }

        #endregion
    }

    public class TridionNameValueCollection : NameValueCollection
    {
        #region static
        public static string DefaultConfigurationComponentsFieldName = "ConfigurationComponents";
        #endregion

        #region constructors
        public TridionNameValueCollection(Publication publication)
        {
            this.publication = publication;
            this.configurationComponents = null;
            this.guid = Guid.NewGuid();
        }

        #endregion

        #region private
        private Publication publication;
        private IList<Component> configurationComponents;
        private Guid guid;
        private string fieldNameConfigurationComponents = null;
        private Configuration config = null;

        private bool TryGetValueFromField(ItemField field, out string value)
        {
            value = null;
            if (field is TextField)
            {
                value = ((TextField)field).Value;
                return true;
            }
            return false;
        }
        private bool TryGetValuesFromField(ItemField field, out NameValueCollection nvc)
        {
            nvc = new NameValueCollection();
            if (field is TextField)
            {
                foreach (string v in ((TextField)field).Values)
                {
                    nvc.Add(field.Name, v);
                }
                return true;
            }
            if (field is NumberField)
            {
                foreach (int i in ((NumberField)field).Values)
                {
                    nvc.Add(field.Name, Convert.ToString(i));
                }
                return true;
            }
            if (field is DateField)
            {
                foreach (DateTime dt in ((DateField)field).Values)
                {
                    nvc.Add(field.Name, dt.ToString());
                }
                return true;
            }
            if (field is ComponentLinkField)
            {
                foreach (Component c in ((ComponentLinkField)field).Values)
                {
                    nvc.Add(field.Name, c.Id.ToString());
                }
                return true;
            }

            return false;
        }

        private Configuration Configuration
        {
            get
            {
                if (config == null)
                {
                    ExeConfigurationFileMap configFileMap = new ExeConfigurationFileMap();
                    configFileMap.ExeConfigFilename = TridionConfigurationManager.ConfigFilePath;

                    // Get the mapped configuration file              
                    config = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);
                }
                return config;
            }
        }

        private string FieldNameConfigurationComponents
        {
            get
            {
                if (this.fieldNameConfigurationComponents != null)
                {
                    return this.fieldNameConfigurationComponents;
                }
                if (Configuration != null)
                {
                    string fieldName = Configuration.AppSettings.Settings["ConfigurationComponentsFieldName"].Value;
                    if (!string.IsNullOrEmpty(fieldName))
                    {
                        this.fieldNameConfigurationComponents = fieldName;
                        return fieldName;
                    }
                }
                this.fieldNameConfigurationComponents = DefaultConfigurationComponentsFieldName;
                return DefaultConfigurationComponentsFieldName;
            }
        }
        #endregion

        #region public
        public new string this[string key]
        {
            get
            {
                
                string val = base[key];
                if (!string.IsNullOrEmpty(val))
                {
                    return val;
                }

                if (Configuration != null)
                {
                    string valueFromConfig = Configuration.AppSettings.Settings[key].Value;
                    if (!string.IsNullOrEmpty(valueFromConfig))
                    {
                        // note: we found the setting in the App.config, return that!
                        return valueFromConfig;
                    }
                }


                if (configurationComponents == null)
                {
                    if (publication.Metadata == null || publication.MetadataSchema == null)
                    {
                        // publication has no metadata, we cannot find where the configuration components are
                        configurationComponents = new List<Component>(); // subsequent calls will return "" directly!
                        return string.Empty;
                    }


                    ItemFields metadataFields = new ItemFields(publication.Metadata, publication.MetadataSchema);

                    if (metadataFields.Contains(key))
                    {
                        string v = null;
                        if (TryGetValueFromField(metadataFields[key], out v))
                        {
                            return v;
                        }
                    }


                    if (!metadataFields.Contains(this.FieldNameConfigurationComponents))
                    {
                        // publication metadata has reference to configuration components, we cannot find where the configuration components are
                        configurationComponents = new List<Component>(); // subsequent calls will return "" directly!
                        return string.Empty;
                    }
                    configurationComponents = ((ComponentLinkField)metadataFields[FieldNameConfigurationComponents]).Values;
                }

                foreach (Component c in configurationComponents)
                {
                    
                    ItemFields fields = new ItemFields(c.Content, c.Schema);
                    foreach (ItemField field in fields)
                    {
                        NameValueCollection fieldNVC = null;
                        if (TryGetValuesFromField(field, out fieldNVC))
                        {
                            base.Add(fieldNVC);
                        }
                    }
                }
                return base[key];
            }

        }
        #endregion
    }
}
