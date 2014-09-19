using DD4T.ContentModel;
using System.Collections.Generic;
using System.Web.Mvc;

namespace DD4T.Mvc.Html
{
    public interface IComponentPresentationRenderer
    {
        MvcHtmlString ComponentPresentations(IPage tridionPage, HtmlHelper htmlHelper, string[] includeComponentTemplate, string includeSchema);
        MvcHtmlString ComponentPresentations(HtmlHelper htmlHelper, IEnumerable<IComponentPresentation> componentPresentations);
    }
}
