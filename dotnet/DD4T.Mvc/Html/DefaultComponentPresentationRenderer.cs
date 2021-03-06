﻿using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using DD4T.ContentModel;
using DD4T.Utils;
using DD4T.ContentModel.Logging;
using DD4T.ContentModel.Exceptions;

namespace DD4T.Mvc.Html
{
    public class DefaultComponentPresentationRenderer : IComponentPresentationRenderer
    {
        public static IComponentPresentationRenderer Create()
        {
            return new DefaultComponentPresentationRenderer();
        }
        private static bool ShowAnchors
        {
            get
            {
                return ConfigurationHelper.ShowAnchors;
            }
        }
        private static bool UseUriAsAnchor
        {
            get
            {
                return ConfigurationHelper.UseUriAsAnchor;
            }
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
                LoggerService.Debug("rendering cp {0} - {1}", LoggingCategory.Performance, cp.Component.Id, cp.ComponentTemplate.Id);
               
                cp.Page = tridionPage;
                LoggerService.Debug("about to call RenderComponentPresentation", LoggingCategory.Performance);
                if (ShowAnchors)
                    sb.Append(string.Format("<a id=\"{0}\"></a>", DD4T.Utils.TridionHelper.GetLocalAnchor(cp)));
                sb.Append(RenderComponentPresentation(cp, htmlHelper));
                LoggerService.Debug("finished calling RenderComponentPresentation", LoggingCategory.Performance);

            }
            LoggerService.Information("<<ComponentPresentations", LoggingCategory.Performance);
            return MvcHtmlString.Create(sb.ToString());
        }

        /// <summary>
        /// Resolve the viewName of the given ComponentTemplate.
        /// 
        /// This should be defined in the MetadataFields, if not the ComponentTemplate name will be used and stripped of spaces.
        /// </summary>
        /// <param name="componentTemplate">ComponentTemplate to use</param>
        /// <returns>Resolved viewName</returns>
        public string GetComponentTemplateView(IComponentTemplate componentTemplate)
        {
            string viewName = null;

            // If MetadataFields are empty or view is not present, fallback to the name of the componentTemplate
            if (componentTemplate.MetadataFields == null || !componentTemplate.MetadataFields.ContainsKey("view"))
                viewName = componentTemplate.Title.Replace(" ", "");
            else
                viewName = componentTemplate.MetadataFields["view"].Value;

            // If for any reason resolving the name fails or returns an empty value, throw an exception
            if (string.IsNullOrEmpty(viewName))
            {
                throw new ConfigurationException("no viewName could be resolved for component template " + componentTemplate.Id);
            }

            return viewName;
        }

        private bool MustInclude(IComponentTemplate itemToCheck, string[] pattern)
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
                string viewName = GetComponentTemplateView(itemToCheck);
                return pattern.Any<string>(item => viewName.ToLower().StartsWith(item.ToLower()));
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
