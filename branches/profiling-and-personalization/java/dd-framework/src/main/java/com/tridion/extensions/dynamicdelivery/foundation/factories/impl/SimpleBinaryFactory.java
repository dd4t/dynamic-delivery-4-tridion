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
import java.util.LinkedList;

import org.apache.log4j.Logger;
import org.springframework.util.StopWatch;

import com.tridion.broker.StorageException;
import com.tridion.data.BinaryData;
import com.tridion.extensions.dynamicdelivery.foundation.contentmodel.Binary;
import com.tridion.extensions.dynamicdelivery.foundation.contentmodel.Component;
import com.tridion.extensions.dynamicdelivery.foundation.contentmodel.SimpleBinary;
import com.tridion.extensions.dynamicdelivery.foundation.contentmodel.exceptions.ItemNotFoundException;
import com.tridion.extensions.dynamicdelivery.foundation.contentmodel.exceptions.NotAuthorizedException;
import com.tridion.extensions.dynamicdelivery.foundation.contentmodel.impl.SimpleBinaryImpl;
import com.tridion.extensions.dynamicdelivery.foundation.factories.BinaryFactory;
import com.tridion.extensions.dynamicdelivery.foundation.factories.ComponentFactory;
import com.tridion.extensions.dynamicdelivery.foundation.filters.Filter;
import com.tridion.extensions.dynamicdelivery.foundation.filters.FilterException;
import com.tridion.extensions.dynamicdelivery.foundation.request.RequestContext;
import com.tridion.extensions.dynamicdelivery.foundation.util.HomeUtils;
import com.tridion.extensions.dynamicdelivery.foundation.util.TridionUtils;
import com.tridion.storage.BinaryMeta;
import com.tridion.storage.BinaryVariant;
import com.tridion.storage.ComponentMeta;
import com.tridion.storage.ItemMeta;
import com.tridion.storage.StorageManagerFactory;
import com.tridion.storage.dao.BinaryVariantDAO;
import com.tridion.storage.dao.ItemDAO;
import com.tridion.util.TCDURI;
import com.tridion.util.TCMURI;

/**
 * Factory for SimpleBinary objects. Note the simple binary objects should not
 * be cached because they are already cached in the broker.
 * 
 * @author bjornl
 * 
 */

public class SimpleBinaryFactory extends BaseFactory implements BinaryFactory {

	private static Logger logger = Logger.getLogger(SimpleBinaryFactory.class);
	
	
	/**
	 * Get a binary by the uri.
	 * 
	 * @return the binary
	 * @throws NotAuthorizedException
	 *             if not allowed to get the item.
	 * @throws ItemNotFoundException
	 *             if no item is found.
	 * @throws ParseException 
	 * @throws StorageException 
	 */
	@Override
	public SimpleBinary getBinary(String uri, RequestContext context)
			throws ItemNotFoundException, NotAuthorizedException, StorageException, ParseException {
		SimpleBinary simple = (SimpleBinary) getCacheAgent()
				.loadFromLocalCache(uri);

		if (simple == null) {
			simple = getBinaryFromUri(uri, context);
			getCacheAgent().storeInCache(uri, simple,
					(ComponentMeta) simple.getNativeMetadata());
		}

		return simple;
	}

	@Override
	public void retrieveBinaryData(Binary binary) {
		BinaryData binaryData;
		BinaryMeta binaryMeta = ((SimpleBinary)binary).getNativeBinaryMetadata();
		try {
			if (logger.isDebugEnabled()) {
				logger.debug("looking for binary content using id " + binaryMeta.getItemId() + ", publication id " + binaryMeta.getPublicationId());
			}
			
			
			binaryData = (BinaryData) HomeUtils.getInstance().getBinaryDao(binaryMeta.getPublicationId()).findByPrimaryKey(binaryMeta.getPublicationId(),(int) binaryMeta.getItemId());
//			binaryData = HomeUtils
//					.getInstance()
//					.getBinaryHome()
//					.findByPrimaryKey(binaryMeta.getPublicationId(),
//							(int) binaryMeta.getId(),binaryMeta.getVariantId());
			binary.setBinaryData(binaryData);
		} catch (StorageException e) {
			logger.error("Storage exception when searching for binary: "
					+ binary.getId(), e);
			throw new RuntimeException(e);
		}
	}

