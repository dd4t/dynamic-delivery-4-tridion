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
import com.tridion.data.CharacterData;
import com.tridion.extensions.dynamicdelivery.foundation.contentmodel.Page;
import com.tridion.extensions.dynamicdelivery.foundation.contentmodel.SimplePage;
import com.tridion.extensions.dynamicdelivery.foundation.contentmodel.exceptions.ItemNotFoundException;
import com.tridion.extensions.dynamicdelivery.foundation.contentmodel.exceptions.NotAuthorizedException;
import com.tridion.extensions.dynamicdelivery.foundation.contentmodel.impl.SimplePageImpl;
import com.tridion.extensions.dynamicdelivery.foundation.factories.PageFactory;
import com.tridion.extensions.dynamicdelivery.foundation.filters.FilterException;
import com.tridion.extensions.dynamicdelivery.foundation.filters.impl.BaseFilter;
import com.tridion.extensions.dynamicdelivery.foundation.request.RequestContext;
import com.tridion.extensions.dynamicdelivery.foundation.util.BrokerUtils;
import com.tridion.extensions.dynamicdelivery.foundation.util.HomeUtils;
import com.tridion.extensions.dynamicdelivery.foundation.util.IOUtils;
import com.tridion.extensions.dynamicdelivery.foundation.util.TridionUtils;
import com.tridion.storage.PageMeta;
import com.tridion.storage.StorageManagerFactory;
import com.tridion.storage.StorageTypeMapping;
import com.tridion.storage.dao.ItemDAO;
import com.tridion.storage.dao.PageDAO;
import com.tridion.util.TCMURI;

/**
 * Factory for SimplePage objects. Note the simple page objects should not be
 * cached because they are already cached in the broker.
 * 
 * @author bjornl
 * 
 */
public class SimplePageFactory extends BaseFactory implements PageFactory {

	private static Logger logger = Logger.getLogger(SimplePageFactory.class);

	/**
	 * Find page by its URL. The url and publication id are specified. No
	 * security available; the method will fail if a SecurityFilter is
	 * configured on the factory.
	 * 
	 * @return
	 * @throws ItemNotFoundException
	 */
	@Override
	public Page findPageByUrl(String url, int publicationId)
			throws ItemNotFoundException {

		SimplePage page = null;
		try {
			page = (SimplePage) this.findSimplePageByUrl(url, publicationId,
					null);
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

		return findSimplePageByUrl(url, publicationId, context);
	}

	private Page findSimplePageByUrl(String url, int publicationId,
			RequestContext context) throws ItemNotFoundException,
			NotAuthorizedException {

		if (context == null && securityFilterPresent()) {
			throw new RuntimeException(
					"use of findPageByUrl is not allowed when a SecurityFilter is set");
		}

		StopWatch stopWatch = null;
		if (logger.isDebugEnabled()) {
			logger.debug("Enter findSimplePageByUrl url: " + url
					+ " and publicationId: " + publicationId);
			stopWatch = new StopWatch("findSimplePageByUrl");
			stopWatch.start();
		}

		PageMeta pageMeta = null;
		Page page = null;
		try {
	                ItemDAO itemDAO = (ItemDAO) StorageManagerFactory.getDAO(publicationId, StorageTypeMapping.PAGE_META);
	                pageMeta = (PageMeta) itemDAO.findByPageURL(publicationId, url);	                       
			
		             if (logger.isDebugEnabled()) {
		                 stopWatch.stop();
		                        logger.debug("Got pageMeta in "+ stopWatch.getTotalTimeMillis() + " ms");
		                        stopWatch.start();
		                }
			
			page = (Page) getPageFromMeta(pageMeta);

                        if (logger.isDebugEnabled()) {
                            stopWatch.stop();
                                   logger.debug("Got Page in "+ stopWatch.getTotalTimeMillis() + " ms");
                                   stopWatch.start();
                           }
                        
			try {
				// run all filters regardless if they are allowed to be cached
				// or not
				doFilters(page, context, BaseFilter.RunPhase.Both);
			} catch (FilterException e) {
				logger.error("Error in filter. ", e);
				throw new RuntimeException(e);
			}

                        if (logger.isDebugEnabled()) {
                            stopWatch.stop();
                                   logger.debug("Ran filters in "+ stopWatch.getTotalTimeMillis() + " ms");
                           }
                        stopWatch.start();
                        
		} catch (StorageException e) {
			logger.error("Storage exception when searching for page: " + url, e);
			throw new RuntimeException(e);
		}

		if (logger.isDebugEnabled()) {
			stopWatch.stop();
			logger.debug("Exit findSimplePageByUrl ("
					+ stopWatch.getTotalTimeMillis() + " ms)");
		}

		return page;
	}

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

		SimplePage page = null;
		try {
			page = (SimplePage) this.getSimplePage(uri, null);
		} catch (NotAuthorizedException e) {
			// can be ignored
			logger.warn("unexpected NotAuthorizedException: no SecurityFilter is configured");
		}

		return page;
	}

