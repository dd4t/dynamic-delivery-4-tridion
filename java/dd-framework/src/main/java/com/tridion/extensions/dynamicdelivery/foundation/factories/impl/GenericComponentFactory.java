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
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.util.StopWatch;

import com.tridion.extensions.dynamicdelivery.foundation.contentmodel.Component;
import com.tridion.extensions.dynamicdelivery.foundation.contentmodel.ComponentPresentation;
import com.tridion.extensions.dynamicdelivery.foundation.contentmodel.GenericComponent;
import com.tridion.extensions.dynamicdelivery.foundation.contentmodel.GenericPage;
import com.tridion.extensions.dynamicdelivery.foundation.contentmodel.SimpleComponent;
import com.tridion.extensions.dynamicdelivery.foundation.contentmodel.exceptions.ItemNotFoundException;
import com.tridion.extensions.dynamicdelivery.foundation.contentmodel.exceptions.NotAuthorizedException;
import com.tridion.extensions.dynamicdelivery.foundation.contentmodel.impl.GenericComponentImpl;
import com.tridion.extensions.dynamicdelivery.foundation.factories.ComponentFactory;
import com.tridion.extensions.dynamicdelivery.foundation.filters.FilterException;
import com.tridion.extensions.dynamicdelivery.foundation.filters.impl.BaseFilter;
import com.tridion.extensions.dynamicdelivery.foundation.request.RequestContext;
import com.tridion.linking.ComponentLink;
import com.tridion.linking.Link;
import com.tridion.storage.ComponentMeta;
import com.tridion.util.TCMURI;

public class GenericComponentFactory extends BaseFactory implements
		ComponentFactory {

	private static Logger logger = Logger
			.getLogger(GenericComponentFactory.class);

	private SimpleComponentFactory simpleComponentFactory = new SimpleComponentFactory();

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
	 */
	@Override
	public Component getComponent(String componentUri, RequestContext context)
			throws ItemNotFoundException, NotAuthorizedException {
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
	 */
	@Override
	public Component getComponent(String uri) throws ItemNotFoundException,
			NotAuthorizedException {

		return getComponent(uri, null, null);
	}

	/**
	 * Get the component by the component uri and template uri.
	 * 
	 * @return the component
	 * @throws ItemNotFoundException
	 *             if no item found NotAuthorizedException if the user is not
	 *             authorized to get the component
	 */
	@Override
	public Component getComponent(String componentUri,
			String componentTemplateUri, RequestContext context)
			throws ItemNotFoundException, NotAuthorizedException {

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

		GenericComponent component = (GenericComponent) getCacheAgent()
				.loadFromLocalCache(componentUri);
		if (component == null) {
			// it is not necessary to pass the context along
			// since we are not using filters anyway
			SimpleComponent simple = (SimpleComponent) simpleComponentFactory
					.getComponent(componentUri, componentTemplateUri);

			if (simple == null) {
				// still no cp, try option 3:
				// look for page that contains this component
				TCMURI tcmUri;
				try {
					tcmUri = new TCMURI(componentUri);
				} catch (ParseException e) {
					logger.warn("cannot parse uri '" + componentUri + "', unable to locate page for this component");
					throw new ItemNotFoundException("No item found with uri: "
							+ componentUri + " and templateUri: "
							+ componentTemplateUri);					
				}
				ComponentLink cLink = new ComponentLink(tcmUri.getPublicationId());
				Link link = cLink.getLink(tcmUri.getItemId());
				if (link != null) {
					// try to deserialize the page
					GenericPage page = (GenericPage) getPageFactory().getPage(link.getTargetURI());
					if (page != null) {
						for (ComponentPresentation cp : page.getComponentPresentations()) {
							if (cp.getComponent().getId().equals(componentUri)) {
								// found 'our' component, use that 
								logger.debug("retrieved component from the page with URI " + page.getId() + " because it was not found in the broker database as a DCP");
								return cp.getComponent();
							}
						}
					}
				}
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
				component.setSource(simple.getSource());

				try {
					doFilters(component, context,
							BaseFilter.RunPhase.BeforeCaching);
				} catch (FilterException e) {
					logger.error("Error in filter. ", e);
					throw new RuntimeException(e);
				}

				getCacheAgent().storeInCache(componentUri, component,
						component.getNativeMetadata());

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
	 */
	@Override
	public Component getComponent(String componentUri,
			String componentTemplateUri) throws ItemNotFoundException,
			NotAuthorizedException {

		return getComponent(componentUri, componentTemplateUri, null);
	}
}
