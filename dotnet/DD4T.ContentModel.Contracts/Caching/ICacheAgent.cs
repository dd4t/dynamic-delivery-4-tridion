using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DD4T.ContentModel.Contracts.Factories;

namespace DD4T.ContentModel.Contracts.Caching
{
    public delegate DateTime GetLastPublishDate(string key, object cachedItem);

    public interface ICacheAgent
    {

        /// <summary>
        /// Load object from the cache
        /// </summary>
        /// <param name="key">Identification of the object</param>
        /// <returns></returns>
        object Load(String key);

        /// <summary>
        /// Store any object in the cache
        /// </summary>
        /// <param name="key">Identification of the item</param>
        /// <param name="item">The IItem to store (can be a page, component, schema, etc) </param>
        void Store(String key, object item);

        /// <summary>
        /// Store any object in the cache
        /// </summary>
        /// <param name="key">Identification of the item</param>
        /// <param name="item">The IItem to store (can be a page, component, schema, etc) </param>
        /// <param name="dependOnItems">List of items on which the current item depends. If one of these items is dropped from the cache, the current item must also be dropped.</param>
        void Store(String key, object item, List<string> dependOnItems);

        /// <summary>
        /// Store an object belonging to a specific region in the cache 
        /// </summary>
        /// <param name="key">Identification of the item</param>
        /// <param name="region">Identification of the region (any string)</param>
        /// <param name="item">The IItem to store (can be a page, component, schema, etc) </param>
        void Store(string key, string region, object item);

        /// <summary>
        /// Store an object belonging to a specific region in the cache with a dependency on other items in the cache.
        /// </summary>
        /// <param name="key">Identification of the item</param>
        /// <param name="region">Identification of the region (any string)</param>
        /// <param name="item">The IItem to store (can be a page, component, schema, etc) </param>
        /// <param name="dependOnItems">List of items on which the current item depends</param>
        void Store(string key, string region, object item, List<string> dependOnItems);

        GetLastPublishDate GetLastPublishDateCallBack { get; set; }

    }
}