	public void retrieveBinaryMetaData(SimpleBinary binary) {
		BinaryMeta binaryMetaData;
		try {
			logger.debug("looking for binaryMetaData");
			
			TCMURI tcmUri = new TCMURI(binary.getId());
			logger.debug("tcmuri = " + tcmUri.toString());
	
			
			
			binaryMetaData = (BinaryMeta) HomeUtils.getInstance().getBinaryMetaDao(tcmUri.getPublicationId()).findBinaryByPrimaryKey(tcmUri.getPublicationId(), tcmUri.getItemId());
			if (binaryMetaData == null) {
				logger.warn("cannot find binary meta for uri " + tcmUri.toString());
				return;
			}
			logger.debug("found binaryMetaData " + binaryMetaData);
			binary.setNativeBinaryMetadata(binaryMetaData);
		} catch (StorageException e) {
			logger.error("Storage exception when searching for binary: "
					+ binary.getId(), e);
			throw new RuntimeException(e);
		} catch (ParseException e) {
			logger.error("Parse exception when parsing TCDURI: "
					+ binary.getId(), e);
			throw new RuntimeException(e);
		}
	}
	
	/**
	 * Get a binary by the url and publication id.
	 * 
	 * @return the binary
	 * @throws NotAuthorizedException
	 *             if not allowed to get the item.
	 * @throws ItemNotFoundException
	 *             if no item is found.
	 * @throws ParseException 
	 * @throws StorageException 
	 */
	@Override
	public SimpleBinary findBinaryByUrl(String url, int publicationId,
			RequestContext context) throws ItemNotFoundException,
			NotAuthorizedException, StorageException, ParseException {
		String cacheKey = publicationId + "-" + url;
		SimpleBinary simpleBinary = (SimpleBinary) getCacheAgent()
				.loadFromLocalCache(cacheKey);

		if (simpleBinary == null) {
			// find out tcmUri of the mm component
			BinaryVariant binaryVariant;
			try {
				binaryVariant = getBinaryVariantByUrl(publicationId, url);
			} catch (StorageException e) {
				logger.warn("storageException while trying to retrieve variant by url '" + url + "'",e);
				throw new ItemNotFoundException(e);
			}
			BinaryMeta binaryMeta = binaryVariant.getBinaryMeta();
			String uri = new TCMURI(binaryMeta.getPublicationId(),
					(int) binaryMeta.getItemId(), 16, 0).toString();
			simpleBinary = getBinaryFromUri(uri, context);
			simpleBinary.setNativeBinaryMetadata(binaryMeta);
			getCacheAgent().storeInCache(cacheKey, simpleBinary,
					(ComponentMeta) simpleBinary.getNativeMetadata());
		}
		return simpleBinary;
	}

	private SimpleBinary getBinaryFromUri(String uri, RequestContext context)
			throws ItemNotFoundException, NotAuthorizedException, ParseException, StorageException {

		StopWatch stopWatch = null;
		if (logger.isDebugEnabled()) {
			logger.debug("Enter getBinaryFromUri with uri: " + uri);
			stopWatch = new StopWatch("getBinaryFromUri");
			stopWatch.start();
		}

		SimpleBinary simpleBinary;
		// retrieve the SimpleComponent for this mm component
		ComponentFactory compFactory = new SimpleComponentFactory();
		// make sure there are no filters set, we need only the simple component
		// factory logic itself!
		compFactory.setFilters(new LinkedList<Filter>());
		Component mmComp = compFactory.getComponent(uri, context);

		TCMURI tcmUri = new TCMURI(uri);
		ItemMeta binaryMeta = HomeUtils.getInstance().getBinaryMetaDao(tcmUri.getPublicationId()).findByPrimaryKey(tcmUri.getPublicationId(), tcmUri.getItemId());
		
		simpleBinary = new SimpleBinaryImpl();
		simpleBinary.setNativeMetadata(mmComp.getNativeMetadata());
		simpleBinary.setFactory(this);
		simpleBinary.setId(TridionUtils.createUri(mmComp.getNativeMetadata())
				.toString());
		simpleBinary.setTitle(mmComp.getNativeMetadata().getTitle());

		try {
			doFilters(simpleBinary, context, null);
		} catch (FilterException e) {
			logger.warn("Error in filter. ", e);
			throw new RuntimeException(e);
		} finally {
			if (logger.isDebugEnabled()) {
				stopWatch.stop();
				logger.debug("Exit getBinaryFromUri ("
						+ stopWatch.getTotalTimeMillis() + " ms)");
			}
		}

		return simpleBinary;
	}

