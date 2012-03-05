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
package com.tridion.extensions.dynamicdelivery.foundation.contentmodel.impl;

import org.simpleframework.xml.Element;

import com.tridion.extensions.dynamicdelivery.foundation.contentmodel.OrganizationalItem;
import com.tridion.extensions.dynamicdelivery.foundation.contentmodel.Publication;
import com.tridion.extensions.dynamicdelivery.foundation.contentmodel.RepositoryLocalItem;

/**
 * Base class for all tridion items except for publications and organizational items
 * 
 * @author bjornl
 * 
 */
public abstract class BaseRepositoryLocalItem extends BaseItem implements
		RepositoryLocalItem {

	@Element(name = "publication", required = false)
	private Publication publication;

	@Element(name = "folder", required = false)
	private OrganizationalItem organizationalItem;

	/**
	 * Get the organizational item
	 */
	@Override
	public OrganizationalItem getOrganizationalItem() {
		return organizationalItem;
	}

	/**
	 * Set the organizational item
	 */
	@Override
	public void setOrganizationalItem(OrganizationalItem organizationalItem) {
		this.organizationalItem = organizationalItem;
	}

	/**
	 * Get the publication
	 */
	@Override
	public Publication getPublication() {
		return publication;
	}

	/**
	 * Set the publication
	 */
	@Override
	public void setPublication(Publication publication) {
		this.publication = publication;
	}
}