using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using DD4T.ContentModel.Contracts.Providers;

namespace DD4T.Utils
{
    public static class ConfigurationKeys
    {

        // the Alt1, Alt2 versions are there to make the system backwards compatible
        // note that the associated properties in the ConfigurationHelper must be changed as well
        public const string ProviderVersion = "DD4T.ProviderVersion";
        public const string LoggerClass = "DD4T.LoggerClass";
        public const string IncludeLastPublishedDate = "DD4T.IncludeLastPublishedDate";
        public const string BinaryHandlerCacheExpiration = "DD4T.BinaryHandlerCacheExpiration";
        public const string BinaryHandlerCacheExpirationAlt1 = "BinaryHandlerCaching";
        public const string BinaryFileExtensions = "DD4T.BinaryFileExtensions";
        public const string BinaryFileExtensionsAlt1 = "BinaryFileExtensions";
        public const string ComponentPresentationController = "DD4T.ComponentPresentationController";
        public const string ComponentPresentationControllerAlt1 = "Controller";
        public const string ComponentPresentationAction = "DD4T.ComponentPresentationAction";
        public const string ComponentPresentationActionAlt1 = "Action";
        public const string SitemapPath = "DD4T.SitemapPath";
        public const string SitemapPathAlt1 = "SitemapPath";
        public const string SelectComponentByComponentTemplateId = "ComponentFactory.ComponentTemplateId";
        public const string SelectComponentByComponentTemplateIdAlt1 = "DD4T.SelectComponentByComponentTemplateId";
        public const string SelectComponentByOutputFormat = "ComponentFactory.OutputFormat";
        public const string SelectComponentByOutputFormatAlt1 = "DD4T.SelectComponentByOutputFormat";
        public const string ActiveWebsite = "DD4T.Site.ActiveWebSite";
        public const string ActiveWebsiteAlt1 = "Site.ActiveWebSite";
        public const string DefaultPage = "DD4T.DefaultPage";
        public const string DefaultPageAlt1 = "DefaultPage";
        public const string ShowAnchors = "DD4T.ShowAnchors";
        public const string LinkToAnchor = "DD4T.LinkToAnchor";
        public const string UseUriAsAnchor = "DD4T.UseUriAsAnchor";
        public const string PublicationId = "DD4T.PublicationId";
    }

    public static class ConfigurationHelper
    {

        public static string GetSetting(params string[] key)
        {
            return SafeGetConfigSettingAsString(key);
        }

        public static int GetSettingAsInt(params string[] key)
        {
            return SafeGetConfigSettingAsInt(key);
        }

        public static string ComponentPresentationController
        {
            get
            {
                return SafeGetConfigSettingAsString(ConfigurationKeys.ComponentPresentationController, ConfigurationKeys.ComponentPresentationControllerAlt1);
            }
        }

        public static string ComponentPresentationAction
        {
            get
            {
                return SafeGetConfigSettingAsString(ConfigurationKeys.ComponentPresentationAction, ConfigurationKeys.ComponentPresentationActionAlt1);
            }
        }

        public static string ActiveWebsite
        {
            get
            {
                return SafeGetConfigSettingAsString(ConfigurationKeys.ActiveWebsite, ConfigurationKeys.ActiveWebsiteAlt1);
            }
        }

        public static string DefaultPage
        {
            get
            {
                return SafeGetConfigSettingAsString(ConfigurationKeys.DefaultPage, ConfigurationKeys.DefaultPageAlt1);
            }
        }

        public static string SelectComponentByComponentTemplateId
        {
            get
            {
                return SafeGetConfigSettingAsString(ConfigurationKeys.SelectComponentByComponentTemplateId, ConfigurationKeys.SelectComponentByComponentTemplateIdAlt1);
            }
        }

        public static string SelectComponentByOutputFormat
        {
            get
            {
                return SafeGetConfigSettingAsString(ConfigurationKeys.SelectComponentByOutputFormat, ConfigurationKeys.SelectComponentByOutputFormatAlt1);
            }
        }

        public static string SiteMapPath
        {
            get
            {
                return SafeGetConfigSettingAsString(ConfigurationKeys.SitemapPath, ConfigurationKeys.SitemapPathAlt1);
            }
        }

        public static int BinaryHandlerCacheExpiration
        {
            get
            {
                return SafeGetConfigSettingAsInt(ConfigurationKeys.BinaryHandlerCacheExpiration, ConfigurationKeys.BinaryHandlerCacheExpirationAlt1);
            }
        }

        public static int PublicationId
        {
            get
            {
                return SafeGetConfigSettingAsInt(ConfigurationKeys.PublicationId);
            }
        }

        public static string BinaryFileExtensions
        {
            get
            {
                return SafeGetConfigSettingAsString(ConfigurationKeys.BinaryFileExtensions, ConfigurationKeys.BinaryFileExtensionsAlt1);
            }
        }

        public static string LoggerClass
        {
            get
            {
                return SafeGetConfigSettingAsString(ConfigurationKeys.LoggerClass);
            }
        }

        public static bool IncludeLastPublishedDate
        {
            get
            {
                return SafeGetConfigSettingAsBoolean(ConfigurationKeys.IncludeLastPublishedDate);
            }
        }

        public static bool ShowAnchors
        {
            get
            {
                return SafeGetConfigSettingAsBoolean(ConfigurationKeys.ShowAnchors);
            }
        }

        public static bool LinkToAnchor
        {
            get
            {
                return SafeGetConfigSettingAsBoolean(ConfigurationKeys.LinkToAnchor);
            }
        }

        public static bool UseUriAsAnchor
        {
            get
            {
                return SafeGetConfigSettingAsBoolean(ConfigurationKeys.UseUriAsAnchor);
            }
        }

        public static ProviderVersion ProviderVersion
        {
            get
            {
                string version = SafeGetConfigSettingAsString(ConfigurationKeys.ProviderVersion);
                if (string.IsNullOrEmpty(version))
                    return ContentModel.Contracts.Providers.ProviderVersion.Undefined;
                try
                {
                    return (ProviderVersion)Enum.Parse(typeof(ProviderVersion), version);
                }
                catch (Exception e)
                {
                    LoggerService.Warning("invalid provider version {0}", version);
                    return ProviderVersion.Undefined;
                }
            }
        }
        private static int SafeGetConfigSettingAsInt(params string[] keys)
        {
            string setting = SafeGetConfigSettingAsString(keys);
            if (string.IsNullOrEmpty(setting))
                return int.MinValue;
            int i = int.MinValue;
            Int32.TryParse(setting, out i);
            return i;
        }

        private static bool SafeGetConfigSettingAsBoolean(params string[] keys)
        {
            string setting = SafeGetConfigSettingAsString(keys);
            if (string.IsNullOrEmpty(setting))
                return false;
            bool b = false;
            Boolean.TryParse(setting, out b);
            return b;
        }

        private static string SafeGetConfigSettingAsString(params string[] keys)
        {
            foreach (string key in keys)
            {
                string setting = ConfigurationManager.AppSettings[key];
                if (! string.IsNullOrEmpty(setting))
                    return setting;
            }
            return string.Empty;
        }
    }

}
