using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using DD4T.ContentModel;
using System.Web.Mvc;

namespace DD4T.Mvc.Html
{
    public interface IComponentPresentationRenderer
    {
        MvcHtmlString ComponentPresentations(IPage tridionPage, HtmlHelper htmlHelper, string[] includeComponentTemplate, string includeSchema);
    }
}
