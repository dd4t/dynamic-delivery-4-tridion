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

import com.tridion.extensions.dynamicdelivery.foundation.contentmodel.Component;
import com.tridion.extensions.dynamicdelivery.foundation.contentmodel.ComponentPresentation;
import com.tridion.extensions.dynamicdelivery.foundation.contentmodel.ComponentTemplate;

/**
 * Class representing a component presentation which holds a component template and a component.
 * 
 * @author bjornl
 * 
 */
public class ComponentPresentationImpl implements ComponentPresentation {
	@Element(name = "component", required = false)
	private Component component;
	@Element(name = "componentTemplate", required = false)
	private ComponentTemplate componentTemplate;

	@Element(name = "renderedContent", required = false)
	private String renderedContent;
	
	/**
	 * Get the component
	 * 
	 * @return the component
	 */
	public Component getComponent() {
		return component;
	}

	/**
	 * Set the component
	 * 
	 * @param component
	 */
	public void setComponent(Component component) {
		this.component = component;
	}

	/**
	 * Get the component template
	 * 
	 * @return the component template
	 */
	public ComponentTemplate getComponentTemplate() {
		return componentTemplate;
	}

	/**
	 * Set the component template
	 * 
	 * @param componentTemplate
	 */
	public void setComponentTemplate(ComponentTemplate componentTemplate) {
		this.componentTemplate = componentTemplate;
	}

	public void setRenderedContent(String renderedContent) {
		this.renderedContent = renderedContent;
	}

	public String getRenderedContent() {
		return renderedContent;
	}
}
