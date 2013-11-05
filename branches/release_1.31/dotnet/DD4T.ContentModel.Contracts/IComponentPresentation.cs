using System.Collections.Generic;

namespace DD4T.ContentModel
{
    public interface IComponentPresentation 
    {
        IComponent Component { get; }
        IComponentTemplate ComponentTemplate { get; }
        IPage Page { get; set; }
        bool IsDynamic { get; set; }
        string RenderedContent { get; }
        int OrderOnPage { get; set; }
        IList<ICondition> Conditions { get; } 
    }
}
