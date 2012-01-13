using System.Configuration;

namespace DD4T.Utils
{
    /// <summary>
    /// Tridion Helper class with usefull methods
    /// </summary>
    public static class TridionHelper
    {
        #region constants
        const string ACTIVEWEBSITEKEY = "Site.ActiveWebSite";
        const string PUBLICATIONIDKEY = "Website.{0}.PublicationId";
        #endregion

        #region private statics
        private readonly static string webSite = ConfigurationManager.AppSettings[ACTIVEWEBSITEKEY];
        private readonly static string publicationIdKey = string.Format(PUBLICATIONIDKEY, webSite);
        #endregion

        #region methods
        /// <summary>
        /// Reads the publication id from the web.config
        /// </summary>
        /// <returns>The current publicationId</returns>
        public static int PublicationId
        {
            get
            {
                //TODO: Errorhandling
                return int.Parse(ConfigurationManager.AppSettings[publicationIdKey]);
            }
        }
        #endregion
    }
}
