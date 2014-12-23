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
package org.dd4t.core.caching;

import java.io.Serializable;

/**
 * Interface any object Cachable by the CacheAgent class must implement.
 * Its functions are used as such:
 * - After caching, notifyCached(key, realm) is called by the Cache Agent
 * - When placing a dependency to this object, the CacheAgent calls 
 * 	 getCacheRealm and getCacheKey, expecting the same values back.
 * 
 * @author Rogier Oudshoorn, Capgemini
 *
 */
public interface Cachable extends Serializable {
	public void notifyCached(String key, String realm);
	
	public String getCacheRealm();
	
	public String getCacheKey();
}