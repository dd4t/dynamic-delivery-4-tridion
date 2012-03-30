using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tridion.ContentManager.ContentManagement;
using Tridion.ContentManager.Templating.Assembly;
using Tridion.ContentManager.Templating;
using Tridion.ContentManager.CommunicationManagement;
using Tridion.ContentManager.Publishing;

namespace DD4T.Examples.Templates.ExtensionMethods
{

    /// <summary>
    /// Contains the methods from Tridion PS' BaseTemplate class, rewritten as extension methods.
    /// Purpose is to be able to attach these methods to any implementation of ITemplate.
    /// In order to use these extension methods, template classes must implement IExtendibleTemplate, which 
    /// provides the Initialize method that stores the package and engine for future reference.
    /// <remarks>
    /// - not all methods from BaseTemplate have been ported, if you need another one just add it
    /// - there is no such thing as an 'extension property', so all properties must be rewritten as methods
    /// </remarks>
    /// 
    /// </summary>
    public static class TemplateExtensions
    {
        /// <summary>
        /// Returns the component object that is defined in the package for this template.
        /// </summary>
        /// <remarks>
        /// This method should only be called when there is an actual Component item in the package. 
        /// It does not currently handle the situation where no such item is available.
        /// </remarks>
        /// <returns>the component object that is defined in the package for this template.</returns>
        public static Component GetComponent(this IExtendibleTemplate template)
        {
            template.CheckInitialized();
            Item component = template.Package.GetByType(ContentType.Component);
            return (Component)template.Engine.GetObject(component.GetAsSource().GetValue("ID"));
        }

        /// <summary>
        /// Returns the page object that is defined in the package for this template.
        /// </summary>
        /// <remarks>
        /// This method should only be called when there is an actual Page item in the package. 
        /// It does not currently handle the situation where no such item is available.
        /// </remarks>
        /// <returns>the page object that is defined in the package for this template.</returns>
        public static Page GetPage(this IExtendibleTemplate template)
        {
            template.CheckInitialized();

            Item pageItem = template.Package.GetByType(ContentType.Page);
            if (pageItem != null)
            {
                return template.Engine.GetObject(pageItem.GetAsSource().GetValue("ID")) as Page;
            }

            Page page = template.Engine.PublishingContext.RenderContext.ContextItem as Page;
            return page;
        }

        /// <summary>
        /// Returns the publication object that can be determined from the package for this template.
        /// </summary>
        /// <remarks>
        /// This method currently depends on a Page item being available in the package, meaning that
        /// it will only work when invoked from a Page Template.
        /// 
        /// Updated by Kah Tan (kah.tang@indivirtual.com)
        /// </remarks>
        /// <returns>the Publication object that can be determined from the package for this template.</returns>
        public static Publication GetPublication(this IExtendibleTemplate template)
        {
            template.CheckInitialized();

            RepositoryLocalObject pubItem = null;
            Repository repository = null;

            if (template.Package.GetByType(ContentType.Page) != null)
                pubItem = template.GetPage();
            else
                pubItem = template.GetComponent();

            if (pubItem != null) repository = pubItem.ContextRepository;

            return repository as Publication;
        }



        /// <summary>
        /// True if the rendering context is a page, rather than component
        /// </summary>
        public static bool IsPageTemplate(this IExtendibleTemplate template)
        {
            template.CheckInitialized();
            return template.Engine.PublishingContext.ResolvedItem.Item is Page;
        }


        public static bool IsPublishing(this IExtendibleTemplate template)
        {
            template.CheckInitialized();
            return (template.Engine.RenderMode == RenderMode.Publish);
        }

        /// <summary>
        /// Creates a new text item in the package
        /// </summary>
        /// <param name="name">The name of the text item to create in the package </param>
        /// <param name="value">The value of the text item to create in the package</param>
        public static void CreateTextItem(this IExtendibleTemplate template, string name, string value)
        {
            template.CheckInitialized();
            template.CreateStringItem(name, value, ContentType.Text);
        }
        /// <summary>
        /// creates a new String item in the package with the specified ContentType
        /// </summary>
        /// <param name="name">The name of the text item to create in the package </param>
        /// <param name="value">The value of the text item to create in the package</param>
        /// <param name="type">The Type of the item to create, for instance: text, html, xml, etc.</param>
        public static void CreateStringItem(this IExtendibleTemplate template, string name, string value, ContentType type)
        {
            template.CheckInitialized();
            template.Package.PushItem(name, template.Package.CreateStringItem(type, value));
        }

        #region private methods (can be called from the public extension methods in this static class)
        private static void CheckInitialized(this IExtendibleTemplate template)
        {
            if (template.Package == null || template.Engine == null)
            {
                throw new InvalidOperationException("This method can not be invoked, unless Initialize has been called");
            }
        }
        #endregion
    }
}