	/**
	 * Get the page by the uri
	 * 
	 * @return the page
	 * @throws ItemNotFoundException
	 *             if not item found
	 * @throws NotAuthorizedException
	 *             if the user is not authorixed to get the item
	 */
	@Override
	public Page getPage(String uri, RequestContext context)
			throws ItemNotFoundException, NotAuthorizedException {

		return getSimplePage(uri, context);
	}

	private Page getSimplePage(String uri, RequestContext context)
			throws ItemNotFoundException, NotAuthorizedException {

		if (context == null && securityFilterPresent()) {
			throw new RuntimeException(
					"use of getPage is not allowed when a SecurityFilter is set");
		}

		StopWatch stopWatch = null;
		if (logger.isDebugEnabled()) {
			logger.debug("Enter getSimplePage with uri: " + uri);
			stopWatch = new StopWatch("getSimplePage");
			stopWatch.start();
		}

		PageMeta pageMeta = null;
		Page page = null;
		try {
			TCMURI tcmUri = new TCMURI(uri);
			
			ItemDAO itemDAO = (ItemDAO) StorageManagerFactory.getDAO(tcmUri.getPublicationId(), StorageTypeMapping.PAGE_META);
			pageMeta = (PageMeta) itemDAO.findByPrimaryKey(tcmUri.getPublicationId(), tcmUri.getItemId());

			// before 2011:
//			pageMeta = HomeUtils
//					.getInstance()
//					.getPageMetaHome()
//					.findByPrimaryKey(tcmUri.getPublicationId(),
//							tcmUri.getItemId());

			page = (Page) getPageFromMeta(pageMeta);

			try {
				// run all filters regardless if they are allowed to be cached
				// or not
				doFilters(page, context, BaseFilter.RunPhase.Both);
			} catch (FilterException e) {
				logger.error("Error in filter. ", e);
				throw new RuntimeException(e);
			}

		} catch (ParseException e) {
			logger.error("Parse exception when searching for page: " + uri, e);
			throw new RuntimeException(e);
		} catch (StorageException e) {
			logger.error("Storage exception when searching for page: " + uri, e);
			throw new RuntimeException(e);
		}

		if (logger.isDebugEnabled()) {
			stopWatch.stop();
			logger.debug("Exit getSimplePage ("
					+ stopWatch.getTotalTimeMillis() + " ms)");
		}

		return page;
	}

	private SimplePage getPageFromMeta(PageMeta pageMeta)
			throws ItemNotFoundException {

		if (pageMeta == null) {
			throw new ItemNotFoundException("Page not found");
		}
		try {
			CharacterData data;
			
			PageDAO pageDAO = (PageDAO) StorageManagerFactory.getDAO(pageMeta.getPublicationId(), StorageTypeMapping.PAGE);
			
			data = pageDAO.findByPrimaryKey(pageMeta.getPublicationId(),
					pageMeta.getItemId());

// before 2011:
//			PageHome pageHome = HomeUtils.getInstance().getPageHome();
//			data = pageHome.findByPrimaryKey(pageMeta.getPublicationId(),
//					pageMeta.getId());

			if (data == null) {
				throw new ItemNotFoundException(
						"Page source not found (is your deployer configured correctly?)");
			}

			String s = IOUtils.convertStreamToString(data.getInputStream());
			SimplePage p = new SimplePageImpl();

			p.setNativeMetadata(pageMeta);
			p.setSource(s);
			p.setId(TridionUtils.createUri(pageMeta).toString());
			p.setTitle(pageMeta.getTitle());
			return p;
		} catch (Exception e) {
			logger.error("Exception when creating page from meta data ", e);
			throw new RuntimeException(e);
		}
	}
}