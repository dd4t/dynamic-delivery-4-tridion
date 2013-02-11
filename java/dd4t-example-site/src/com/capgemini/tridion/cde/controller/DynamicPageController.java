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
package com.capgemini.tridion.cde.controller;

import javax.servlet.http.HttpServletRequest;
import javax.servlet.http.HttpServletResponse;

import org.apache.log4j.Logger;
import org.dd4t.contentmodel.GenericPage;
import org.dd4t.contentmodel.Page;
import org.dd4t.core.factories.impl.GenericPageFactory;
import org.dd4t.core.factories.impl.SimplePageFactory;
import org.dd4t.core.filters.Filter;
import org.dd4t.core.filters.LinkResolverFilter;
import org.dd4t.core.request.impl.BasicRequestContext;
import org.springframework.web.servlet.ModelAndView;

import com.capgemini.tridion.cde.constants.Constants;
import com.capgemini.tridion.cde.view.model.ComponentViews;

public class DynamicPageController extends BaseCWAController {
    private static Logger logger = Logger
            .getLogger(DynamicPageController.class);

    private boolean isInitialized;
    
    private GenericPageFactory genericPageFactory;

    private SimplePageFactory simplePageFactory;
    
    private ContentController contentController;
    
    private int publication;
    
    private String subcontext;

    public String getSubcontext() {
    
        return subcontext;
    }

    public void setSubcontext(String subcontext) {
    
        this.subcontext = subcontext;
    }

    public int getPublication() {
    
        return publication;
    }

    public void setPublication(int publication) {
    
        this.publication = publication;
    }

    public ContentController getContentController() {

        return contentController;
    }

    public void setContentController(ContentController contentController) {

        this.contentController = contentController;
    }

    public SimplePageFactory getSimplePageFactory() {
        
        return simplePageFactory;
    }

    public void setSimplePageFactory(SimplePageFactory simplePageFactory) {
    
        this.simplePageFactory = simplePageFactory;
    }
    
    public GenericPageFactory getGenericPageFactory() {

        return genericPageFactory;
    }

    public void setGenericPageFactory(GenericPageFactory genericPageFactory) {

        this.genericPageFactory = genericPageFactory;
    }

    @Override
    protected ModelAndView handleRequestInternal(HttpServletRequest request,
            HttpServletResponse response) throws Exception {

        long start = System.currentTimeMillis();
        
        if (! isInitialized) {
            // set context root on the link resolver filter
            if (logger.isDebugEnabled())
                    logger.debug("setting context path for all LinkResolverFilters");
            for (Filter filter : getGenericPageFactory().getFilters()) {
                    if (filter instanceof LinkResolverFilter) {
                            if (logger.isDebugEnabled())
                                    logger.debug("setting context path for LinkResolverFilter " + filter);
                            ((LinkResolverFilter)filter).setContextPath(request.getContextPath());
                    }               
            }
            isInitialized = true;
        }        
        
        String URL = request.getRequestURI();
        
        if(logger.isDebugEnabled())
            logger.debug("Received request in MAV " + URL);

        URL = URL.replaceFirst(request.getContextPath(), "");

        if(logger.isDebugEnabled())
            logger.debug("Request shortened to " + URL);

        GenericPage pageModel =
                (GenericPage) genericPageFactory.findPageByUrl(URL, publication,
                new BasicRequestContext(request));

        String view = getViewFromTemplate(pageModel.getPageTemplate());

        ModelAndView mav = new ModelAndView(view);      
        mav.addObject(Constants.PAGE_MODEL_KEY, pageModel); 
        
        long pagemodeldone = System.currentTimeMillis();
        
        if(logger.isDebugEnabled())
            logger.debug("Built pageModel for page: " + pageModel.getTitle() +" in "+(pagemodeldone-start)+" milliseconds.");

        ComponentViews contentModel =
                contentController.buildComponentViews(pageModel, request,
                        response);

        long contentmodeldone = System.currentTimeMillis();
        
        mav.addObject(Constants.CONTENT_MODEL_KEY, contentModel);
        
        if(logger.isDebugEnabled())
            logger.debug("Built contentModel: " + contentModel+" in "+(contentmodeldone-pagemodeldone)+" milliseconds.");

        /*
        Page navigationModel = getNavigationModel(pageModel);
        
        long navmodeldone = System.currentTimeMillis();
        
        if(logger.isDebugEnabled())
            logger.debug("Built navigationmodel: " + navigationModel+" in "+(navmodeldone-contentmodeldone)+" milliseconds.");  
        mav.addObject(Constants.NAVIGATION_MODEL_KEY, navigationModel);  
          */
          

        if(logger.isInfoEnabled()){
            long end = System.currentTimeMillis();        
            logger.info("Built pageresponse in " + (end - start) + " milliseconds.");
        }
        
        return mav;        
    }
    
    public Page getNavigationModel(Page page){
        try{
            return simplePageFactory.findPageByUrl(subcontext + Constants.NAVIGATION_PAGE,publication);
        }
        catch(Exception ex){
            logger.error("Unable to retrieve navigationmodel: "+ex.getMessage(), ex);
        }
        
        return null;
    }
}
