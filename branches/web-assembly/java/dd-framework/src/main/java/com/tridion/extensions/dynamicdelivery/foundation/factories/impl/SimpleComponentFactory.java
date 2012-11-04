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
package com.tridion.extensions.dynamicdelivery.foundation.factories.impl;

import java.text.ParseException;

import org.apache.log4j.Logger;
import org.springframework.util.StopWatch;

import com.tridion.broker.StorageException;
import com.tridion.dcp.ComponentPresentation;
import com.tridion.dcp.ComponentPresentationFactory;
import com.tridion.extensions.dynamicdelivery.foundation.contentmodel.Component;
import com.tridion.extensions.dynamicdelivery.foundation.contentmodel.Schema;
import com.tridion.extensions.dynamicdelivery.foundation.contentmodel.SimpleComponent;
import com.tridion.extensions.dynamicdelivery.foundation.contentmodel.exceptions.ItemNotFoundException;
import com.tridion.extensions.dynamicdelivery.foundation.contentmodel.exceptions.NotAuthorizedException;
import com.tridion.extensions.dynamicdelivery.foundation.contentmodel.impl.PublicationImpl;
import com.tridion.extensions.dynamicdelivery.foundation.contentmodel.impl.SchemaImpl;
import com.tridion.extensions.dynamicdelivery.foundation.contentmodel.impl.SimpleComponentImpl;
import com.tridion.extensions.dynamicdelivery.foundation.factories.ComponentFactory;
import com.tridion.extensions.dynamicdelivery.foundation.filters.FilterException;
import com.tridion.extensions.dynamicdelivery.foundation.filters.impl.BaseFilter;
import com.tridion.extensions.dynamicdelivery.foundation.request.RequestContext;
import com.tridion.extensions.dynamicdelivery.foundation.util.HomeUtils;
import com.tridion.extensions.dynamicdelivery.foundation.util.TridionUtils;
import com.tridion.storage.ComponentMeta;
import com.tridion.storage.PageMeta;
import com.tridion.storage.StorageManagerFactory;
import com.tridion.storage.StorageTypeMapping;
import com.tridion.storage.dao.ComponentPresentationDAO;
import com.tridion.storage.dao.ItemDAO;
import com.tridion.storage.dao.PageDAO;
import com.tridion.storage.util.ComponentPresentationTypeEnum;
import com.tridion.util.TCMURI;

/**
 * Factory for SimpleComponent objects. Note the simple component objects should
 * not be cached because they are already cached in the broker.
 * 
 * @author bjornl
 * 
 */
