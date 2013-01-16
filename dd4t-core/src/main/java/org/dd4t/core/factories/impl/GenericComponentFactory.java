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
package org.dd4t.core.factories.impl;

import java.text.ParseException;

import org.apache.log4j.Logger;
import org.dd4t.contentmodel.Component;
import org.dd4t.contentmodel.ComponentPresentation;
import org.dd4t.contentmodel.GenericComponent;
import org.dd4t.contentmodel.GenericPage;
import org.dd4t.contentmodel.Schema;
import org.dd4t.contentmodel.SimpleComponent;
import org.dd4t.contentmodel.exceptions.ItemNotFoundException;
import org.dd4t.contentmodel.exceptions.NotAuthenticatedException;
import org.dd4t.contentmodel.exceptions.NotAuthorizedException;
import org.dd4t.contentmodel.impl.BasePublishedItem;
import org.dd4t.contentmodel.impl.GenericComponentImpl;
import org.dd4t.contentmodel.impl.PublicationImpl;
import org.dd4t.contentmodel.impl.SchemaImpl;
import org.dd4t.contentmodel.impl.SimpleComponentImpl;
import org.dd4t.core.factories.ComponentFactory;
import org.dd4t.core.filters.FilterException;
import org.dd4t.core.filters.impl.BaseFilter;
import org.dd4t.core.request.RequestContext;
import org.dd4t.core.util.TridionUtils;
import org.dd4t.providers.ComponentProvider;
import org.dd4t.providers.impl.BrokerComponentProvider;
import org.springframework.util.StopWatch;

import com.tridion.broker.StorageException;
import com.tridion.linking.ComponentLink;
import com.tridion.linking.Link;
import com.tridion.storage.ComponentMeta;
import com.tridion.util.TCMURI;

