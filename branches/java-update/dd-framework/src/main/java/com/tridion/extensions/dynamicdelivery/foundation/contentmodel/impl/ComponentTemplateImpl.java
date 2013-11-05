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

import java.util.Map;

import org.simpleframework.xml.Element;
import org.simpleframework.xml.ElementMap;

import com.tridion.extensions.dynamicdelivery.foundation.contentmodel.ComponentTemplate;
import com.tridion.extensions.dynamicdelivery.foundation.contentmodel.Field;

public class ComponentTemplateImpl extends BaseRepositoryLocalItem implements ComponentTemplate {
	
	@Element(name = "outputFormat", required = false)
	private String outputFormat;
	@ElementMap(name = "metadata", keyType = String.class, valueType = Field.class, entry = "item", required = false)
	private Map<String, Field> metadata;

	@Override
	public Map<String, Field> getMetadata() {
		return this.metadata;
	}

	@Override
	public void setMetadata(Map<String, Field> metadata) {
		this.metadata = metadata;
	}

	/**
	 * Get the output format
	 * @return 
	 */
	public String getOutputFormat() {
		return outputFormat;
	}

	/**
	 * Set the output format
	 * @param outputFormat
	 */
	public void setOutputFormat(String outputFormat) {
		this.outputFormat = outputFormat;
	}	
}
