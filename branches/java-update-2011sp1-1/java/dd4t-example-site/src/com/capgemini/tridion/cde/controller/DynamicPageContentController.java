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

import org.dd4t.contentmodel.Component;
import org.dd4t.contentmodel.ComponentPresentation;
import org.dd4t.contentmodel.GenericPage;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.web.servlet.ModelAndView;

import com.capgemini.tridion.cde.siteedit.SiteEditService;
import com.capgemini.tridion.cde.view.IViewHandler;
import com.capgemini.tridion.cde.view.model.ComponentViews;
import com.capgemini.tridion.cde.view.model.ViewRegion;

public class DynamicPageContentController extends BaseDD4TController implements
        ContentController {
    private static Logger logger = LoggerFactory
            .getLogger(DynamicPageContentController.class);

    @Autowired
    private IViewHandler<Component> componentViewManager;

    public IViewHandler<Component> getComponentViewManager() {

        return componentViewManager;
    }

    public void setComponentViewManager(
            IViewHandler<Component> componentViewManager) {

        this.componentViewManager = componentViewManager;
    }

    /**
     * Function builds a ComponentViews model based on given pagemodel
     */
    public ComponentViews buildComponentViews(GenericPage model,
            HttpServletRequest req, HttpServletResponse res) throws Exception {

        ComponentViews viewmodel = new ComponentViews();

        int order = 1;
        for (ComponentPresentation cp : model.getComponentPresentations()) {
            if (logger.isDebugEnabled()){
                logger.debug("found cp with ct " + cp.getComponentTemplate().getId());
            }

            String region = getRegionFromTemplate(cp.getComponentTemplate());
            if (!viewmodel.getRegions().containsKey(region)) {
                viewmodel.getRegions().put(region, new ViewRegion());
            }
            
            if (logger.isDebugEnabled()){
                logger.debug("using region " + region);
            }
            
            // attempt to load the view result from the rendered content
            String viewresult = cp.getRenderedContent();

            // if successfull, no need to dispatch
            if (viewresult != null && viewresult.length()>0) {
            	if (logger.isDebugEnabled()){
            	    logger.debug("found cp with (pre)rendered content");
            	}
            } 
            // otherwise, we'll need to call the viewManager
            else {
                String view = getViewFromTemplate(cp.getComponentTemplate());	
                if (logger.isDebugEnabled()){
                    logger.debug("using view " + view);
                }
                
                // add the site edit String to the generated HTML
                String se = SiteEditService.generateSiteEditComponentTag(cp, order, region, req);
                
                viewresult = "<div>"+se +
                    componentViewManager.handleView(model, cp.getComponent(), view,
                            req, res) +"</div>";
            }

            viewmodel.getRegions().get(region).getComponentViews()
                    .add(viewresult);
            
            order++;
        }

        return viewmodel;
    }


    
    @Override
    protected ModelAndView handleRequestInternal(HttpServletRequest request,
            HttpServletResponse response) throws Exception {

    	return null;
    }
}
