using System.Configuration;

namespace DD4T.Utils
{
    /// <summary>
    /// Tridion Helper class with usefull methods
    /// </summary>
    public static class TridionHelper
    {
        #region constants
        const string PublicationIdKey = "DD4T.Website.{0}.PublicationId";
        const string PublicationIdKeyAlt1 = "Website.{0}.PublicationId"; // keep this for backwards compatibility
        #endregion

        #region private statics
        private readonly static string webSite = ConfigurationHelper.ActiveWebsite;
        #endregion

        #region methods
        /// <summary>
        /// Reads the publication id from the web.config
        /// </summary>
        /// <returns>The current publicationId</returns>
        /// 

        public static int PublicationId
        {
            get
            {
                //TODO: Errorhandling
                return ConfigurationHelper.GetSettingAsInt(string.Format(PublicationIdKey, webSite), string.Format(PublicationIdKeyAlt1, webSite));
            }
        }
        #endregion
    }
}
