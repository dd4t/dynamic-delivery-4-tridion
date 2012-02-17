using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using DD4T.ContentModel;
using DD4T.Utils;
using DD4T.ContentModel.Logging;

namespace DD4T.Mvc.Html
{
    [Export(typeof(IComponentPresentationRenderer))]
    public class DefaultComponentPresentationRenderer : IComponentPresentationRenderer
    {
        public static IComponentPresentationRenderer Create()
        {
            return new DefaultComponentPresentationRenderer();
        }

        public MvcHtmlString ComponentPresentations(IPage tridionPage, HtmlHelper htmlHelper, string[] includeComponentTemplate, string includeSchema)
        {
            LoggerService.Information(">>ComponentPresentations", LoggingCategory.Performance);
            StringBuilder sb = new StringBuilder();
            foreach (IComponentPresentation cp in tridionPage.ComponentPresentations)
            {
                if (includeComponentTemplate != null && !MustInclude(cp.ComponentTemplate, includeComponentTemplate))
                    continue;
                if (cp.Component != null && (!string.IsNullOrEmpty(includeSchema)) && !MustInclude(cp.Component.Schema, includeSchema))
                    continue;
                // Quirijn: if a component presentation was created by a non-DD4T template, its output was stored in RenderedContent
                // In that case, we simply write it out
                // Note that this type of component presentations cannot be excluded based on schema, because we do not know the schema
                if (!string.IsNullOrEmpty(cp.RenderedContent))
                    return new MvcHtmlString(cp.RenderedContent);
                LoggerService.Debug("found cp {0} - {1}", LoggingCategory.Performance, cp.Component.Id, cp.ComponentTemplate.Id);
                LoggerService.Debug("setting page property on cp ", LoggingCategory.Performance);
                cp.Page = tridionPage;
                LoggerService.Debug("about to call RenderComponentPresentation", LoggingCategory.Performance);
                sb.Append(RenderComponentPresentation(cp, htmlHelper));
                LoggerService.Debug("finished calling RenderComponentPresentation", LoggingCategory.Performance);

            }
            LoggerService.Information("<<ComponentPresentations", LoggingCategory.Performance);
            return MvcHtmlString.Create(sb.ToString());
        }

        private static bool MustInclude(IComponentTemplate itemToCheck, string[] pattern)
        {
            if (pattern.Length == 0)
            {
                return true; // if no template was specified, always render the component presentation (so return true)
            }
            if (pattern[0].StartsWith("tcm:"))
            {
                return pattern.Any<string>(item => item.Equals(itemToCheck.Id));
            }
            else
            {
                // pattern does not start with tcm:, we will treat it as a (part of a) view
                IField view;
                if (itemToCheck.MetadataFields != null && itemToCheck.MetadataFields.TryGetValue("view", out view))
                {
                    return pattern.Any<string>(item => view.Value.ToLower().StartsWith(item.ToLower()));
                }
                else
                {
                    System.Diagnostics.Trace.TraceError("view for {0} not set", itemToCheck.Title);
                    return false;
                }
            }
        }

        private static bool MustInclude(ISchema itemToCheck, string pattern)
        {
            bool reverse = false;
            if (pattern.StartsWith("!"))
            {
                // reverse sign of the boolean
                reverse = true;
                pattern = pattern.Substring(1);
            }

            if (pattern.StartsWith("tcm:"))
            {
                return itemToCheck.Id == pattern ^ reverse; // use 'exclusive or' to reverse the sign if necessary!
            }

            return itemToCheck.Title.ToLower().StartsWith(pattern.ToLower()) ^ reverse;
        }

        private static MvcHtmlString RenderComponentPresentation(IComponentPresentation cp, HtmlHelper htmlHelper)
        {
            string controller = ConfigurationHelper.ComponentPresentationController;
            string action = ConfigurationHelper.ComponentPresentationAction;


            if (cp.ComponentTemplate.MetadataFields != null && cp.ComponentTemplate.MetadataFields.ContainsKey("controller"))
            {
                controller = cp.ComponentTemplate.MetadataFields["controller"].Value;
            }
            if (cp.ComponentTemplate.MetadataFields != null && cp.ComponentTemplate.MetadataFields.ContainsKey("action"))
            {
                action = cp.ComponentTemplate.MetadataFields["action"].Value;
            }


            LoggerService.Debug("about to render component presentation with controller {0} and action {1}", LoggingCategory.Performance, controller, action);
            //return ChildActionExtensions.Action(htmlHelper, action, controller, new { componentPresentation = ((ComponentPresentation)cp) });
            MvcHtmlString result = htmlHelper.Action(action, controller, new { componentPresentation = ((ComponentPresentation)cp) });
            LoggerService.Debug("finished rendering component presentation with controller {0} and action {1}", LoggingCategory.Performance, controller, action);
            return result;
        }
    }
}