public class GenericComponentFactory extends BaseFactory implements
		ComponentFactory {

	private static Logger logger = Logger
			.getLogger(GenericComponentFactory.class);


	// provider class to use for finding content
	private ComponentProvider componentProvider;
	
	public ComponentProvider getComponentProvider() {
		if(componentProvider == null){
			componentProvider = new BrokerComponentProvider();
		}
		return componentProvider;
	}

	public void setComponentProvider(ComponentProvider componentProvider) {
		this.componentProvider = componentProvider;
	}

	private GenericPageFactory pageFactory = null;
	public GenericPageFactory getPageFactory() {
		if (pageFactory == null)
				pageFactory = new GenericPageFactory();
		return pageFactory;
	}

	public void setPageFactory(GenericPageFactory pageFactory) {
		this.pageFactory = pageFactory;
	}

	
	/**
	 * Get the component by the uri
	 * 
	 * @return the component
	 * @throws ItemNotFoundException
	 *             if no component found
	 * @throws NotAuthorizedException
	 *             if the user is not authorized to get the component
	 * @throws NotAuthenticatedException 
	 */
	@Override
	public Component getComponent(String componentUri, RequestContext context)
			throws ItemNotFoundException, NotAuthorizedException, NotAuthenticatedException {
		return getComponent(componentUri, null, context);
	}

	/**
	 * Get the component by the uri. No security available; the method will fail
	 * if a SecurityFilter is configured on the factory.
	 * 
	 * @return the component
	 * @throws ItemNotFoundException
	 *             if no component found
	 * @throws NotAuthorizedException
	 *             if the user is not authorized to get the component
	 * @throws NotAuthenticatedException 
	 */
	@Override
	public Component getComponent(String uri) throws ItemNotFoundException,
			NotAuthorizedException, NotAuthenticatedException {

		return getComponent(uri, null, null);
	}
	
	/**
	 * Function retrieves a genericComponent from the GenericPage it is stored in.
	 * 
	 * @param tcmUri
	 * @return
	 * @throws ItemNotFoundException
	 */
	@Override
	public Component getEmbeddedComponent(String uri) throws ItemNotFoundException{
		Component comp = null;
		
		TCMURI tcmUri;
		try {
			tcmUri = new TCMURI(uri);
		} catch (ParseException e) {
			throw new ItemNotFoundException("Cannot find item by uri '"+uri+"' as the uri cannot be parsed.");
		}
		
        // get a componentlinkresolver
        ComponentLink cLink = new ComponentLink(tcmUri.getPublicationId());
        
        // resolve the componentlink
        Link link = cLink.getLink(tcmUri.getItemId());
        
        if (logger.isDebugEnabled()) {
            logger.debug("Found link '" + link+"' and '"+link.getURL() + "'");
        }
        
        if (link != null && link.getURL() != null){
        	//this is a proper matching id...
            String matchid = tcmUri.toString();
            String[] token = matchid.split("-");
            if(token.length>=2){
               matchid = token[0].concat("-").concat(token[1]);
            }              
              
              
                // get the page through URL to use right cachekeys (faster!)
                GenericPage page = (GenericPage) getPageFactory().findPageByUrl(link.getURL(), tcmUri.getPublicationId());
                
                // if pageload is successfull
                if (page != null) {
                    // loop over its componentpresentations
                    for (ComponentPresentation cp : page.getComponentPresentations()) {
                        if (logger.isDebugEnabled()) {
                            logger.debug("Search for match between '" + cp.getComponent().getId() + "' and '" +matchid +"'");
                        }
                        // until the one we want is located
                        if (comp == null && cp.getComponent().getId().equals(matchid)) {
                            // found 'our' component, use that 
                            if (logger.isDebugEnabled()) {
                                logger.debug("Matched component on ID!");
                            }
                          
                            comp = (GenericComponent) cp.getComponent();
                            
                            if(logger.isDebugEnabled()){
                            	logger.debug("resolveComponents added is "+(GenericComponent) cp.getComponent());
                            }
                            // and stop evaluating
                        }
                    }                    
                }
          }
        
        if(comp == null){
        	throw new ItemNotFoundException("Unable to find component by uri "+tcmUri.toString());
        }
          
        return comp;
	}
	
	/**
	 * Get the component by the component uri and template uri.
	 * 
	 * @return the component
	 * @throws ItemNotFoundException
	 *             if no item found NotAuthorizedException if the user is not
	 *             authorized to get the component
	 * @throws NotAuthenticatedException 
	 */
	@Override
	public Component getComponent(String componentUri,
			String componentTemplateUri, RequestContext context)
			throws ItemNotFoundException, NotAuthorizedException, NotAuthenticatedException {

		if (context == null && securityFilterPresent()) {
			throw new RuntimeException(
					"use of getComponent without a context is not allowed when a SecurityFilter is set");
		}

		StopWatch stopWatch = null;
		if (logger.isDebugEnabled()) {
			logger.debug("Enter getComponent with uri: " + componentUri);
			stopWatch = new StopWatch("getComponent");
			stopWatch.start();
		}

		GenericComponent component = (GenericComponent) getCacheProvider()
				.loadFromLocalCache(componentUri);
		if (component == null) {
	        TCMURI tcmUri;
	        ComponentMeta componentMeta;
	        SimpleComponent simple  = new SimpleComponentImpl();
	        
			try {
				if(!componentUri.endsWith("-16")){
					componentUri += "-16";
				}
				tcmUri = new TCMURI(componentUri);
				componentMeta = getComponentProvider().getComponentMeta(tcmUri.getItemId(), tcmUri.getPublicationId());
				
	            
	            simple.setId(TridionUtils.createUri(componentMeta).toString());
	            simple.setTitle(componentMeta.getTitle());
	            simple.setNativeMetadata(componentMeta);

	    		PublicationImpl pub = new PublicationImpl();
	    		pub.setId(String.valueOf(componentMeta.getPublicationId()));
	    		simple.setPublication(pub);

	    		Schema schema = new SchemaImpl();
	    		schema.setId(String.valueOf(componentMeta.getSchemaId()));
	    		simple.setSchema(schema);
	    		
	            if(componentTemplateUri != null){
	            	TCMURI ctUri = new TCMURI(componentTemplateUri);
	            	
	            	simple.setSource(getComponentProvider().getComponentXMLByTemplate(tcmUri.getItemId(), ctUri.getItemId(), tcmUri.getPublicationId()));
	            }
	            else{
	            	simple.setSource(getComponentProvider().getComponentXML(tcmUri.getItemId(), tcmUri.getPublicationId()));
	            }
			} catch (ParseException e1) {
				logger.error("No item found with uri: "
						+ componentUri + "; could not parse the URI: "+e1);
				throw new ItemNotFoundException("No item found with uri: "
						+ componentUri + "; could not parse the URI: "+e1);
			} catch (StorageException e2){
				logger.error("No item found with uri: "
						+ componentUri + "; broker is down.");
				throw new ItemNotFoundException("No item found with uri: "
						+ componentUri + "; broker is down.");
			}
            
			if (simple.getSource() == null) {
				logger.error("Source is null for item: " + componentUri
						+ " and templateUri: " + componentTemplateUri);
				throw new ItemNotFoundException("No item found with uri: "
						+ componentUri + " and templateUri: "
						+ componentTemplateUri);
			}

			try {
				if (logger.isDebugEnabled()) {
					stopWatch.stop();
					stopWatch.start();
				}

				component = (GenericComponent) this
						.getSerializer()
						.deserialize(simple.getSource(), GenericComponentImpl.class);

				if (logger.isDebugEnabled()) {
					stopWatch.stop();
					logger.debug("Deserialization of component took: "
							+ stopWatch.getLastTaskTimeMillis() + " ms");
					stopWatch.start();
				}

				component.setNativeMetadata(simple.getNativeMetadata());
				
				// Rogier Oudshoorn, 7/2/2012
				// object size is roughly half the xml string size; 
				
				((BasePublishedItem) component).setSourceSize(simple.getSource().length());

				try {
					doFilters(component, context,
							BaseFilter.RunPhase.BeforeCaching);
				} catch (FilterException e) {
					logger.error("Error in filter. ", e);
					throw new RuntimeException(e);
				}

				getCacheProvider().storeInItemCache(componentUri, component, component.getNativeMetadata().getPublicationId(), component.getNativeMetadata().getItemId());				
			} catch (Exception e) {
				logger.error("error when deserializing component", e);
				throw new RuntimeException(e);
			}
		}

		try {
			doFilters(component, context, BaseFilter.RunPhase.AfterCaching);
		} catch (FilterException e) {
			logger.error("Error in filter. ", e);
			throw new RuntimeException(e);
		}

		if (logger.isDebugEnabled()) {
			stopWatch.stop();
			logger.debug("Exit getComponent (" + stopWatch.getTotalTimeMillis()
					+ " ms)");
		}

		return component;
	}
	public GenericComponent getComponentFromSource(String source) {
		StopWatch stopWatch = null;
		if (logger.isDebugEnabled()) {
			stopWatch = new StopWatch("getComponentFromSource");
			stopWatch.start();
		}
		try {
			if (logger.isDebugEnabled()) {
				stopWatch.stop();
				stopWatch.start();
			}

			return (GenericComponent) this.getSerializer().deserialize(source, GenericComponent.class);

			
		} catch (Exception e) {
			logger.error("error when deserializing component", e);
			throw new RuntimeException(e);
		} finally {
			if (logger.isDebugEnabled()) {
				stopWatch.stop();
				logger.debug("Deserialization of component took: "
						+ stopWatch.getLastTaskTimeMillis() + " ms");
			}
		}
	}

	/**
	 * Get the component by the component uri and template uri. No security
	 * available; the method will fail if a SecurityFilter is configured on the
	 * factory.
	 * 
	 * @return the component
	 * @throws ItemNotFoundException
	 *             if no item found NotAuthorizedException if the user is not
	 *             authorized to get the component
	 * @throws NotAuthenticatedException 
	 */
	@Override
	public Component getComponent(String componentUri,
			String componentTemplateUri) throws ItemNotFoundException,
			NotAuthorizedException, NotAuthenticatedException {

		return getComponent(componentUri, componentTemplateUri, null);
	}
}
