using System.ComponentModel.Composition;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Linq;

using DD4T.Extensions.DynamicDelivery.ContentModel;
using System;
using System.IO;

namespace DD4T.Extensions.DynamicDelivery.Mvc.Html
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
            StringWriter sw = new StringWriter();
            foreach (IComponentPresentation cp in tridionPage.ComponentPresentations)
            {
                if (!MustInclude(cp.ComponentTemplate, includeComponentTemplate))
                    continue;
                if ((!string.IsNullOrEmpty(includeSchema)) && !MustInclude(cp.Component.Schema, includeSchema))
                    continue;
                cp.Page = tridionPage;
                sw.Write(RenderComponentPresentation(cp, htmlHelper));
            }
            return MvcHtmlString.Create(sw.ToString());
        }

        private bool MustInclude(IComponentTemplate itemToCheck, string[] pattern)
        {
            if (pattern[0].StartsWith("tcm:"))
            {
                return pattern.Any<string>(item => item.Equals(itemToCheck.Id));
            }
            else
            {
                // pattern does not start with tcm:, we will treat it as a (part of a) title
                IField view;
                if (itemToCheck.MetadataFields != null && itemToCheck.MetadataFields.TryGetValue("view", out view))
                {
                    string viewName = FactoryService.ModelFactory.MapComponentViewName(view.Value);
                    return pattern.Any<string>(item => item.Equals(viewName));
                }
                else
                {
                    System.Diagnostics.Trace.TraceError("view for {0} not set", itemToCheck.Title);
                    return false;
                }
            }
        }

        private bool MustInclude(ISchema itemToCheck, string pattern)
        {
            if (pattern.StartsWith("tcm:"))
            {
                return itemToCheck.Id == pattern;
            }
            else
            {
                return itemToCheck.Title.StartsWith(pattern);
            }
        }

        private MvcHtmlString RenderComponentPresentation(IComponentPresentation cp, HtmlHelper htmlHelper)
        {
            string controller = WebConfigurationManager.AppSettings["Controller"];
            string action = WebConfigurationManager.AppSettings["Action"]; 
        
            
            if (cp.ComponentTemplate.MetadataFields.ContainsKey("controller"))
            {
                controller = cp.ComponentTemplate.MetadataFields["controller"].Value;
            }
            if (cp.ComponentTemplate.MetadataFields.ContainsKey("action"))
            {
                action = cp.ComponentTemplate.MetadataFields["action"].Value;
            }

            return ChildActionExtensions.Action(htmlHelper, action, controller, new { componentPresentation = cp });
        }

    }
}
