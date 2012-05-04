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

import java.util.ArrayList;
import java.util.List;
import com.tridion.extensions.dynamicdelivery.foundation.caching.CacheAgent;
import com.tridion.extensions.dynamicdelivery.foundation.caching.impl.DefaultBrokerCacheAgent;
import com.tridion.extensions.dynamicdelivery.foundation.contentmodel.Binary;
import com.tridion.extensions.dynamicdelivery.foundation.contentmodel.Item;
import com.tridion.extensions.dynamicdelivery.foundation.contentmodel.SimpleComponent;
import com.tridion.extensions.dynamicdelivery.foundation.contentmodel.SimplePage;
import com.tridion.extensions.dynamicdelivery.foundation.contentmodel.exceptions.NotAuthorizedException;
import com.tridion.extensions.dynamicdelivery.foundation.filters.Filter;
import com.tridion.extensions.dynamicdelivery.foundation.filters.FilterException;
import com.tridion.extensions.dynamicdelivery.foundation.filters.LinkResolverFilter;
import com.tridion.extensions.dynamicdelivery.foundation.filters.SecurityFilter;
import com.tridion.extensions.dynamicdelivery.foundation.filters.impl.BaseFilter;
import com.tridion.extensions.dynamicdelivery.foundation.filters.impl.DefaultLinkResolverFilter;
import com.tridion.extensions.dynamicdelivery.foundation.request.RequestContext;
import com.tridion.extensions.dynamicdelivery.foundation.serializers.Serializer;
import com.tridion.extensions.dynamicdelivery.foundation.serializers.impl.DefaultSerializer;

/**
 * Base class for all factories. All factories have a list of filters and a
 * default cache agent.
 * 
 * @author bjornl
 * 
 */
public abstract class BaseFactory {

	private List<Filter> filters;
	private CacheAgent cacheAgent;
	private Serializer serializer;

	/**
	 * Get the filters. If no filters are set the defaults are instantiated and
	 * returned.
	 */
	public List<Filter> getFilters() {
		if (filters == null) {
			this.filters = new ArrayList<Filter>();
			// add the default filters if no are specified.
			LinkResolverFilter linkResolver = new DefaultLinkResolverFilter();
			this.filters.add(linkResolver);
			// note: Security should not be a default setting, since most of our
			// customers do not use it.
		}
		return filters;
	}

	/**
	 * Set the filters
	 */
	public void setFilters(List<Filter> filters) {
		this.filters = new ArrayList<Filter>();
		// adds the filters to the list and makes sure that any securityFilter
		// is at the start of the list (so security is checked before resolving links)
		for (Filter filter : filters) {
			if (filter instanceof SecurityFilter) {
				this.filters.add(0, filter);
			} else {
				this.filters.add(filter);
			}
		}
	}

	/**
	 * Get the cache agent. If no agents is set the default is instantiated and
	 * returned.
	 */
	public CacheAgent getCacheAgent() {
		if (cacheAgent == null) {
			this.cacheAgent = new DefaultBrokerCacheAgent();
		} 
		return cacheAgent;
	}

	/**
	 * Set the cache agent.
	 */
	public void setCacheAgent(CacheAgent cacheAgent) {
		this.cacheAgent = cacheAgent;
	}

	/**
	 * Runs all the filters on an item. If the cachingAllowed is true it will
	 * only run the filters where the result is allowed to be cached.
	 * 
	 * @param item
	 * @param context
	 * @param cachingAllowed
	 * @throws FilterException
	 * @throws NotAuthorizedException
	 */
	public void doFilters(Item item, RequestContext context,
			BaseFilter.RunPhase runPhase) throws FilterException,
			NotAuthorizedException {
		if (item != null) {

			for (Filter filter : getFilters()) {
				// if runphase is Both run all filters
				if (runPhase == BaseFilter.RunPhase.Both) {
					this.doFilter(filter, item, context);
				}
				// if runphase is BeforeCaching only run filters which are set
				// to caching allowed.
				else if (runPhase == BaseFilter.RunPhase.BeforeCaching
						&& filter.getCachingAllowed()) {
					this.doFilter(filter, item, context);
				}
				// if runphase is AfterCaching only run filters which are set
				// to caching not allowed.
				else if (runPhase == BaseFilter.RunPhase.AfterCaching
						&& !filter.getCachingAllowed()) {
					this.doFilter(filter, item, context);
				}
			}
		}
	}

	private void doFilter(Filter filter, Item item, RequestContext context)
			throws FilterException, NotAuthorizedException {

		// link resolving is not needed for the simple objects or
		// binary
		if (filter instanceof LinkResolverFilter) {
			if (item instanceof SimplePage || item instanceof SimpleComponent
					|| item instanceof Binary) {
				return;
			}
		}

		filter.doFilter(item, context);
	}

	/**
	 * Get the serializer. The serializer is only used by the Generic factories.
	 * 
	 * @return
	 */
	public Serializer getSerializer() {
		if (serializer == null) {
			this.serializer = new DefaultSerializer();
		}
		return serializer;
	}

	/**
	 * Set the serializer
	 * 
	 * @param serializer
	 */
	public void setSerializer(Serializer serializer) {
		this.serializer = serializer;
	}

	protected boolean securityFilterPresent() {
		for (Filter filter : this.getFilters()) {
			if (filter instanceof SecurityFilter) {
				return true;
			}
		}
		return false;
	}
}