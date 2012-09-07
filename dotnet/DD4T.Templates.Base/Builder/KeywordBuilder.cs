using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamic = DD4T.ContentModel;
using Tridion.ContentManager.ContentManagement;

namespace DD4T.Templates.Base.Builder
{
   public class KeywordBuilder
   {
      public static Dynamic.Keyword BuildKeyword(Keyword keyword)
      {
         Dynamic.Keyword dk = new Dynamic.Keyword();
         dk.Id = keyword.Id;
         dk.Title = keyword.Title;
         dk.Path = FindKeywordPath(keyword);
         dk.Description = keyword.Description;
         dk.Key = keyword.Key;
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