	/**
	 * Returns a BinaryMeta object or throws ItemNotFoundException if not found.
	 * 
	 * @param pubId
	 * @param url
	 * @return
	 * @throws ItemNotFoundException
	 * @throws StorageException 
	 */
	private BinaryVariant getBinaryVariantByUrl(int pubId, String url)
			throws ItemNotFoundException, StorageException {

		StopWatch stopWatch = null;
		if (logger.isDebugEnabled()) {
			logger.debug("Enter getBinaryMetaByUrl with url: " + url
					+ " and publicationId: " + pubId);
			stopWatch = new StopWatch("getBinaryMetaByUrl");
			stopWatch.start();
		}

		BinaryVariantDAO binaryVariantDAO = HomeUtils.getInstance().getBinaryVariantDao(pubId);
		
		BinaryVariant binaryVariant = null;
		try {
			binaryVariant = binaryVariantDAO.findByURL(pubId, url);
			if (binaryVariant == null) {
				if (logger.isDebugEnabled()) {
					logger.debug("Cannot find BinaryVariant in Tridion.");
				}
				throw new ItemNotFoundException("No binary found with url :"
						+ url);
			}
			if (logger.isDebugEnabled()) {
				logger.debug("Returning BinaryMeta " + binaryVariant);
			}
		} catch (StorageException e) {
			logger.error("StorageException caught throw runtime exception");
			throw new RuntimeException(e);
		} finally {
			if (logger.isDebugEnabled()) {
				stopWatch.stop();
				logger.debug("Exit getBinaryMetaByUrl ("
						+ stopWatch.getTotalTimeMillis() + " ms)");
			}
		}

		return binaryVariant;
	}

	@Override
	public SimpleBinary getBinary(String uri) throws ItemNotFoundException, StorageException, ParseException {
		if (securityFilterPresent()) {
			throw new RuntimeException(
					"use of getBinary(String) not allowed when a SecurityFilter is set");
		}

		SimpleBinary simple = (SimpleBinary) getCacheAgent()
				.loadFromLocalCache(uri);

		if (simple == null) {
			try {
				simple = getBinaryFromUri(uri, null);
				getCacheAgent().storeInCache(uri, simple,
						(ComponentMeta) simple.getNativeMetadata());
			} catch (NotAuthorizedException e) {
				// this can be ignored since there is no SecurityFilter
				// configured anyway
				logger.warn("unexpected NotAuthorizedException: no SecurityFilter is configured");
			}
		}

		return simple;
	}

	@Override
	public SimpleBinary findBinaryByUrl(String url, int publicationId)
			throws ItemNotFoundException, StorageException, ParseException {

		if (securityFilterPresent()) {
			throw new RuntimeException(
					"use of findBinaryByUrl(String,int) not allowed when a SecurityFilter is set");
		}

		String cacheKey = publicationId + "-" + url;
		SimpleBinary simpleBinary = (SimpleBinary) getCacheAgent()
				.loadFromLocalCache(cacheKey);

		if (simpleBinary == null) {
			// find out tcmUri of the mm component
		        BinaryVariant binaryVariant = getBinaryVariantByUrl(publicationId, url);
		    
			BinaryMeta binaryMeta = binaryVariant.getBinaryMeta();
			String uri = new TCMURI(binaryMeta.getPublicationId(),
					binaryMeta.getItemId(), 16, 0).toString();
			try {
				simpleBinary = getBinaryFromUri(uri, null);
				simpleBinary.setNativeBinaryMetadata(binaryMeta);
				getCacheAgent().storeInCache(cacheKey, simpleBinary,
						(ComponentMeta) simpleBinary.getNativeMetadata());
			} catch (NotAuthorizedException e) {
				// this can be ignored since there is no SecurityFilter
				// configured anyway
				logger.warn("unexpected NotAuthorizedException: no SecurityFilter is configured");
			}
		}
		return simpleBinary;
	}

}
