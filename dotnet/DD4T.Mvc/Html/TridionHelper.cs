using System.ComponentModel.Composition;
using System.Web.Mvc;
using DD4T.ContentModel;
using DD4T.Utils;
using DD4T.ContentModel.Logging;

namespace DD4T.Mvc.Html
{
    public static class TridionHelper
    {
        public static MvcHtmlString RenderComponentPresentations(this HtmlHelper helper)
        {
            return RenderComponentPresentations(helper, null, null, null);
        }

        public static MvcHtmlString RenderComponentPresentations(this HtmlHelper helper, IComponentPresentationRenderer renderer)
        {
           return RenderComponentPresentations(helper, null, null, renderer);
        }

        public static MvcHtmlString RenderComponentPresentationsByView(this HtmlHelper helper, string byComponentTemplate, IComponentPresentationRenderer renderer)
        {
            if (string.IsNullOrEmpty(byComponentTemplate))
                return RenderComponentPresentations(helper, new string[] { }, null, renderer);
            else
                return RenderComponentPresentations(helper, new [] {byComponentTemplate}, null, renderer);
        }
        public static MvcHtmlString RenderComponentPresentationsByView(this HtmlHelper helper, string byComponentTemplate)
        {
            if (string.IsNullOrEmpty(byComponentTemplate))
                return RenderComponentPresentations(helper, new string[] { }, null, null);
            else
                return RenderComponentPresentations(helper, new[] { byComponentTemplate }, null, null);
        }


        public static MvcHtmlString RenderComponentPresentationsByView(this HtmlHelper helper, string[] byComponentTemplate, IComponentPresentationRenderer renderer)
        {
            return RenderComponentPresentations(helper, byComponentTemplate, null, renderer);
        }

        public static MvcHtmlString RenderComponentPresentationsByView(this HtmlHelper helper, string[] byComponentTemplate)
        {
            return RenderComponentPresentations(helper, byComponentTemplate, null, null);
        }

        public static MvcHtmlString RenderComponentPresentationsBySchema(this HtmlHelper helper, string bySchema, IComponentPresentationRenderer renderer)
        {
            return RenderComponentPresentations(helper, null, bySchema, renderer);
        }

        public static MvcHtmlString RenderComponentPresentationsBySchema(this HtmlHelper helper, string bySchema)
        {
            return RenderComponentPresentations(helper, null, bySchema, null);
        }

        public static MvcHtmlString RenderComponentPresentations(this HtmlHelper helper, string[] byComponentTemplate, string bySchema, IComponentPresentationRenderer renderer)
        {
            LoggerService.Information(">>RenderComponentPresentations", LoggingCategory.Performance);
            IComponentPresentationRenderer cpr = renderer;
            if (!(helper.ViewData.Model is IPage))
            {
                return new MvcHtmlString("<!-- RenderComponentPresentations can only be used if the model is an instance of IPage -->");
            }

            LoggerService.Debug("about to cast object as IPage", LoggingCategory.Performance);
            IPage tridionPage = helper.ViewData.Model as IPage;
            LoggerService.Debug("finished casting object as IPage", LoggingCategory.Performance);
            if (renderer == null)
            {
                LoggerService.Debug("about to create DefaultComponentPresentationRenderer", LoggingCategory.Performance);
                renderer = DefaultComponentPresentationRenderer.Create();
                LoggerService.Debug("finished creating DefaultComponentPresentationRenderer", LoggingCategory.Performance);
            }

            LoggerService.Debug("about to call renderer.ComponentPresentations", LoggingCategory.Performance);
            MvcHtmlString output = renderer.ComponentPresentations(tridionPage, helper, byComponentTemplate, bySchema);
            LoggerService.Debug("finished calling renderer.ComponentPresentations", LoggingCategory.Performance);
            LoggerService.Information("<<RenderComponentPresentations", LoggingCategory.Performance);

            return output;
        }
    }
}
