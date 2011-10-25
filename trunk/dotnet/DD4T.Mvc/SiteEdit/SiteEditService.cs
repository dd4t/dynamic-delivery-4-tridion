using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DD4T.ContentModel;



namespace DD4T.Mvc.SiteEdit
{
    /**
     * 
     * The class, with static methods only, serves views and controllers with SiteEdit tags. The point
     * of bundling them is to keep all SiteEdit behaviour in a single point, and hence its strange JSON
     * codes accessible to non-Tridion specialized developers. 
     *
     * @author Rogier Oudshoorn, .Net port by Quirijn Slings
     */
    public class SiteEditService
    {

        public static SiteEditSettings SiteEditSettings = new SiteEditSettings();

        /**
         * string Format used to create the Page-level SiteEdit tags.
         * 
         */
        public static string PAGE_SE_Format =
            "<!-- SiteEdit Settings: {{" +
                "\"PageID\":\"{0}\", " +    // page id
                "\"PageVersion\":{1}, " +   // page version
                "\"ComponentPresentationLocation\":1, " +     // add components to bottom of list, rather then front of list
                "\"BluePrinting\" : {{" +
                    "\"PageContext\" : \"tcm:0-{2}-1\", " + // point to publication where pages must be created
                    "\"ComponentContext\" : \"tcm:0-{3}-1\", " + // point to publication where components must be created                
                    "\"PublishContext\" : \"tcm:0-{4}-1\" " + // point to publication where page is published                
                "}}" +
            "}} -->";

        /**
     * string Format representing a Component-level SiteEdit tag.
     * 
     */
        public static string COMPONENT_SE_Format =
            "<!-- Start SiteEdit Component Presentation: {{" +
                "\"ID\" : \"CP{0}\", " + // unique id
                "\"ComponentID\" : \"{1}\", " + // comp id
                "\"ComponentTemplateID\" : \"{2}\", " + // ct id
                "\"ComponentVersion\" : {3}, " + // comp version
                "\"IsQueryBased\" : false, " + // query will be true for lists; out of scope for now
                "\"SwapLabel\" : \"{4}\" " + // label with which components can be swapped; so region
            "}} -->";

        /**
         * string Format representing a simple, non-multivalue SiteEdit field marking.
         */
        public static string FIELD_SE_Format =
            "<!-- Start SiteEdit Component Field: {{" +
                "\"ID\" : \"{0}\", " +         // id (name) of the field
                "\"IsMultiValued\" : {1}, " + // multivalue?
                "\"XPath\" : \"{2}\" " +       // xpath of the field
            "}} -->";




        /**
         * Support function, checking if SE is enabled for the given item ID.
         * 
         * @param tcmPubId An ID of a Tridion item
         * 
         * @return whether the item belongs to a publication that has active SE in this web application
         */
        public static bool IsSiteEditEnabled(IRepositoryLocal item)
        {
            string pubIdWithoutTcm = Convert.ToString(new TcmUri(item.Id).PublicationId);

            try
            {
                return SiteEditSettings.ContainsKey(pubIdWithoutTcm);
            }
            catch (Exception ex)
            {
// todo: add logging (log.Error("Unable to get pubID from URI", ex))
                return false;
            }
        }

        /**
         * Generates SiteEdit tag for given page.
         * 
         * @param page Page the tag belongs to.
         * @return string representing the JSON SiteEdit tag..
         */
        public static string GenerateSiteEditPageTag(IPage page)
        {
            string pubIdWithoutTcm = Convert.ToString(new TcmUri(page.Id).PublicationId);

            //try
            //{
            if (SiteEditSettings.ContainsKey(pubIdWithoutTcm))
                {
                    SiteEditSetting setting = SiteEditSettings[pubIdWithoutTcm];
                    string usePageContext = string.IsNullOrEmpty(setting.PagePublication) ? page.OwningPublication.Id : setting.PagePublication;
                    TcmUri pageContextUri = new TcmUri(usePageContext);
                    if (setting.Enabled)
                    {
                        return string.Format(PAGE_SE_Format, page.Id, Convert.ToString(page.Version), pageContextUri.ItemId, setting.ComponentPublication, setting.PublishPublication);
                    }
                }
                throw new Exception("Cannot create siteEdit pagetag, publication is not configured to be editable.");
            //}
            //catch (Exception ex)
            //{
            //    // Utilities.log.Error("Caught error while generating SiteEdit tags", ex); // TODO: add logging
            //    return string.Empty; 
            //}
        }


