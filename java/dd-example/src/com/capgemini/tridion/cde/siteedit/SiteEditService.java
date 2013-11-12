/**  
 *  Copyright 2011 Capgemini & SDL
 * 
 *    Licensed under the Apache License, Version 2.0 (the "License");
 *    you may not use this file except in compliance with the License.
 *    You may obtain a copy of the License at
 * 
 *        http://www.apache.org/licenses/LICENSE-2.0
 * 
 *    Unless required by applicable law or agreed to in writing, software
 *    distributed under the License is distributed on an "AS IS" BASIS,
 *    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *    See the License for the specific language governing permissions and
 *    limitations under the License.
 */
package com.capgemini.tridion.cde.siteedit;

import javax.servlet.http.HttpServletRequest;

import org.apache.log4j.Logger;

import com.capgemini.tridion.cde.constants.Constants;
import com.capgemini.tridion.cde.siteedit.SiteEditSettings.SiteEditSetting;
import com.capgemini.util.spring.ApplicationContextProvider;
import com.tridion.extensions.dynamicdelivery.foundation.contentmodel.ComponentPresentation;
import com.tridion.extensions.dynamicdelivery.foundation.contentmodel.GenericComponent;
import com.tridion.extensions.dynamicdelivery.foundation.contentmodel.GenericPage;
import com.tridion.util.TCMURI;

/**
 * 
 * The class, with static methods only, serves views and controllers with SiteEdit tags. The point
 * of bundling them is to keep all SiteEdit behaviour in a single point, and hence its strange JSON
 * codes accessible to non-Tridion specialized developers. 
 *
 * @author <a href="rogier.oudshoorn@">Rogier Oudshoorn</a>
 * @version $Revision$
 */
public class SiteEditService {
    private static Logger logger = Logger.getLogger(SiteEditService.class);  
    
    private static SiteEditSettings settings;
    
    static {
        settings = (SiteEditSettings) ApplicationContextProvider.getBean("SiteEditSettings");
    }
    
    /**
     * String format used to create the Page-level SiteEdit tags.
     * 
     */
    public static String PAGE_SE_FORMAT = 
        "<!-- SiteEdit Settings: {"+
            "\"PageID\":\"%1$s\", "+    // page id
            "\"PageVersion\":%2$d, "+   // page version
            "\"ComponentPresentationLocation\":1, "+     // add components to bottom of list, rather then front of list
            "\"BluePrinting\" : {"+
                "\"PageContext\" : \"tcm:0-%3$d-1\", "+ // point to publication where pages must be created
                "\"ComponentContext\" : \"tcm:0-%4$d-1\", "+ // point to publication where components must be created                
                "\"PublishContext\" : \"tcm:0-%5$d-1\" "+ // point to publication where page is published                
            "}"+
        "} -->";       
    
    /**
     * Generates siteEdit tag for given servlet request.
     * 
     * @param req Request for the page the tag belongs to.
     * @return String representing the JSON SiteEdit tag.
     */
    public static String generateSiteEditPageTag(HttpServletRequest req){
        GenericPage page = (GenericPage) req.getAttribute(Constants.PAGE_MODEL_KEY);
        
        
        return generateSiteEditPageTag(page);        
    }
    
    /**
     * Support function, checking if SE is enabled for the given item ID.
     * 
     * @param tcmPubId An ID of a Tridion item
     * 
     * @return whether the item belongs to a publication that has active SE in this web application
     */
    public static boolean isSiteEditEnabled(String tcmPubId){
        try{
            TCMURI uri = new TCMURI(tcmPubId);
            
            if(settings.hasPubSE(uri.getPublicationId())){
                return true;
            }                        
        }
        catch(Exception ex){
            logger.error("Unable to get pubID from URI", ex);
        }
        
        return false;
    }
    
    /**
     * Generates SiteEdit tag for given page.
     * 
     * @param page Page the tag belongs to.
     * @return String representing the JSON SiteEdit tag..
     */
    public static String generateSiteEditPageTag(GenericPage page){
        try{
            TCMURI uri = new TCMURI(page.getId());
            
            if(settings.hasPubSE(uri.getPublicationId())){
                SiteEditSetting setting = settings.getSetting(uri.getPublicationId());                
                return String.format(PAGE_SE_FORMAT, page.getId(), page.getVersion(), setting.getPagePub(),setting.getComponentPub(), setting.getPublishPub());
            }
            else{
                throw new Exception("Cannot create siteEdit pagetag, publication is not configured to be editable.");
            }
        }
        catch(Exception ex){
            logger.error("Unable to get pubID from URI", ex);
            return ex.getMessage();
        }
    }
    
