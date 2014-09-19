using System.Web.Mvc;
using DD4T.ContentModel;
using DD4T.Mvc.SiteEdit;
using System.Collections.Generic;
using System.Xml.Serialization;
using DD4T.ContentModel.Contracts.Factories;
using DD4T.Factories;
using System.Linq;
using System.Collections;

namespace DD4T.Mvc.Html
{
    public static class SmartTargetHelper
    {
        public static MvcHtmlString RenderSmartTargetRegion(this HtmlHelper helper, string region, string view, int? limit = null)
        {
            IEnumerable<IComponentPresentation> componentPresentations = QuerySmartTarget(region, view, limit);
            
            //Render ComponentPresentations
            return TridionHelper.RenderDynamicComponentPresentations(helper, componentPresentations.Take(limit.Value));
        }

        private static IEnumerable<IComponentPresentation> QuerySmartTarget(string region, string view, int? limit)
        {
            //Query SmartTarget passing region and limit values

            //Initialize Component Presenatation container
            IList<ComponentPresentation> componentPresentations = new List<ComponentPresentation>();
            //Initialize ComponentFactory
            ComponentFactory cf = new ComponentFactory();

            //Loop SmartTarget Promotion Results
            //foreach(var promotion in Promotions)
            //{
                //Add Component Presentation to container
                componentPresentations.Add(new ComponentPresentation
                                                {
                                                    ComponentTemplate = new ComponentTemplate
                                                    {
                                                        Id = "tcm:2011-3103-32",
                                                        MetadataFields = new FieldSet 
                                                                                    { 
                                                                                        {
                                                                                            "view", 
                                                                                            new Field 
                                                                                            {
                                                                                                Values = new List<string> 
                                                                                                {
                                                                                                    view
                                                                                                }
                                                                                            } 
                                                                                        } 
                                                                                    }
                                                    },
                                                    Component = cf.GetComponent("tcm:2011-69-16", "tcm:2011-3103-32") as Component
                                                }
                                            );
            
            componentPresentations.Add(new ComponentPresentation
                                                {
                                                    ComponentTemplate = new ComponentTemplate
                                                    {
                                                        Id = "tcm:2011-3103-32",
                                                        MetadataFields = new FieldSet 
                                                                                    { 
                                                                                        {
                                                                                            "view", 
                                                                                            new Field 
                                                                                            {
                                                                                                Values = new List<string> 
                                                                                                {
                                                                                                    "General"
                                                                                                }
                                                                                            } 
                                                                                        } 
                                                                                    }
                                                    },
                                                    Component = cf.GetComponent("tcm:2011-3080-16", "tcm:2011-3103-32") as Component
                                                }
                                            );

            //}


            return componentPresentations;
        }  
    }
}
