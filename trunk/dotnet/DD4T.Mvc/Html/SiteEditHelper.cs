using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using DD4T.ContentModel;
using DD4T.Mvc.SiteEdit;

namespace DD4T.Mvc.Html
{
    public static class SiteEditHelper
    {
        public static MvcHtmlString SiteEditPage(this HtmlHelper helper, IPage page)
        {
            return new MvcHtmlString(SiteEdit.SiteEditService.GenerateSiteEditPageTag(page));
        }
        public static MvcHtmlString SiteEditComponentPresentation(this HtmlHelper helper, IComponentPresentation componentPresentation)
        {
            return SiteEditComponentPresentation(helper, componentPresentation, "default");
        }
        public static MvcHtmlString SiteEditComponentPresentation(this HtmlHelper helper, IComponent component, string componentTemplateId, string region)
        {
            return SiteEditComponentPresentation(helper, component, componentTemplateId, false, region);
        }
        public static MvcHtmlString SiteEditComponentPresentation(this HtmlHelper helper, IComponent component, string componentTemplateId, bool queryBased, string region)
        {
            ComponentTemplate ct = new ComponentTemplate();
            ct.Id = "tcm:10-403-32";
            ComponentPresentation cp = new ComponentPresentation();
            cp.Component = component as Component;
            cp.ComponentTemplate = ct;
            cp.OrderOnPage = -1;
            return SiteEditComponentPresentation(helper, cp, queryBased, region);
        }
        public static MvcHtmlString SiteEditComponentPresentation(this HtmlHelper helper, IComponentPresentation componentPresentation, string region)
        {
            return SiteEditComponentPresentation(helper, componentPresentation, false, region);
        }
        public static MvcHtmlString SiteEditComponentPresentation(this HtmlHelper helper, IComponentPresentation componentPresentation, bool queryBased, string region)
        {
            return new MvcHtmlString(SiteEdit.SiteEditService.GenerateSiteEditComponentTag(componentPresentation, queryBased, region));
        }
        public static MvcHtmlString SiteEditField(this HtmlHelper helper, IComponent component, IField field)
        {
            return SiteEditField(helper, component, field, -1);
        }

        public static MvcHtmlString SiteEditField(this HtmlHelper helper, IComponent component, IField field, int index)
        {
            string retVal = string.Empty;
            bool seEnabled = SiteEditService.IsSiteEditEnabled(component);

            if (seEnabled)
            {
                //if (field.EmbeddedSchema != null)
                //{
                    if (index == -1)
//                        retVal += SiteEditService.GenerateSiteEditFieldMarking(field.Name, field.EmbeddedSchema.RootElementName);
                        retVal += SiteEditService.GenerateSiteEditFieldTag(field, 0);
                    else
//                        retVal += SiteEditService.GenerateSiteEditFieldMarking(field.Name, field.EmbeddedSchema.RootElementName, index);
                        retVal += SiteEditService.GenerateSiteEditFieldTag(field, index);
                //}
                //else
                //{
                //    if (index == -1)
                //        retVal += SiteEditService.GenerateSiteEditFieldMarking(field.Name);
                //    else
                //        retVal += SiteEditService.GenerateSiteEditFieldMarking(field.Name, index);
                //}
            }
            // TODO: handle component links, embedded fields, etc
            return new MvcHtmlString(retVal);
        }
    }
}
