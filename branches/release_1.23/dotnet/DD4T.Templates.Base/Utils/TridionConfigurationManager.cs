using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using Tridion.ContentManager;
using Tridion.ContentManager.CommunicationManagement;
using Tridion.ContentManager.ContentManagement;
using Tridion.ContentManager.ContentManagement.Fields;
using Tridion.ContentManager.Templating;

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
        #region public static

        public static TridionConfigurationManager GetInstance(Publication publication)
        {
            log.Debug(">>GetInstance called for " + publication.Id);
            if (!Instances.ContainsKey(publication.Id))
            {
                log.Debug("no instance found, creating new one");
                AddInstance(publication);
            }
            else
            {
                log.Debug(string.Format("found instance, publication revision date {0}, config file modification date {1}, cache date {2}", publication.RevisionDate, ConfigurationModificationDate, InstanceCacheDates[publication.Id]));
                if (InstanceCacheDates[publication.Id].CompareTo(publication.RevisionDate) < 0 || InstanceCacheDatesFS[publication.Id].CompareTo(ConfigurationModificationDate) < 0)
                {
                    log.Debug("creating new instance");
                    RemoveInstance(publication);
                    AddInstance(publication);
                }
            }
            return Instances[publication.Id];
        }

        [Obsolete("Please use GetInstance(Publication) instead")]
        public static TridionConfigurationManager GetInstance(Engine engine, Publication publication)
        {
            return GetInstance(publication);
        }

        public static TridionConfigurationManager GetInstance(Engine engine, Package package)
        {
            Publication publication = GetPublication(engine, package);
            return GetInstance(publication);
        }

        
        #endregion

        #region private static
        protected static TemplatingLogger log = TemplatingLogger.GetLogger(typeof(TridionConfigurationManager));
        private static readonly Dictionary<TcmUri, TridionConfigurationManager> Instances = new Dictionary<TcmUri, TridionConfigurationManager>();
        private static readonly Dictionary<TcmUri, DateTime> InstanceCacheDates = new Dictionary<TcmUri, DateTime>();
        private static readonly Dictionary<TcmUri, DateTime> InstanceCacheDatesFS = new Dictionary<TcmUri, DateTime>();
        private static void AddInstance(Publication publication)
        {
            Instances.Add(publication.Id, new TridionConfigurationManager(publication));
            InstanceCacheDates.Add(publication.Id, publication.RevisionDate);
            InstanceCacheDatesFS.Add(publication.Id, ConfigurationModificationDate);
        }
        private static void RemoveInstance(Publication publication)
        {
            Instances.Remove(publication.Id);
            InstanceCacheDates.Remove(publication.Id);
            InstanceCacheDatesFS.Remove(publication.Id);
        }
        private static DateTime ConfigurationModificationDate
        {
            get
            {
                FileInfo fi = new FileInfo(ConfigurationFilePath);
                return fi.LastWriteTime;
            }
        }
        private static string ConfigurationFilePath
        {
            get
            {
                string tridionConfigPath = string.Format("{0}\\config\\", Tridion.ContentManager.ConfigurationSettings.GetTcmHomeDirectory());
                string tridionBaseDir = Path.GetDirectoryName(tridionConfigPath);
                return Path.Combine(tridionBaseDir, @"DD4T.config");
            }
        }

        #endregion

        #region constructors
        private TridionConfigurationManager(Publication publication)
        {
            _nvc = new TridionNameValueCollection(publication, ConfigurationFilePath);
        }
        
        //private TridionConfigurationManager(Engine engine, Publication publication)
        //{
        //    string tridionConfigPath = engine.GetConfiguration().CurrentConfiguration.FilePath;
        //    string tridionBaseDir = Path.GetDirectoryName(tridionConfigPath);
        //    _nvc = new TridionNameValueCollection(publication, Path.Combine(tridionBaseDir, @"DD4T.config"));
        //}
        #endregion

        #region private
        private readonly TridionNameValueCollection _nvc = null;
        //private Engine _engine;
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
                return _nvc;
            }
        }
        #endregion
    }

    public class TridionNameValueCollection : Hashtable
    {
        #region static
        public static string DefaultConfigurationComponentsFieldName = "ConfigurationComponents";
        #endregion

        #region constructors
        public TridionNameValueCollection(Publication publication, string configPath)
        {
            _publication = publication;
            _configurationComponents = null;
            _guid = Guid.NewGuid();
            this.ConfigurationPath = configPath;
        }
        #endregion

        #region private
        private readonly Publication _publication;
        private IList<Component> _configurationComponents;
        private Guid _guid;
        private string _fieldNameConfigurationComponents = null;
        private Configuration _config = null;
        private bool _checkedPublicationMetadata = false;
        private bool _checkedConfigurationComponents = false;

        private static bool TryGetValueFromField(ItemField field, out string value)
        {
            value = null;
            if (field is TextField)
            {
                value = ((TextField)field).Value;
                return true;
            }
            return false;
        }

        //private bool TryGetValuesFromField(ItemField field, out NameValueCollection nvc)
        //{
        //    nvc = new NameValueCollection();
        //    if (field is TextField)
        //    {
        //        foreach (string v in ((TextField)field).Values)
        //        {
        //            nvc.Add(field.Name, v);
        //        }
        //        return true;
        //    }
        //    if (field is NumberField)
        //    {
        //        foreach (int i in ((NumberField)field).Values)
        //        {
        //            nvc.Add(field.Name, Convert.ToString(i));
        //        }
        //        return true;
        //    }
        //    if (field is DateField)
        //    {
        //        foreach (DateTime dt in ((DateField)field).Values)
        //        {
        //            nvc.Add(field.Name, dt.ToString());
        //        }
        //        return true;
        //    }
        //    if (field is ComponentLinkField)
        //    {
        //        foreach (Component c in ((ComponentLinkField)field).Values)
        //        {
        //            nvc.Add(field.Name, c.Id.ToString());
        //        }
        //        return true;
        //    }

        //    return false;
        //}

        private Configuration Configuration
        {
            get
            {
                if (_config == null)
                {
                    ExeConfigurationFileMap configFileMap = new ExeConfigurationFileMap {ExeConfigFilename = ConfigurationPath};

                    // Get the mapped configuration file              
                    _config = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);
                }
                return _config;
            }
        }

        private string FieldNameConfigurationComponents
        {
            get
            {
                if (_fieldNameConfigurationComponents != null)
                {
                    return _fieldNameConfigurationComponents;
                }
                if (Configuration != null)
                {
                    KeyValueConfigurationElement elmt= Configuration.AppSettings.Settings["ConfigurationComponentsFieldName"];
                    _fieldNameConfigurationComponents  = elmt == null ? DefaultConfigurationComponentsFieldName : elmt.Value;
                    return _fieldNameConfigurationComponents;
                }
                return DefaultConfigurationComponentsFieldName;
            }
        }
        #endregion

        #region public
        public string ConfigurationPath
        {
            get;
            set;
        }

        public string this[string key]
        {
            get
            {

                // 1. Check if the key is already in our base Hashtable (which means it was retrieved earlier)
                string val = base[key] as string;
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
                if (_publication.Metadata == null || _publication.MetadataSchema == null)
                {
                    return string.Empty;
                }

                // 3. Check if the key is present in the publication metadata
                ItemFields metadataFields = null;
                if (!_checkedPublicationMetadata)
                {

                    metadataFields = new ItemFields(_publication.Metadata, _publication.MetadataSchema);
                    bool foundIt = false;
                    foreach (ItemField field in metadataFields)
                    {
                        if (!base.ContainsKey(field.Name))
                        {
                            string v = null;
                            if (TryGetValueFromField(field, out v))
                            {
                                base[field.Name] = v;
                                if (field.Name.Equals(key))
                                {
                                    foundIt = true;
                                }
                            }
                        }
                    }
                    _checkedPublicationMetadata = true;
                    if (foundIt)
                    {
                        return base[key] as string;
                    }
                }

                // 4. Check if there are configuration components linked from the publication's metadata, and try to locate the key in them
                if (!_checkedConfigurationComponents)
                {
                    _checkedConfigurationComponents = true;
                    if (!metadataFields.Contains(this.FieldNameConfigurationComponents))
                    {
                        // publication metadata has reference to configuration components, we cannot find where the configuration components are
                        return string.Empty;
                    }

                    _configurationComponents = ((ComponentLinkField)metadataFields[FieldNameConfigurationComponents]).Values;

                    foreach (Component c in _configurationComponents)
                    {
                        ItemFields fields = new ItemFields(c.Content, c.Schema);
                        foreach (ItemField field in fields)
                        {
                            if (!base.ContainsKey(field.Name))
                            {
                                string v = null;
                                if (TryGetValueFromField(field, out v))
                                {
                                    base[field.Name] = v;
                                }
                            }
                        }
                    }
                    _checkedConfigurationComponents = true;
                    return base[key] as string;
                }
                return string.Empty;
            }

        }
        #endregion
    }
}