public class SimpleComponentFactory extends BaseFactory implements
		ComponentFactory {

	private static Logger logger = Logger
			.getLogger(SimpleComponentFactory.class);

	private String defaultComponentTemplateUri;
	private String defaultOutputFormat = "XML Document";



	/**
	 * Get the component by the uri
	 * 
	 * @return the component
	 * @throws ItemNotFoundException
	 *             if no item found
	 * @throws NotAuthorizedException
	 *             if the user is not authorized to get the item
	 */
	@Override
	public Component getComponent(String componentUri, RequestContext context)
			throws ItemNotFoundException, NotAuthorizedException {

		return getComponent(componentUri, null, context);
	}

	/**
	 * Get the component by the uri. No security available; the method will fail if a
	 * SecurityFilter is configured on the factory.
	 * 
	 * @return the component
	 * @throws ItemNotFoundException
	 *             if no item found
	 * @throws NotAuthorizedException
	 *             if the user is not authorized to get the item
	 */
	@Override
	public Component getComponent(String uri) throws ItemNotFoundException,
			NotAuthorizedException {

		return getComponent(uri, null, null);
	}

	/**
	 * Get the component by the component uri and component template uri
	 * 
	 * @return the component
	 * @throws ItemNotFoundException
	 *             if no item found
	 * @throws NotAuthorizedException
	 *             if the user is not authorized to get the item
	 */
	@Override
	public Component getComponent(String componentUri,
			String componentTemplateUri, RequestContext context)
			throws ItemNotFoundException, NotAuthorizedException {

		if (context == null && securityFilterPresent()) {
			throw new RuntimeException(
					"use of getComponent is not allowed when a SecurityFilter is set");
		}		
		
		StopWatch stopWatch = null;
		if (logger.isDebugEnabled()) {
			logger.debug("Enter getComponent url: " + componentUri
					+ " and componentTemplateUri: " + componentTemplateUri);
			stopWatch = new StopWatch("getComponent");
			stopWatch.start();
		}

		Component component = null;

		try {
		        TCMURI tcmUri = new TCMURI(componentUri);
                    
                        ItemDAO itemDAO = (ItemDAO) StorageManagerFactory.getDAO(tcmUri.getPublicationId(), StorageTypeMapping.COMPONENT_META);
                        ComponentMeta componentMeta = (ComponentMeta) itemDAO.findByPrimaryKey(tcmUri.getPublicationId(), tcmUri.getItemId());
                        
			component = getComponentFromMeta(componentMeta,
					componentTemplateUri);

			try {
				doFilters(component, context, BaseFilter.RunPhase.Both);
			} catch (FilterException e) {
				logger.error("Error in filter. ", e);
				throw new RuntimeException(e);
			}

		} catch (ParseException e) {
			logger.error("Not able to parse uri: " + componentUri, e);
			throw new RuntimeException(e);
		} catch (StorageException e) {
			logger.error("Not possible to get uri: " + componentUri, e);
			throw new RuntimeException(e);
		}

		if (logger.isDebugEnabled()) {
			stopWatch.stop();
			logger.debug("Exit getComponent (" + stopWatch.getTotalTimeMillis()
					+ " ms)");
		}

		return component;
	}

	/**
	 * Get the component by the component uri and component template uri. No
	 * security available; the method will fail if a SecurityFilter is
	 * configured on the factory.
	 * 
	 * @return the component
	 * @throws ItemNotFoundException
	 *             if no item found
	 * @throws NotAuthorizedException
	 *             if the user is not authorized to get the item
	 */
	@Override
	public Component getComponent(String componentUri,
			String componentTemplateUri) throws ItemNotFoundException,
			NotAuthorizedException {

		return getComponent(componentUri, componentTemplateUri, null);
	}

	private SimpleComponent getComponentFromMeta(ComponentMeta componentMeta,
			String componentTemplateUri) throws ItemNotFoundException {

		if (componentMeta != null) {

			SimpleComponent component = getComponentFromMetadata(componentMeta);
			
			ComponentPresentationFactory factory = new ComponentPresentationFactory(componentMeta.getPublicationId());
			
			/*
			ComponentPresentationDAO cpDAO = (ComponentPresentationDAO) StorageManagerFactory.getDAO(ComponentMeta.getPublicationId(), StorageTypeMapping.COMPONENT_PRESENTATION);
                        
			cpDAO.getComponentPresentation(componentMeta.getPublicationId(), componentMeta.getItemId(), 0, ComponentPresentationTypeEnum.JSP);
                        data = pageDAO.findByPrimaryKey(pageMeta.getPublicationId(),
                                        pageMeta.getItemId());
			*/
			/*
			ComponentPresentationFactory cpFactory = HomeUtils.getInstance()
					.getComponentPresentationFactory(
							componentMeta.getPublicationId());
							
							*/
			ComponentPresentation cp = null;

			TCMURI templateUri = null;
			if (componentTemplateUri != null) {
				logger.debug("comonentTemplateUri is specified");
				try {
					templateUri = new TCMURI(componentTemplateUri);
					cp = factory.getComponentPresentation(
							componentMeta.getItemId(), templateUri.getItemId());
					if (cp == null) {
						// no cp found, return simple component without source
						return component;
					}
					String source = cp.getContent();
					component.setSource(source);
					return component;
				} catch (ParseException e) {
					logger.error("Not possible to parse uri: "
							+ componentTemplateUri);
					throw new RuntimeException(e);
				}
			}

			// option 1: use defaultComponentTemplateUri
			String defCT = this.getDefaultComponentTemplateUri();
			if (defCT != null && !defCT.equals("")) {
				logger.debug("defaultComponentTemplateUri is specified as "
						+ defCT);
				TCMURI defCTUri;
				try {
					defCTUri = new TCMURI(this.defaultComponentTemplateUri);
					cp = factory.getComponentPresentation(
							componentMeta.getItemId(), defCTUri.getItemId());
				} catch (ParseException e) {
					logger.warn(
							"malformed defaultComponentTemplateUri in configuration",
							e);
				}
			}

			if (cp == null) {
				// still no cp, try option 2:
				// use OutputFormat
				String defOF = this.getDefaultOutputFormat();
				if (defOF != null && !defOF.equals("")) {
					logger.debug("defaultOutputFormat is specified as " + defOF);

					cp = factory.getComponentPresentationWithOutputFormat(
							componentMeta.getItemId(), defOF);
				}
			}
			
			if(cp == null){
			    // then lets check priority
			    cp = factory.getComponentPresentationWithHighestPriority(componentMeta.getItemId());
			}

			if (cp != null) {
				logger.debug("component presentation found");
				String source = cp.getContent();
				component.setSource(source);
			}

			return component;

		}
		throw new ItemNotFoundException("Component metadata is null");
		}

	private SimpleComponent getComponentFromMetadata(ComponentMeta componentMeta) {
		SimpleComponent component = new SimpleComponentImpl();
		component.setId(TridionUtils.createUri(componentMeta).toString());
		component.setTitle(componentMeta.getTitle());
		component.setNativeMetadata(componentMeta);

		PublicationImpl pub = new PublicationImpl();
		pub.setId(String.valueOf(componentMeta.getPublicationId()));
		component.setPublication(pub);

		Schema schema = new SchemaImpl();
		schema.setId(String.valueOf(componentMeta.getSchemaId()));
		component.setSchema(schema);

		return component;
	}

	public String getDefaultComponentTemplateUri() {
		return defaultComponentTemplateUri;
	}

	public void setDefaultComponentTemplateUri(
			String defaultComponentTemplateUri) {
		this.defaultComponentTemplateUri = defaultComponentTemplateUri;
	}

	public void setDefaultOutputFormat(String defaultOutputFormat) {
		this.defaultOutputFormat = defaultOutputFormat;
	}

	public String getDefaultOutputFormat() {
		return defaultOutputFormat;
	}
}
	
	