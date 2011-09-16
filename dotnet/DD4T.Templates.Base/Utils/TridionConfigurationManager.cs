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

        private static Dictionary<TcmUri, TridionConfigurationManager> instances = new Dictionary<TcmUri, TridionConfigurationManager>();

        public static TridionConfigurationManager GetInstance(Engine engine, Publication publication)
        {
            if (!instances.ContainsKey(publication.Id))
            {
                instances.Add(publication.Id, new TridionConfigurationManager(engine, publication));
            }
            return instances[publication.Id];
        }
        public static TridionConfigurationManager GetInstance(Engine engine, Package package)
        {
            Publication publication = GetPublication(engine, package);
            if (!instances.ContainsKey(publication.Id))
            {
                instances.Add(publication.Id, new TridionConfigurationManager(engine, publication));
            }
            return instances[publication.Id];
        }
        #endregion

        #region constructors
        private TridionConfigurationManager(Engine engine, Publication publication)
        {
            string tridionConfigPath = engine.GetConfiguration().CurrentConfiguration.FilePath;
            string tridionBaseDir = Path.GetDirectoryName(tridionConfigPath);
            nvc = new TridionNameValueCollection(publication, Path.Combine(tridionBaseDir, @"DD4T.config"));
        }
        #endregion

        #region private
        private TridionNameValueCollection nvc = null;
        private Engine engine;
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
        public TridionNameValueCollection(Publication publication, string configPath)
        {
            this.publication = publication;
            this.configurationComponents = null;
            this.guid = Guid.NewGuid();
            this.ConfigurationPath = configPath;
        }

        #endregion

        #region private
        private Publication publication;
        private IList<Component> configurationComponents;
        private Guid guid;
        private string fieldNameConfigurationComponents = null;
        private Configuration config = null;
        private bool checkedPublicationMetadata = false;
        private bool checkedConfigurationComponents = false;


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
                    configFileMap.ExeConfigFilename = ConfigurationPath;

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
                    KeyValueConfigurationElement elmt= Configuration.AppSettings.Settings["ConfigurationComponentsFieldName"];
                    this.fieldNameConfigurationComponents  = elmt == null ? DefaultConfigurationComponentsFieldName : elmt.Value;
                    return fieldNameConfigurationComponents;
                }
            }
        }
        #endregion

        #region public
        public string ConfigurationPath
        {
            get;
            set;
        }

        public new string this[string key]
        {
            get
            {

                // 1. Check if the key is already in our base NameValueCollection (which means it was retrieved earlier)
                string val = base[key];
                if (!string.IsNullOrEmpty(val))
                {
                    return val;
                }

                // 2. Check if the key can be found in the DD4T.config file
                if (Configuration != null)
                {
                    KeyValueConfigurationElement elmt = Configuration.AppSettings.Settings[key];
                    if (elmt != null)
                    {
                        string valueFromConfig = elmt.Value;
                        if (!string.IsNullOrEmpty(valueFromConfig))
                        {
                            // note: we found the setting in the App.config, return that!
                            base[key] = valueFromConfig;
                            return valueFromConfig;
                        }
                    }
                }

                // before we continue, we want to check if the publication has metadata at all, otherwise the rest of the logic is not necessary anymore
                if (publication.Metadata == null || publication.MetadataSchema == null)
                {
                    return string.Empty;
                }

                // 3. Check if the key is present in the publication metadata
                ItemFields metadataFields = null;
                if (!checkedPublicationMetadata)
                {

                    metadataFields = new ItemFields(publication.Metadata, publication.MetadataSchema);
                    bool foundIt = false;
                    foreach (ItemField field in metadataFields)
                    {
                        NameValueCollection fieldNVC = null;
                        if (TryGetValuesFromField(field, out fieldNVC))
                        {
                            base.Add(fieldNVC);
                            if (field.Name.Equals(key))
                            {
                                foundIt = true;
                            }
                        }
                    }
                    checkedPublicationMetadata = true;
                    if (foundIt)
                    {
                        return base[key];
                    }
                }

                // 4. Check if there are configuration components linked from the publication's metadata, and try to locate the key in them
                if (!checkedConfigurationComponents)
                {
                    checkedConfigurationComponents = true;
                    if (!metadataFields.Contains(this.FieldNameConfigurationComponents))
                    {
                        // publication metadata has reference to configuration components, we cannot find where the configuration components are
                        return string.Empty;
                    }

                    configurationComponents = ((ComponentLinkField)metadataFields[FieldNameConfigurationComponents]).Values;

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
                    checkedConfigurationComponents = true;
                    return base[key];
                }
                return string.Empty;
            }

        }
        #endregion
    }
}
