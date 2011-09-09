///   
/// Copyright 2011 Capgemini & SDL
///
///   Licensed under the Apache License, Version 2.0 (the "License");
///   you may not use this file except in compliance with the License.
///   You may obtain a copy of the License at
///
///       http://www.apache.org/licenses/LICENSE-2.0
///
///   Unless required by applicable law or agreed to in writing, software
///   distributed under the License is distributed on an "AS IS" BASIS,
///   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
///   See the License for the specific language governing permissions and
///   limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tridion.ContentManager.Templating.Assembly;
using Tridion.ContentManager.Templating;
using Tridion.Extensions.DynamicDelivery.Templates.Builder;

namespace Tridion.Extensions.DynamicDelivery.Templates
{
   public abstract class BaseTemplate : ITemplate
   {
      protected TemplatingLogger log = TemplatingLogger.GetLogger(typeof(BaseTemplate));
      protected Package package;
      protected Engine engine;
      private BuildManager buildManager = new BuildManager();
      
       public BuildManager manager {
           get { return buildManager; }
           set { buildManager = value; }
       }

      public abstract void Transform(Engine engine, Package package);

      protected bool PackageHasValue(Package package, string key)
      {
         foreach (KeyValuePair<string, Item> kvp in package.GetEntries())
         {
            if (kvp.Key.Equals(key))
            {
               return true;
            }
         }
         return false;
      }

   }
}
