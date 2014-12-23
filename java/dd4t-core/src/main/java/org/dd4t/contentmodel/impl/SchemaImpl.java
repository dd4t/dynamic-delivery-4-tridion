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
package org.dd4t.contentmodel.impl;

import org.dd4t.contentmodel.Schema;
import org.simpleframework.xml.Element;


public class SchemaImpl extends BaseRepositoryLocalItem implements Schema {
	private static final long serialVersionUID = 1113470472825303735L;
	@Element(name = "rootElement", required = false)
    private String rootElement;

    public String getRootElement() {
        return rootElement;
    }

    public void setRootElement(String rootElement) {
    
        this.rootElement = rootElement;
    }
}
