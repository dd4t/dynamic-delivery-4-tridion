using System.ComponentModel.Composition;
using System.Web.Mvc;
using DD4T.ContentModel;
using DD4T.Utils;
using DD4T.ContentModel.Logging;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Configuration;
using DD4T.ContentModel.Factories;
using DD4T.Factories;

namespace DD4T.Mvc.Html
{
    public static class TridionHelper
    {
        private static ILinkFactory _linkFactory = null;
        private static ILinkFactory LinkFactory
        {
            get
            {
                if (_linkFactory == null)
                {
                    _linkFactory = new LinkFactory();
                }
                return _linkFactory;
            }
        }

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
            IPage page = null;
            if (helper.ViewData.Model is IPage)
            {
                page = helper.ViewData.Model as IPage;
            }
            else 
            {
                try
                {
                    page = helper.ViewContext.Controller.ViewBag.Page;
                }
                catch
                {
                    return new MvcHtmlString("<!-- RenderComponentPresentations can only be used if the model is an instance of IPage or if there is a Page property in the viewbag with type IPage -->");
                }
            }

            if (renderer == null)
            {
                LoggerService.Debug("about to create DefaultComponentPresentationRenderer", LoggingCategory.Performance);
                renderer = DefaultComponentPresentationRenderer.Create();
                LoggerService.Debug("finished creating DefaultComponentPresentationRenderer", LoggingCategory.Performance);
            }

            LoggerService.Debug("about to call renderer.ComponentPresentations", LoggingCategory.Performance);
            MvcHtmlString output = renderer.ComponentPresentations(page, helper, byComponentTemplate, bySchema);
            LoggerService.Debug("finished calling renderer.ComponentPresentations", LoggingCategory.Performance);
            LoggerService.Information("<<RenderComponentPresentations", LoggingCategory.Performance);

            return output;
        }


        #region linking functionality
        public static string GetResolvedUrl(this IComponent component)
        {
            return LinkFactory.ResolveLink(component.Id);            
        }
        public static MvcHtmlString GetResolvedLink(this IComponent component)
        {
            return component.GetResolvedLink(component.Title, "");
        }

        public static MvcHtmlString GetResolvedLink(this IComponent component, string linkText, string showOnFail)
        {
            string url = component.GetResolvedUrl();
            if (string.IsNullOrEmpty(url))
                return new MvcHtmlString(showOnFail);
            return new MvcHtmlString(string.Format("<a href=\"{0}\">{1}</a>", url, linkText));
        }

        #endregion

        #region welcome file functionality
        public static string AddWelcomeFile(string url)
        {
            if (string.IsNullOrEmpty(url))
                return DefaultPageFileName;
            if (!reDefaultPage.IsMatch("/" + url))
                return url;
            if (url.EndsWith("/"))
                return url + DefaultPageFileName;
            return url + "/" + DefaultPageFileName;
        }

        private static string _defaultPageFileName = null;
        private static Regex reDefaultPage = new Regex(@".*/[^/\.]*(/?)$");
        public static string DefaultPageFileName
        {
            get
            {
                if (_defaultPageFileName == null)
                    _defaultPageFileName = ConfigurationHelper.WelcomeFile;

                return _defaultPageFileName;
            }
        }
        #endregion
    }
}
