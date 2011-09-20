using System.ComponentModel.Composition;
using System.Web.Mvc;
using DD4T.ContentModel;

namespace DD4T.Mvc.Html
{
    public static class TridionHelper
    {
        public static MvcHtmlString RenderComponentPresentations(this HtmlHelper helper)
        {
            return RenderComponentPresentations(helper, null);
        }

        public static MvcHtmlString RenderComponentPresentations(this HtmlHelper helper, IComponentPresentationRenderer renderer)
        {
           return RenderComponentPresentations(helper, string.Empty, renderer);
        }

        public static MvcHtmlString RenderComponentPresentations(this HtmlHelper helper, string byComponentTemplate, IComponentPresentationRenderer renderer)
        {
            if (string.IsNullOrEmpty(byComponentTemplate))
                return RenderComponentPresentations(helper, new string[] { }, null, renderer);
            else
                return RenderComponentPresentations(helper, new [] {byComponentTemplate}, null, renderer);
        }

        public static MvcHtmlString RenderComponentPresentations(this HtmlHelper helper, string[] byComponentTemplate, IComponentPresentationRenderer renderer)
        {
            return RenderComponentPresentations(helper, byComponentTemplate, null, renderer);
        }

        public static MvcHtmlString RenderComponentPresentations(this HtmlHelper helper, string[] byComponentTemplate, string bySchema, IComponentPresentationRenderer renderer)
        {
            IComponentPresentationRenderer cpr = renderer;
            if (!(helper.ViewData.Model is IPage))
            {
                return new MvcHtmlString("<!-- RenderComponentPresentations can only be used if the model is an instance of IPage -->");
            }

            IPage tridionPage = helper.ViewData.Model as IPage;
            if (renderer == null)
                renderer = DefaultComponentPresentationRenderer.Create();

            MvcHtmlString output = renderer.ComponentPresentations(tridionPage, helper, byComponentTemplate, bySchema);

            return output;
        }
    }
}
