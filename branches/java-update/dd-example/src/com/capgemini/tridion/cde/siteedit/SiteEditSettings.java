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
package com.capgemini.tridion.cde.siteedit;

import java.util.HashMap;
import java.util.Map;

/**
 * This class is a settings container, suitable to be injected using Spring Dependency Injection.
 *
 * @author <a href="rogier.oudshoorn@">Rogier Oudshoorn</a>
 * @version $Revision$
 */
public class SiteEditSettings {
    /**
     * Inner (original) Mapping structure.
     */
    private Map<Integer, Map<String, Integer>> innerSettings;
    /**
     * Outer (parsed) Mapping structure.
     */
    private Map<Integer, SiteEditSetting> settings;

    /**
     * Default getter for inner Mapping.
     * @return Map
     */
    public Map<Integer,  Map<String, Integer>> getInnerSettings() {    
        return innerSettings;
    }

    /**
     * Setter for inner Mapping, will parse and create the new
     * outer Mapping upon calling.
     * 
     * @param innerSettings
     */
    public void setInnerSettings(Map<Integer,  Map<String, Integer>> innerSettings) {    
        this.innerSettings = innerSettings;
        settings = new HashMap<Integer, SiteEditSetting>();
        
        for(Integer key : innerSettings.keySet()){
            Map<String, Integer> submap = innerSettings.get(key);
            
            SiteEditSetting setting = new SiteEditSetting(
                    submap.containsKey("comp")?submap.get("comp"):key, 
                    submap.containsKey("page")?submap.get("comp"):key, 
                    submap.containsKey("publish")?submap.get("comp"):key                    
            );
            
            settings.put(key, setting);
        }
    }
    
    /**
     * Retrieve siteedit setting for a specific publication id.
     * 
     * @param pubid The publication ID
     * @return SiteEditSetting for given pubid
     */
    public SiteEditSetting getSetting(int pubid){
        return settings.get(pubid);
    }
    
    /**
     * Function indicates if a given publication has SiteEdit enabled.
     * 
     * @param pubid The Tridion Publication ID
     * @return true or false
     */
    public boolean hasPubSE(int pubid){
        return settings.containsKey(pubid);
    }
    
    /**
     * Inner class, used to represent settings for a specific
     * Tridion publication.
     *  
     * author <a href="rogier.oudshoorn">Rogier Oudshoorn</a>     
     * @version $Revision$
     */
    public class SiteEditSetting {
        /**
         * publication ID where the components are to be edited in
         */
        private int componentPub;
        /**
         * publication ID where the pages are to be edited in
         */
        private int pagePub;
        /**
         * publication ID where the pages and component need to be published to
         */
        private int publishPub;

        public int getComponentPub() {
        
            return componentPub;
        }

        public void setComponentPub(int componentPub) {
        
            this.componentPub = componentPub;
        }

        public int getPagePub() {
        
            return pagePub;
        }

        public void setPagePub(int pagePub) {
        
            this.pagePub = pagePub;
        }

        public int getPublishPub() {
        
            return publishPub;
        }

        public void setPublishPub(int publishPub) {
        
            this.publishPub = publishPub;
        }
        
        public SiteEditSetting(int comp, int page, int publish){
            componentPub = comp;
            pagePub = page;
            publishPub = publish;
        }
    }
}
