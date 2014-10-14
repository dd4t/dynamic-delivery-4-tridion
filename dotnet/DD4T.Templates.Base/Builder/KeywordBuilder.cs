using System.Collections.Generic;
using Dynamic = DD4T.ContentModel;
using Tridion.ContentManager.ContentManagement;

namespace DD4T.Templates.Base.Builder
{
   public class KeywordBuilder
   {       
       public static Dynamic.Keyword BuildKeyword(Keyword keyword)
       {
           return BuildKeyword(keyword, 0, false, false, null);
       }

       public static Dynamic.Keyword BuildKeyword(Keyword keyword, int linkLevels, bool resolveWidthAndHeight, bool publishEmptyFields)
       {
           return BuildKeyword(keyword, linkLevels, resolveWidthAndHeight, publishEmptyFields, null);
       }

      public static Dynamic.Keyword BuildKeyword(Keyword keyword, int linkLevels, bool resolveWidthAndHeight, bool publishEmptyFields, BuildManager manager)
      {
         Dynamic.Keyword dk = new Dynamic.Keyword();
         dk.Id = keyword.Id;
         dk.Title = keyword.Title;
         dk.Path = FindKeywordPath(keyword);
         dk.Description = keyword.Description;
         dk.Key = keyword.Key;
         dk.TaxonomyId = keyword.OrganizationalItem.Id;

         if (keyword.MetadataSchema != null)
         {
             if (manager != null)
             {
                 var tcmFields = new Tridion.ContentManager.ContentManagement.Fields.ItemFields(keyword.Metadata, keyword.MetadataSchema);
                 dk.MetadataFields = manager.BuildFields(tcmFields, linkLevels, resolveWidthAndHeight, publishEmptyFields);
             }
         }

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
