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
import com.tridion.extensions.dynamicdelivery.foundation.contentmodel.GenericPage;
import com.tridion.extensions.dynamicdelivery.foundation.contentmodel.Page;
import com.tridion.extensions.dynamicdelivery.foundation.contentmodel.exceptions.ItemNotFoundException;
import com.tridion.extensions.dynamicdelivery.foundation.contentmodel.exceptions.NotAuthorizedException;
import com.tridion.extensions.dynamicdelivery.foundation.factories.PageFactory;
import com.tridion.extensions.dynamicdelivery.foundation.filters.FilterException;
import com.tridion.extensions.dynamicdelivery.foundation.filters.impl.BaseFilter;
import com.tridion.extensions.dynamicdelivery.foundation.request.RequestContext;
import com.tridion.extensions.dynamicdelivery.foundation.util.BrokerUtils;
import com.tridion.storage.ItemMeta;
import com.tridion.storage.PageMeta;
import com.tridion.storage.StorageManagerFactory;
import com.tridion.storage.StorageTypeMapping;
import com.tridion.storage.dao.ItemDAO;
import com.tridion.util.TCMURI;

public class GenericPageFactory extends BaseFactory implements PageFactory {

	private static Logger logger = Logger.getLogger(GenericPageFactory.class);
		
	private SimplePageFactory simplePageFactory = new SimplePageFactory();
		
	/**
	 * Get a page by its URI. No security available; the method will fail if a
	 * SecurityFilter is configured on the factory.
	 * 
	 * @param uri
	 *            of the page
	 * @return
	 * @throws ItemNotFoundException
	 */
	public Page getPage(String uri) throws ItemNotFoundException {

		Page page = null;
		try {
			page = this.getGenericPage(uri, null);
		} catch (NotAuthorizedException e) {
			// can be ignored
			logger.warn("unexpected NotAuthorizedException: no SecurityFilter is configured");
		}
		return page;
	}

	/**
	 * Get a page by the uri.
	 * 
	 * @return the page
	 * @throws NotAuthorizedException
	 *             if not allowed to get the page.
	 * @throws ItemNotFoundException
	 *             if no page found.
	 */
	@Override
	public Page getPage(String uri, RequestContext context)
			throws ItemNotFoundException, NotAuthorizedException {

		return this.getGenericPage(uri, context);
	}

	private Page getGenericPage(String uri, RequestContext context)
			throws ItemNotFoundException, NotAuthorizedException {

		if (context == null && securityFilterPresent()) {
			throw new RuntimeException(
					"use of getPage is not allowed when a SecurityFilter is set");
		}
		
		StopWatch stopWatch = null;
		if (logger.isDebugEnabled()) {
			logger.debug("Enter getGenericPage with uri: " + uri);
			stopWatch = new StopWatch("getGenericPage");
			stopWatch.start();
		}

		GenericPage page = (GenericPage) getCacheAgent()
				.loadFromLocalCache(uri);
		if (page == null) {
			try {

				// it is not necessary to supply a context since we are 
				// not using any filters anyway
				Page simplePage = simplePageFactory.getPage(uri);

				page = getGenericFromSimplePage(simplePage);

				TCMURI tcmUri = new TCMURI(uri);
				ItemDAO pageMetaDao = (ItemDAO) StorageManagerFactory.getDAO(tcmUri.getPublicationId(), StorageTypeMapping.PAGE_META);
				PageMeta pageMeta = (PageMeta) pageMetaDao.findByPrimaryKey(tcmUri.getPublicationId(), tcmUri.getItemId());
				page.setNativeMetadata(pageMeta);
				page.setSource(simplePage.getSource());

				try {
					// run only the filters where the result is allowed to be
					// cached.
					doFilters(page, context, BaseFilter.RunPhase.BeforeCaching);
				} catch (FilterException e) {
					logger.error("Error in filter. ", e);
					throw new RuntimeException(e);
				}

				
				getCacheAgent().storeInCache(uri, page, (ItemMeta) pageMeta);

			} catch (ParseException e) {
				logger.error("ParseException when searching for page: " + uri,
						e);
				throw new RuntimeException(e);
			} catch (StorageException e) {
				logger.error(
						"StorageException when searching for page: " + uri, e);
				throw new RuntimeException(e);
			}
		}

		try {
			// run only the filters where the result is not allowed to be
			// cached.
			doFilters(page, context, BaseFilter.RunPhase.AfterCaching);
		} catch (FilterException e) {
			logger.error("Error in filter. ", e);
			throw new RuntimeException(e);
		} finally {
			if (logger.isDebugEnabled()) {
				stopWatch.stop();
				logger.debug("Exit getGenericPage ("
						+ stopWatch.getTotalTimeMillis() + " ms)");
			}
		}

		return page;
	}