    /**
     * String format representing a Component-level SiteEdit tag.
     * 
     */
    public static String COMPONENT_SE_FORMAT = 
        "<!-- Start SiteEdit Component Presentation: {"+
            "\"ID\" : \"CP%1$d\", "+ // unique id
            "\"ComponentID\" : \"%2$s\", "+ // comp id
            "\"ComponentTemplateID\" : \"%3$s\", "+ // ct id
            "\"ComponentVersion\" : %4$d, "+ // comp version
            "\"IsQueryBased\" : false, "+ // query will be true for lists; out of scope for now
            "\"SwapLabel\" : \"%5$s\" "+ // label with which components can be swapped; so region
        "} -->"; 
    
    /**
     * Generates a SiteEdit tag for a componentpresentation. It also needs to know which region it's in (for component
     * swapping) and the order of the page (for a true unique ID).
     * 
     * @param cp The componentpresentation to mark.
     * @param orderOnPage The number the componentpresentation has on the page.
     * @param region The region the componentpresentation is to be shown in.
     * @return String representing the JSON SiteEdit tag.
     * @throws Exception When a componentpresentation is given that can not be parsed into a SiteEdit tag.
     */
    public static String generateSiteEditComponentTag(ComponentPresentation cp, int orderOnPage, String region) throws Exception{
        if(!(cp.getComponent() instanceof GenericComponent)) throw new Exception("Cannot create siteEdit tags for non-generic component.");
        
        if(!isSiteEditEnabled(cp.getComponent().getId())){
            return "";
        }
        
        return String.format(COMPONENT_SE_FORMAT, orderOnPage,  
                cp.getComponent().getId(), 
                cp.getComponentTemplate().getId(), 
                ((GenericComponent) cp.getComponent()).getVersion(),
                region);
    }
    
    /**
     * String format representing a simple, non-multivalue SiteEdit field marking.
     */
    public static String FIELD_SE_FORMAT = 
        "<!-- Start SiteEdit Component Field: {"+
            "\"ID\" : \"%1$s\", "+         // id (name) of the field
            "\"IsMultiValued\" : %2$s, "+ // multivalue?
            "\"XPath\" : \"%3$s\" "+       // xpath of the field
        "} -->";
    
    /**
     * Basic SiteEdit XPATH Prefix for simple fields (that is, non-multivalue and non-embedded).
     */
    public static String SIMPLE_SE_XPATH_PREFIX = "tcm:Content/custom:Content/custom:";
    public static String SINGLE_VALUE_SE_XPATH_FORMAT = "tcm:Content/custom:%1$s/custom:%2$s";    
    public static String MULTI_VALUE_SE_XPATH_FORMAT = "tcm:Content/custom:%1$s/custom:%2$s[%3$s]";
    
    /**
     * Function generates a fieldmarking for SiteEditable simple (non-multivalue and non-embedded) fields. For 
     * embedded fields, use the overloaded function with a better xpath. For multivalue fields, please code the JSON
     * yourself.
     * 
     * @param fieldname The Content Manager XML name of the field.
     * @return String representing the JSON SiteEdit tag.
     */
    public static String generateSiteEditFieldMarking(String fieldname){
        return String.format(FIELD_SE_FORMAT, fieldname, "false", SIMPLE_SE_XPATH_PREFIX + fieldname);
    }
  
    public static String generateSiteEditFieldMarking(String fieldname, String schemaname){
        return String.format(FIELD_SE_FORMAT, 
                fieldname, 
                "false", 
                String.format(
                        SINGLE_VALUE_SE_XPATH_FORMAT,       
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
     * @return String representing the JSON SiteEdit tag.
     */
    public static String generateSiteEditFieldMarkingWithXpath(String fieldname, String xpath){
        return String.format(FIELD_SE_FORMAT, fieldname, "false", xpath);
    }
    
    /**
     * Generates a MV fieldmarking for specific instance of MV field.
     * 
     * @param fieldname Name of the field to mark
     * @param MVOrder Order of the MV instance
     * @return
     */
    public static String generateSiteEditFieldMarking(String fieldname, int MVOrder){
        return String.format(FIELD_SE_FORMAT, 
                fieldname, 
                "true", 
                String.format(
                        MULTI_VALUE_SE_XPATH_FORMAT,       
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
    public static String generateSiteEditFieldMarking(String fieldname, String schemaname, int MVOrder){
        return String.format(FIELD_SE_FORMAT, 
                fieldname, 
                "true", 
                String.format(
                        MULTI_VALUE_SE_XPATH_FORMAT,       
                        schemaname,
                        fieldname, 
                        MVOrder
                )
        );
    }
}
