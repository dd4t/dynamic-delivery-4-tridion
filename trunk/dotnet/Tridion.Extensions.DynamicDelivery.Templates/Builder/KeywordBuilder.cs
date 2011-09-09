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
using Dynamic = Tridion.Extensions.DynamicDelivery.ContentModel;
using Tridion.ContentManager.ContentManagement;

namespace Tridion.Extensions.DynamicDelivery.Templates.Builder
{
   public class KeywordBuilder
   {
      public static Dynamic.Keyword BuildKeyword(Keyword keyword)
      {
         Dynamic.Keyword dk = new Dynamic.Keyword();
         dk.Id = keyword.Id;
         dk.Title = keyword.Title;
         dk.Path = FindKeywordPath(keyword);
         dk.TaxonomyId = keyword.OrganizationalItem.Id;
         return dk;
      }
      private static string FindKeywordPath(Keyword keyword)
      {
         IList<Keyword> parentKeywords = keyword.ParentKeywords;
         string path = @"\" + keyword.Title;
         while (parentKeywords.Count > 0) {
            path = @"\" + parentKeywords[0].Title + path;
            parentKeywords = parentKeywords[0].ParentKeywords;
         }
         return @"\" + keyword.OrganizationalItem.Title + path;
      }
   }
}
