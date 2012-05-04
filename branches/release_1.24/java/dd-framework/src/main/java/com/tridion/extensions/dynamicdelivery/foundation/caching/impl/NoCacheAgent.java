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
package com.tridion.extensions.dynamicdelivery.foundation.caching.impl;

import java.util.Collection;

import com.tridion.dcp.ComponentPresentation;
import com.tridion.extensions.dynamicdelivery.foundation.caching.Cachable;
import com.tridion.extensions.dynamicdelivery.foundation.caching.CacheAgent;
import com.tridion.storage.ComponentMeta;
import com.tridion.storage.ItemMeta;
import com.tridion.storage.PageMeta;
import com.tridion.taxonomies.Keyword;

public class NoCacheAgent implements CacheAgent {

    @Override
    public Object loadFromLocalCache(String key) {

        // TODO Auto-generated method stub
        return null;
    }

    @Override
    public void storeInCache(String key, Object ob,
            ComponentPresentation compPres) {

        // TODO Auto-generated method stub

    }

    @Override
    public void storeInCache(String key, Object ob, ComponentMeta componentMeta) {

        // TODO Auto-generated method stub

    }

    @Override
    public void storeInCache(String key, Object ob, PageMeta pageMeta) {

        // TODO Auto-generated method stub

    }

    @Override
    public void storeInCache(String key, Object ob, ItemMeta item) {

        // TODO Auto-generated method stub

    }

    @Override
    public void storeInCache(String key, Cachable ob, Collection<Cachable> deps) {

        // TODO Auto-generated method stub

    }

    @Override
    public void storeInCache(String key, Object ob, Keyword keyword) {

        // TODO Auto-generated method stub

    }

}