	/**
	 * Find page by its URL. The url and publication id are specified. No
	 * security available; the method will fail if a SecurityFilter is
	 * configured on the factory.
	 * 
	 * @return
	 * @throws ItemNotFoundException
	 */
	public Page findPageByUrl(String url, int publicationId)
			throws ItemNotFoundException {

		Page page = null;
		try {
			page = this.findGenericPageByUrl(url, publicationId, null);
		} catch (NotAuthorizedException e) {
			// can be ignored
			logger.warn("unexpected NotAuthorizedException: no SecurityFilter is configured");
		}

		return page;
	}

	/**
	 * Get a page by the url and publication id.
	 * 
	 * @return the page
	 * @throws NotAuthorizedException
	 *             if not allowed to get the page.
	 * @throws ItemNotFoundException
	 *             if no page found.
	 */
	@Override
	public Page findPageByUrl(String url, int publicationId,
			RequestContext context) throws ItemNotFoundException,
			NotAuthorizedException {
		return this.findGenericPageByUrl(url, publicationId, context);
	}

	/**
	 * This is just a intermediate method to avoid naming conflicts with the
	 * simpleFactory.
	 */
	private Page findGenericPageByUrl(String url, int publicationId,
			RequestContext context) throws ItemNotFoundException,
			NotAuthorizedException {

		if (context == null && securityFilterPresent()) {
			throw new RuntimeException(
					"use of findPageByUrl is not allowed when a SecurityFilter is set");
		}

		StopWatch stopWatch = null;
		if (logger.isDebugEnabled()) {
			logger.debug("Enter findGenericPageByUrl with url:" + url
					+ " and publicationId: " + publicationId);
			stopWatch = new StopWatch("findGenericPageByUrl");
			stopWatch.start();
		}

		String cacheKey = publicationId + "-" + url;
		GenericPage page = (GenericPage) getCacheAgent().loadFromLocalCache(
				cacheKey);
		if (page == null) {
			try {
				// it is not necessary to supply a context since we are 
				// not using any filters anyway
				Page simplePage = simplePageFactory.findPageByUrl(url, publicationId);
				
				page = getGenericFromSimplePage(simplePage);
				TCMURI tcmUri = new TCMURI(page.getId());
				
				ItemDAO pageMetaDao = (ItemDAO) StorageManagerFactory.getDAO(tcmUri.getPublicationId(), StorageTypeMapping.PAGE_META);
				PageMeta pageMeta = (PageMeta) pageMetaDao.findByPrimaryKey(tcmUri.getPublicationId(), tcmUri.getItemId());

				page.setNativeMetadata(pageMeta);
				page.setSource(simplePage.getSource());

				try {
					// run only the filters where the result is allowed to be
					// cached.
					doFilters(page, context, BaseFilter.RunPhase.BeforeCaching);
				} catch (FilterException e) {
					logger.warn("Error in filter. ", e);
					throw new RuntimeException(e);
				}

				getCacheAgent().storeInCache(cacheKey, page, (ItemMeta) pageMeta);

			} catch (ParseException e) {
				logger.error("ParseException when searching for page: " + url,
						e);
				throw new RuntimeException(e);
			} catch (StorageException e) {
				logger.error(
						"StorageException when searching for page: " + url, e);
				throw new RuntimeException(e);
			}
		}

		try {
			// run only the filters where the result is not allowed to be
			// cached.
			doFilters(page, context, BaseFilter.RunPhase.AfterCaching);
		} catch (FilterException e) {
			logger.error("Error in filter. ", e);
			throw new RuntimeException(e);
		} finally {
			if (logger.isDebugEnabled()) {
				stopWatch.stop();
				logger.debug("Exit findGenericPageByUrl ("
						+ stopWatch.getTotalTimeMillis() + " ms)");
			}
		}

		return page;
	}


	public GenericPage getGenericFromSimplePage(Page simplePage)
			throws ItemNotFoundException {
		if (simplePage == null) {
			if (logger.isDebugEnabled()) {
				logger.debug("SimplePage is empty");
			}
			throw new ItemNotFoundException("Simple page not found");
		}
		return getPageFromSource(simplePage.getSource());
	}
	public GenericPage getPageFromSource(String source) {
		StopWatch stopWatch = null;
		try {
			if (logger.isDebugEnabled()) {
				stopWatch = new StopWatch();
				stopWatch.start("getPageFromSource");
			}
			return (GenericPage) this.getSerializer().deserialize(source, GenericPage.class);	
		} catch (Exception e) {
			logger.error("Exception when deserializing page: ", e);
			throw new RuntimeException(e);
		} finally {
			if (logger.isDebugEnabled()) {
				stopWatch.stop();
				logger.debug("Deserialization of page took: "
						+ stopWatch.getLastTaskTimeMillis() + " ms");
			}
		}

	}
}