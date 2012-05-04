using System.Collections.Generic;
namespace DD4T.ContentModel
{
    public interface IKeyword : IRepositoryLocal
    {
        string Path { get; }
        string TaxonomyId { get; }
        IList<IKeyword> ParentKeywords { get;}        
    }
}
