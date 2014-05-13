using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Configuration;
using Tridion.ContentManager.Templating.Assembly;
using Tridion.ContentManager.Templating;
using DD4T.Templates.Base.Builder;
using DD4T.ContentModel.Contracts.Serializing;
using DD4T.Serialization;

namespace DD4T.Templates.Base
{
    public abstract class BaseTemplate : ITemplate
    {
        public BaseTemplate()
        {
            Log = TemplatingLogger.GetLogger(typeof(BaseTemplate));
        }

        public BaseTemplate(TemplatingLogger log)
        {
            Log = log;
        }

        protected TemplatingLogger Log
        {
            get;
            set;
        }
        private ISerializerService _serializerService = null;
        protected ISerializerService SerializerService
        {
            get
            {
                if (_serializerService == null)
                {
                    _serializerService = FindBestService();
                }
                return _serializerService;
            }
        }
        private ISerializerService FindBestService()
        {
            ISerializerService s;
                    if (Manager.BuildProperties.SerializationFormat == SerializationFormats.JSON)
                    {
                        s = new JSONSerializerService();
                        if (s.IsAvailable())
                            return s;
                    }
                    if (Manager.BuildProperties.SerializationFormat == SerializationFormats.XML)
                    {
                        s = new XmlSerializerService();
                        if (s.IsAvailable())
                            return s;
                    }
            // service for the configured serialization format is unavailable, pick one of the available services
                    s = new JSONSerializerService();
                    if (s.IsAvailable())
                        return s;
                    s = new XmlSerializerService();
                    if (s.IsAvailable())
                        return s;

                    throw new Exception("Unsupported serialization format: " + Manager.BuildProperties.SerializationFormat);

        }

        protected Package Package { get; set; }
        protected Engine Engine { get; set; }
        private BuildManager _buildManager = null;

        public BuildManager Manager
        {
            get 
            {
                if (_buildManager == null)
                {
                    _buildManager = new BuildManager(Package);
                    _buildManager.SerializerService = SerializerService;
                }
                return _buildManager; }
            set 
            { 
                _buildManager = value; 
            }
        }

        public abstract void Transform(Engine engine, Package package);

        protected bool HasPackageValue(Package package, string key)
        {
            foreach (KeyValuePair<string, Item> kvp in package.GetEntries())
            {
                if (kvp.Key.Equals(key))
                {
                    return true;
                }
            }
            return false;
        }

        public static string DD4TContextVariableKey = "RenderedByDD4T";
    }


}
