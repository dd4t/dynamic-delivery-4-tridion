using System.Web.Configuration;

namespace DD4T.Utilities
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
        private readonly static string webSite = WebConfigurationManager.AppSettings[ACTIVEWEBSITEKEY];
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
                return int.Parse(WebConfigurationManager.AppSettings[publicationIdKey]);
            }
        }
        #endregion
    }
}