        /**
         * Generates a SiteEdit tag for a componentpresentation. It also needs to know which region it's in (for component
         * swapping) and the order of the page (for a true unique ID).
         * 
         * @param cp The componentpresentation to mark.
         * @param orderOnPage The number the componentpresentation has on the page.
         * @param region The region the componentpresentation is to be shown in.
         * @return string representing the JSON SiteEdit tag.
         * @throws Exception When a componentpresentation is given that can not be parsed into a SiteEdit tag.
         */
        public static string GenerateSiteEditComponentTag(IComponentPresentation cp, string region)
        {
            string pubIdWithoutTcm = Convert.ToString(new TcmUri(cp.Component.Id).PublicationId);
           
            try
            {
                if (SiteEditSettings.ContainsKey(pubIdWithoutTcm))
                {
                    SiteEditSetting setting = SiteEditSettings[pubIdWithoutTcm];
                    if (setting.Enabled)
                    {
                        return string.Format(COMPONENT_SE_Format, cp.OrderOnPage,
                                cp.Component.Id,
                                cp.ComponentTemplate.Id,
                                cp.Component.Version,
                                region);
                    }
                }
                throw new Exception("Cannot create siteEdit component tag, publication is not configured to be editable.");
            }
            catch (Exception ex)
            {
                // log.Error("Caught error while generating SiteEdit tags", ex); // TODO: logging
                return string.Empty;
            }
        }

        /**
         * Basic SiteEdit XPATH Prefix for simple fields (that is, non-multivalue and non-embedded).
         */
        public static string SIMPLE_SE_XPATH_PREFIX = "tcm:Content/custom:Content/custom:";
        public static string SINGLE_VALUE_SE_XPATH_Format = "tcm:Content/custom:{0}/custom:{1}";
        public static string MULTI_VALUE_SE_XPATH_Format = "tcm:Content/custom:{0}/custom:{1}[{2}]";

        /**
         * Function generates a fieldmarking for SiteEditable simple (non-multivalue and non-embedded) fields. For 
         * embedded fields, use the overloaded function with a better xpath. For multivalue fields, please code the JSON
         * yourself.
         * 
         * @param fieldname The Content Manager XML name of the field.
         * @return string representing the JSON SiteEdit tag.
         */
        public static string GenerateSiteEditFieldMarking(string fieldname)
        {
            return string.Format(FIELD_SE_Format, fieldname, "false", SIMPLE_SE_XPATH_PREFIX + fieldname);
        }

        public static string GenerateSiteEditFieldMarking(string fieldname, string schemaname)
        {
            return string.Format(FIELD_SE_Format,
                    fieldname,
                    "false",
                    string.Format(
                            SINGLE_VALUE_SE_XPATH_Format,
                            schemaname,
                            fieldname
                    )
            );
        }


        /**
         * Function generates a fieldmarking for a single-value SiteEditable field based on field name
         * and xpath. For multi-value fields, please code the JSON yourself.
         * 
         * @param fieldname
         * @param xpath
         * @return string representing the JSON SiteEdit tag.
         */
        public static string GenerateSiteEditFieldMarkingWithXpath(string fieldname, string xpath)
        {
            return string.Format(FIELD_SE_Format, fieldname, "false", xpath);
        }

        /**
         * Generates a MV fieldmarking for specific instance of MV field.
         * 
         * @param fieldname Name of the field to mark
         * @param MVOrder Order of the MV instance
         * @return
         */
        public static string GenerateSiteEditFieldMarking(string fieldname, int MVOrder)
        {
            return string.Format(FIELD_SE_Format,
                    fieldname,
                    "true",
                    string.Format(
                            MULTI_VALUE_SE_XPATH_Format,
                            "Content",
                            fieldname,
                            MVOrder
                    )
            );
        }

        /**
         * Generates a MV fieldmarking for specific instance of MV field.
         * 
         * @param fieldname Name of the field to mark
         * @param MVOrder Order of the MV instance
         * @return
         */
        public static string GenerateSiteEditFieldMarking(string fieldname, string schemaname, int MVOrder)
        {
            return string.Format(FIELD_SE_Format,
                    fieldname,
                    "true",
                    string.Format(
                            MULTI_VALUE_SE_XPATH_Format,
                            schemaname,
                            fieldname,
                            MVOrder
                    )
            );
        }
    }


}
