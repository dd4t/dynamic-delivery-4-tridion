using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using Tridion.ContentManager;
using Tridion.ContentManager.CommunicationManagement;
using Tridion.ContentManager.ContentManagement;
using Tridion.ContentManager.ContentManagement.Fields;
using Tridion.ContentManager.Templating;
using Tridion.ContentManager.Templating.Assembly;
using Tridion.ContentManager.Publishing;

namespace Sample.Web.Templating.Base.Regular
{
    /// <summary>
    /// This class contains useful helper functions for typical things you might want to do in a
    /// template. It can be used as the base class for your own Template Building Blocks.
    ///  Use this abstract class to derive your new Template from.
    /// </summary>
    /// <date>
    ///     created 31-January-2008
    ///		updated: 09-June-2009 
    /// </date>	
    /// 
    [Obsolete]
    public abstract class TemplateBase : ITemplate, IDisposable
    {
        #region Constants

        // Output Format constants
        protected const string OfAspjscript = "ASP JScript";
        protected const string OfAspvbscript = "ASP VBScript";
        protected const string OfAscx = "ASCX WebControl";
        protected const string OfJsp = "JSP Scripting";

        // Target Language constants
        protected const string TlAspjscript = "ASP/JavaScript";
        protected const string TlAspvbscript = "ASP/VBScript";
        protected const string TlAspdotnet = "ASP.NET";
        protected const string TlJsp = "JSP";

        #endregion

        #region Private Members

        private bool _disposed = false; //Indicates whether system resources used by this instance have been released
        private TemplatingLogger _logger;
        private XmlNamespaceManager _nsm;

        #endregion

        #region Protected Members

        protected Engine MEngine;
        protected Package MPackage;
        protected int RenderContext = -1;

        #endregion

        #region Properties

        /// <summary>
        /// An XmlNameSpaceManager already initialized with several XML namespaces such like: tcm, xlink and xhtml
        /// </summary>
        protected XmlNamespaceManager NsManager
        {
            get
            {
                if (_nsm == null)
                {
                    _nsm = new XmlNamespaceManager(new NameTable());

                    _nsm.AddNamespace(Tridion.Constants.TcmPrefix, Tridion.Constants.TcmNamespace);
                    _nsm.AddNamespace(Tridion.Constants.XlinkPrefix, Tridion.Constants.XlinkNamespace);
                    _nsm.AddNamespace(Tridion.Constants.XhtmlPrefix, Tridion.Constants.XhtmlNamespace);
                }

                return _nsm;
            }
        }

        protected TemplatingLogger Logger
        {
            get
            {
                if (_logger == null) _logger = TemplatingLogger.GetLogger(this.GetType());

                return _logger;
            }
        }

        /// <summary>
        /// Returns true if the current render mode is Publish
        /// </summary>
        protected bool IsPublishing
        {
            get
            {
                CheckInitialized();
                return (MEngine.RenderMode == RenderMode.Publish);
            }
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Initializes the engine and package to use in this BaseTemplate object.
        /// </summary>
        /// <param name="engine">The engine to use in calls to the other methods of this BaseTemplate object</param>
        /// <param name="package">The package to use in calls to the other methods of this BaseTemplate object</param>
        protected void Initialize(Engine engine, Package package)
        {
            MEngine = engine;
            MPackage = package;
        }

        /// <summary>
        /// Checks whether the BaseTemplate object has been initialized correctly.
        /// This method should be called from any method that requires the <c>MEngine</c>, 
        /// <c>MPackage</c> or <c>_log</c> member fields.
        /// </summary>
        protected void CheckInitialized()
        {
            if (MEngine == null || MPackage == null)
            {
                throw new InvalidOperationException("This method can not be invoked, unless Initialize has been called");
            }
        }

        protected bool MustInclude(Regex regex, ref string title)
        {
            if (regex == null) return true;
            MatchCollection matches = regex.Matches(title);
            if (matches.Count > 0)
            {
                // remove the prefix (using the first match)
                title = title.Substring(matches[0].Groups[0].Length);
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region Base Functionality

        /// <summary>
        /// Returns the component object that is defined in the package for this template.
        /// </summary>
        /// <remarks>
        /// This method should only be called when there is an actual Component item in the package. 
        /// It does not currently handle the situation where no such item is available.
        /// </remarks>
        /// <returns>the component object that is defined in the package for this template.</returns>
        protected Component GetComponent()
        {
            CheckInitialized();
            Item component = MPackage.GetByType(ContentType.Component);
            return (Component)MEngine.GetObject(component.GetAsSource().GetValue("ID"));
        }

        /// <summary>
        /// Returns the Template from the resolved item if it's a Component Template
        /// </summary>
        /// <returns>A Component Template or null</returns>
        protected ComponentTemplate GetComponentTemplate()
        {
            CheckInitialized();
            Template template = MEngine.PublishingContext.ResolvedItem.Template;

            // "if (template is ComponentTemplate)" might work instead
            if (template.GetType().Name.Equals("ComponentTemplate"))
            {
                return (ComponentTemplate)template;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Returns the Template from the resolved item if it's a Page Template
        /// </summary>
        /// <returns>A Page Template or null</returns>
        protected PageTemplate GetPageTemplate()
        {
            CheckInitialized();
            Template template = MEngine.PublishingContext.ResolvedItem.Template;

            // "if (template is PageTemplate)" might work instead
            if (template.GetType().Name.Equals("PageTemplate"))
            {
                return (PageTemplate)template;
            }
            else
            {
                return null;
            }
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
        protected Publication GetPublication()
        {
            CheckInitialized();

            RepositoryLocalObject pubItem = null;
            Repository repository = null;

            if (MPackage.GetByType(ContentType.Page) != null)
                pubItem = GetPage();
            else
                pubItem = GetComponent();

            if (pubItem != null) repository = pubItem.ContextRepository;

            return repository as Publication;
        }

        /// <summary>
        /// Returns the list of structure groups of the current publication.
        /// </summary>
        /// <remarks>
        /// This call is expensive, as each object must be retrieved from the underlying layers separately.
        /// For a cheaper alternative (with less functionality) you should consider <c>GetListStructureGroups</c>
        /// </remarks>
        /// <returns>the list of structure groups of the current publication</returns>
        /// <seealso cref="StructureGroup"/>
        /// <seealso cref="GetListStructureGroups"/>
        protected IList<StructureGroup> GetStructureGroups()
        {
            CheckInitialized();
            Publication publication = GetPublication();
            //Filter sgFilter = new Filter();
            //sgFilter.Conditions["ItemType"] = ItemType.StructureGroup;
            OrganizationalItemsFilter sgFilter = new OrganizationalItemsFilter(publication.Session) { ItemTypes = new List<ItemType> { ItemType.StructureGroup } };

            return GetObjectsFromXmlList<StructureGroup>(MEngine, publication.GetListOrganizationalItems(sgFilter));
        }

        /// <summary>
        /// Returns the list of structure groups of the current publication. This call is cheap, but the results 
        /// are shallow.
        /// </summary>
        /// <remarks>
        /// Note that the objects returned are not TOM.NET StructureGroup objects, but a more shallow
        /// type of object. If you require the full functionality of TOM.NET StructureGroup objects, you should 
        /// call the more expensive <c>GetStructureGroups</c> method.
        /// </remarks>
        /// <example>
        /// The results are unsorted. Sorting the result in place can be done with:
        /// <code>
        /// SGs.Sort(
        ///     delegate(ListItem item1, ListItem item2)
        ///     {
        ///         return item1.Title.CompareTo(item2.Title);
        ///     }
        /// );
        /// </code>
        /// </example>
        /// <returns>the list of structure groups of the current publication</returns>
        /// <seealso cref="ListItem"/>
        /// <seealso cref="GetStructureGroups"/>
        protected IList<ListItem> GetListStructureGroups()
        {
            CheckInitialized();
            Publication publication = GetPublication();
            Filter filter = new Filter();
            filter.Conditions["ItemType"] = ItemType.StructureGroup;
            filter.BaseColumns = ListBaseColumns.Extended;
            filter.AdditionalColumns.Add("url");
            OrganizationalItemsFilter sgFilter = new OrganizationalItemsFilter(filter, publication.Session);

            XmlElement orgItems = publication.GetListOrganizationalItems(sgFilter);

            XmlNodeList itemElements = orgItems.SelectNodes("*");
            List<ListItem> result = new List<ListItem>(itemElements.Count);
            foreach (XmlElement itemElement in itemElements)
            {
                ListItem sg = new ListItem(itemElement);
                result.Add(sg);
            }

            return result;
        }

        protected void GetPages()
        {
            CheckInitialized();
            Publication publication = GetPublication();
            Filter filter = new Filter();
            filter.Conditions["ItemType"] = ItemType.Page;
            filter.Conditions["Recursive"] = true;
            filter.BaseColumns = ListBaseColumns.Extended;
            filter.AdditionalColumns.Add("url");
            RepositoryItemsFilter pageFilter = new RepositoryItemsFilter(filter, publication.Session);

            XmlElement orgItems = publication.GetListItems(pageFilter);

            /*
            XmlNodeList itemElements = orgItems.SelectNodes("*");
            List<ListItem> result = new List<ListItem>(itemElements.Count);
            foreach (XmlElement itemElement in itemElements)
            {
                ListItem sg = new ListItem(itemElement);
                result.Add(sg);
            }
            */
            XmlDocument doc = new XmlDocument();
            doc.AppendChild(doc.ImportNode(orgItems, true));
            MPackage.PushItem("Pages", MPackage.CreateXmlDocumentItem(ContentType.Xml, doc));

        }

        protected List<ListItem> GetPages(StructureGroup sg)
        {
            CheckInitialized();
            Filter filter = new Filter();
            filter.Conditions["ItemType"] = ItemType.Page;
            //filter.BaseColumns = ListBaseColumns.Extended;
            //filter.AdditionalColumns.Add("url");
            //filter.AdditionalColumns.Add("path");
            OrganizationalItemItemsFilter pageFilter = new OrganizationalItemItemsFilter(filter, sg.Session);

            XmlElement orgItems = sg.GetListItems(pageFilter);

            XmlNodeList itemElements = orgItems.SelectNodes("*");
            List<ListItem> result = new List<ListItem>(itemElements.Count);
            foreach (XmlElement itemElement in itemElements)
            {
                result.Add(new ListItem(itemElement));
            }

            return result;
        }

        protected IList<Page> GetPagesInSG(ListItem sg)
        {
            CheckInitialized();

            Filter filter = new Filter();
            filter.Conditions["ItemType"] = ItemType.Page;
            filter.BaseColumns = ListBaseColumns.Extended;
            filter.AdditionalColumns.Add("url");
            filter.AdditionalColumns.Add("path");

            // TODO: find a way to avoid retrieving the SG
            StructureGroup structuregroup = MEngine.GetObject(sg.Id) as StructureGroup;
            OrganizationalItemItemsFilter pageFilter = new OrganizationalItemItemsFilter(filter, structuregroup.Session);
            IList<RepositoryLocalObject> rlos = (IList<RepositoryLocalObject>)structuregroup.GetItems(pageFilter);

            List<Page> pages = new List<Page>(rlos.Count);
            foreach (RepositoryLocalObject o in rlos)
            {
                pages.Add((Page)o);
            }

            return pages;
        }

        /// <summary>
        /// Creates a new text item in the package
        /// </summary>
        /// <param name="name">The name of the text item to create in the package </param>
        /// <param name="value">The value of the text item to create in the package</param>
        protected void CreateTextItem(string name, string value)
        {
            CreateStringItem(name, value, ContentType.Text);
        }

        /// <summary>
        /// creates a new String item in the package with the specified ContentType
        /// </summary>
        /// <param name="name">The name of the text item to create in the package </param>
        /// <param name="value">The value of the text item to create in the package</param>
        /// <param name="type">The Type of the item to create, for instance: text, html, xml, etc.</param>
        protected void CreateStringItem(string name, string value, ContentType type)
        {
            CheckInitialized();

            MPackage.PushItem(name, MPackage.CreateStringItem(type, value));
        }

        /// <summary>
        /// Returns the string value for an item in the package with the specified name
        /// </summary>
        /// <param name="itemName">the name of the item to get the value from the package for</param>
        /// <returns>The string value of the item in the package or null if the item cannot be found</returns>
        protected String GetStringItemValue(string itemName)
        {
            return GetStringItemValue(itemName, false, false);
        }

        /// <summary>
        /// Returns the string value for an item in the package with the specified name
        /// </summary>
        /// <param name="itemName">the name of the item to get the value from the package for</param>
        /// <param name="dblCheck">whether to perfrom a second lookup using the value returned from the package</param>
        /// <returns>The string value of the item in the package or null if the item cannot be found</returns>
        protected String GetStringItemValue(string itemName, bool dblCheck)
        {
            return GetStringItemValue(itemName, dblCheck, false);
        }

        /// <summary>
        /// Returns the string value for an item in the package with the specified name
        /// </summary>
        /// <param name="itemName">the name of the item to get the value from the package for</param>
        /// <param name="fallBackName">another name of a package item to try and get if the first name did not return a value</param>
        /// <returns>The string value of the item in the package or null if the item cannot be found</returns>
        protected String GetStringItemValue(string itemName, string fallBackName)
        {
            return GetStringItemValue(itemName, fallBackName, false);
        }

        /// <summary>
        /// Returns the string value for an item in the package with the specified name
        /// </summary>
        /// <param name="itemName">the name of the item to get the value from the package for</param>
        /// <param name="fallBackName">another name of a package item to try and get if the first name did not return a value</param>
        /// <param name="dblCheck">whether to perfrom a second lookup using the value returned from the package</param>
        /// <returns>The string value of the item in the package or null if the item cannot be found</returns>
        protected String GetStringItemValue(string itemName, string fallBackName, bool dblCheck)
        {
            String res = GetStringItemValue(itemName, dblCheck, !String.IsNullOrEmpty(fallBackName));

            if (String.IsNullOrEmpty(res)) res = GetStringItemValue(fallBackName, false, false);

            return res;
        }

        /// <summary>
        /// Returns the string value for an item in the package with the specified name
        /// </summary>
        /// <param name="itemName">the name of the item to get the value from the package for</param>
        /// <param name="dblCheck">whether to perfrom a second lookup using the value returned from the package</param>
        /// <param name="fallBack">if false, method will get the value for the item with the name specified in the itemName parameter when the double lookup fails </param>
        /// <returns></returns>
        protected String GetStringItemValue(string itemName, bool dblCheck, bool fallBack)
        {
            CheckInitialized();

            string res = MPackage.GetValue(itemName);

            if (dblCheck)
            {
                if (!String.IsNullOrEmpty(res)) res = MPackage.GetValue(res);

                if (String.IsNullOrEmpty(res) && !fallBack) res = MPackage.GetValue(itemName);
            }

            return res;
        }

        /// <summary>
        /// Returns an XML document describing the entire navigation structure of the publication on which this 
        /// template is invoked.
        /// </summary>
        /// <param name="rootSG">the structure group from which to start building the navigation</param>
        /// <param name="SGs">the list of all structure groups in the publication, as retrieved from GetListStructureGroups</param>
        /// <returns>an XML document describing the entire navigation structure of the publication on which this 
        /// template is invoked</returns>
        /// <example>
        /// The following code:
        /// <code>
        ///     XmlDocument sitemap = GetSiteMap(getRootSG(), GetListStructureGroups());
        /// </code>
        /// will result in the following XML:
        /// <code>
        /// &lt;StructureGroup id="tcm:1-7-4" title="www.tridion.com" url="/">
        ///     &lt;StructureGroup id="tcm:1-10-4" title="200 Products" url="/Products">
        ///         &lt;StructureGroup id="tcm:1-11-4" title="210 R5" url="/Products/R5">
        /// 	        &lt;Page id="tcm:1-85-64" title="211 Content Creation" url="/Products/R5/ContentCreation.html"/>
        /// 	        &lt;Page id="tcm:1-95-64" title="215 Dynamic Content Broker" url="/Products/R5/DynamicContentBroker.html"/>
        /// 	        &lt;Page id="tcm:1-66-64" title="217 Archive Manager" url="/Products/R5/ArchiveManager.html"/>
        ///         &lt;/StructureGroup>
        ///         &lt;StructureGroup id="tcm:1-13-4" title="240 Interactive Web Applications" url="/Products/InteractiveWebApplications"/>
        ///     &lt;/StructureGroup>
        ///     &lt;StructureGroup id="tcm:1-14-4" title="100 Solutions" url="/Solutions">
        ///         &lt;StructureGroup id="tcm:1-22-4" title="110 For you industry" url="/Solutions/ForYouIndustry"/>
        ///         &lt;StructureGroup id="tcm:1-26-4" title="150 Download center" url="/Solutions/DownloadCenter"/>
        ///     &lt;/StructureGroup>
        ///     &lt;StructureGroup id="tcm:1-15-4" title="300 Customers" url="/Customers"/>
        ///     &lt;StructureGroup id="tcm:1-16-4" title="400 Service &amp; Support" url="/ServiceSupport"/>
        ///     &lt;StructureGroup id="tcm:1-17-4" title="500 Partners" url="/Partners">
        ///         &lt;Page id="tcm:1-111-64" title="Indivirtual" url="/Partners/Indivirtual.html"/>
        ///     &lt;/StructureGroup>
        ///     &lt;StructureGroup id="tcm:1-19-4" title="700 About Tridion" url="/AboutTridion"/>
        /// &lt;/StructureGroup>
        /// </code>
        /// </example>
        /// <seealso cref="GetListStructureGroups"/>
        /// <seealso cref="GetRootSG"/>
        protected XmlDocument GetSiteMap(ListItem rootSG, IList<ListItem> SGs)
        {
            return GetSiteMap(rootSG, SGs, null, null, null);
        }

        /// <summary>
        /// Returns an XML document describing the entire navigation structure of the publication on which this 
        /// template is invoked.
        /// </summary>
        /// <param name="rootSG">the structure group from which to start building the navigation</param>
        /// <param name="SGs">the list of all structure groups in the publication, as retrieved from GetListStructureGroups</param>
        /// <param name="regex">the regex that items must match to be included in the menu</param>
        /// <returns>an XML document describing the entire navigation structure of the publication on which this 
        /// template is invoked</returns>
        /// <example>
        /// The following code:
        /// <code>
        ///     Regex regex = ...
        ///     XmlDocument sitemap = GetSiteMap(getRootSG(), GetListStructureGroups(), regex);
        /// </code>
        /// will result in the following XML:
        /// <code>
        /// &lt;StructureGroup id="tcm:1-7-4" title="www.tridion.com" url="/">
        ///     &lt;StructureGroup id="tcm:1-10-4" title="200 Products" url="/Products">
        ///         &lt;StructureGroup id="tcm:1-11-4" title="210 R5" url="/Products/R5">
        /// 	        &lt;Page id="tcm:1-85-64" title="211 Content Creation" url="/Products/R5/ContentCreation.html"/>
        /// 	        &lt;Page id="tcm:1-95-64" title="215 Dynamic Content Broker" url="/Products/R5/DynamicContentBroker.html"/>
        /// 	        &lt;Page id="tcm:1-66-64" title="217 Archive Manager" url="/Products/R5/ArchiveManager.html"/>
        ///         &lt;/StructureGroup>
        ///         &lt;StructureGroup id="tcm:1-13-4" title="240 Interactive Web Applications" url="/Products/InteractiveWebApplications"/>
        ///     &lt;/StructureGroup>
        ///     &lt;StructureGroup id="tcm:1-14-4" title="100 Solutions" url="/Solutions">
        ///         &lt;StructureGroup id="tcm:1-22-4" title="110 For you industry" url="/Solutions/ForYouIndustry"/>
        ///         &lt;StructureGroup id="tcm:1-26-4" title="150 Download center" url="/Solutions/DownloadCenter"/>
        ///     &lt;/StructureGroup>
        ///     &lt;StructureGroup id="tcm:1-15-4" title="300 Customers" url="/Customers"/>
        ///     &lt;StructureGroup id="tcm:1-16-4" title="400 Service &amp; Support" url="/ServiceSupport"/>
        ///     &lt;StructureGroup id="tcm:1-17-4" title="500 Partners" url="/Partners">
        ///         &lt;Page id="tcm:1-111-64" title="Indivirtual" url="/Partners/Indivirtual.html"/>
        ///     &lt;/StructureGroup>
        ///     &lt;StructureGroup id="tcm:1-19-4" title="700 About Tridion" url="/AboutTridion"/>
        /// &lt;/StructureGroup>
        /// </code>
        /// </example>
        /// <seealso cref="GetListStructureGroups"/>
        /// <seealso cref="GetRootSG"/>
        protected XmlDocument GetSiteMap(ListItem rootSG, IList<ListItem> SGs, Regex regex)
        {
            return GetSiteMap(rootSG, SGs, null, null, regex);
        }

        /// <summary>
        /// Returns an XML document describing the entire navigation structure of the publication on which this 
        /// template is invoked.
        /// </summary>
        /// <param name="rootSG">the structure group from which to start building the navigation</param>
        /// <param name="SGs">the list of all structure groups in the publication, as retrieved from GetListStructureGroups</param>
        /// <param name="doc">the document to use for creating the new XML elements. If null, a new document will be created</param>
        /// <param name="parent">the element to append the new XML elements to. If null, the element(s) will be appended to the document.</param>
        /// <param name="regex">the regex that items must match to be included in the menu</param>
        /// <returns>an XML document describing the entire navigation structure of the publication on which this 
        /// template is invoked</returns>
        private XmlDocument GetSiteMap(ListItem rootSG, IList<ListItem> SGs, XmlDocument doc, XmlElement parent, Regex regex)
        {
            CheckInitialized();

            string title = rootSG.Title;

            if (MustInclude(regex, ref title) || doc == null)
            {
                if (doc == null)
                {
                    doc = new XmlDocument();
                    parent = null;
                }

                StructureGroup objSG = (StructureGroup)MEngine.GetObject(rootSG.Id);
                XmlElement elmSG = doc.CreateElement("node");
                elmSG.SetAttribute("id", objSG.Id);
                elmSG.SetAttribute("title", title);
                elmSG.SetAttribute("url", objSG.PublishLocationUrl);
                // TODO: add roles (SCD) and orgpub

                // for each child SG
                foreach (ListItem sg in SGs)
                {
                    if (sg.ParentId == rootSG.Id)
                    {
                        // recursively add SG + children
                        GetSiteMap(sg, SGs, doc, elmSG, regex);
                    }
                }

                // get the pages for this SG
                IList<Page> pages = GetPagesInSG(rootSG);

                // add the pages
                foreach (Page page in pages)
                {
                    title = page.Title;
                    if (MustInclude(regex, ref title))
                    {
                        XmlElement elmPage = doc.CreateElement("node");
                        elmPage.SetAttribute("id", page.Id);
                        elmPage.SetAttribute("title", title);
                        elmPage.SetAttribute("url", page.PublishLocationUrl);
                        // TODO: add SCD roles and orgpub
                        elmSG.AppendChild(elmPage);
                    }
                }

                if (parent != null)
                {
                    parent.AppendChild(elmSG);
                }
                else
                {
                    doc.AppendChild(elmSG);
                }
            }

            return doc;
        }

        /// <summary>
        /// Checks whether a Target Type URI is associated with the current publication target being published to
        /// </summary>
        protected bool IsTTInPublicationContext(string ttURI)
        {
            CheckInitialized();

            if (MEngine.PublishingContext.PublicationTarget != null) //not null only during publishing
            {
                foreach (TargetType tt in MEngine.PublishingContext.PublicationTarget.TargetTypes)
                {
                    if (tt.Id == ttURI) return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks whether at least one of a list of Target Type URIs is associated with the current publication target being published to
        /// </summary>
        protected bool IsTTInPublicationContext(IEnumerable<string> ttURIs)
        {
            CheckInitialized();

            if (MEngine.PublishingContext.PublicationTarget != null)//not null only during publishing
            {
                foreach (string uri in ttURIs)
                {
                    foreach (TargetType tt in MEngine.PublishingContext.PublicationTarget.TargetTypes)
                    {
                        if (tt.Id == uri) return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Checks whether there is an item in the package of type tridion/page
        /// </summary>
        /// <returns>True if there is a page item in the package</returns>
        protected bool IsPage()
        {
            Item page = MPackage.GetByType(ContentType.Page);

            return (page != null);
        }

        /// <summary>
        /// Checks whether there is an item in the package of type tridion/component
        /// </summary>
        /// <returns>True if there is a component item in the package</returns>
        protected bool IsComponent()
        {
            Item component = MPackage.GetByType(ContentType.Component);

            return (component != null);
        }

        #endregion

        #region Utilities

        protected static Regex GetRegexForParameter(string parameter)
        {
            Regex result = null;
            if (!String.IsNullOrEmpty(parameter))
            {
                string pattern = parameter.Substring(parameter.IndexOf("("));
                pattern = pattern.Substring(1, pattern.Length - 2);
                //Logger.debug("GetRegexForParameter: " + pattern);
                result = new Regex(pattern);
            }
            return result;
        }

        protected static string GetOpeningTag(string tag, string cssClass)
        {
            string menu = "<" + tag;
            if (!String.IsNullOrEmpty(cssClass))
            {
                menu += " class='" + cssClass + "'";
            }
            menu += ">" + Environment.NewLine;
            return menu;
        }

        /// <summary>
        /// Return a list of objects of the requested type from the XML.
        /// </summary>
        /// <remarks>
        /// This method goes back to the database to retrieve more information. So it is NOT just
        /// a fast and convenient way to get a type safe list from the XML.
        /// </remarks>
        /// <typeparam name="T">The type of object to return, like Publication, User, Group, OrganizationalItem</typeparam>
        /// <param name="listElement">The XML from which to construct the list of objects</param>
        /// <returns>a list of objects of the requested type from the XML</returns>
        protected IList<T> GetObjectsFromXmlList<T>(Engine engine, XmlElement listElement) where T : IdentifiableObject
        {
            XmlNodeList itemElements = listElement.SelectNodes("*");
            List<T> result = new List<T>(itemElements.Count);
            foreach (XmlElement itemElement in itemElements)
            {
                result.Add(GetObjectFromXmlElement<T>(engine, itemElement));
            }
            result.Sort(delegate(T item1, T item2)
            {
                return item1.Title.CompareTo(item2.Title);
            });
            return result;
        }

        protected T GetObjectFromXmlElement<T>(Engine engine, XmlElement itemElement) where T : IdentifiableObject
        {
            return (T)engine.GetObject(itemElement.GetAttribute("ID"));
        }

        protected static string Encode(string value)
        {
            return System.Web.HttpUtility.HtmlEncode(value);
        }

        /// <summary>
        /// Returns the root structure group from the list of structure groups specified.
        /// </summary>
        /// <exception cref="InvalidDataException">when there is no root structure group in the list</exception>
        /// <param name="items">The list of structure groups to search.</param>
        /// <returns>the root structure group from the list of structure groups specified</returns>
        protected ListItem GetRootSG(IList<ListItem> items)
        {
            foreach (ListItem item in items)
            {
                if (item.ParentId.PublicationId == -1)
                {
                    return item;
                }
            }
            throw new InvalidDataException("Could not find root structure group");
        }

        /// <summary>
        /// Returns the root structure group for the specified item
        /// </summary>
        /// <param name="item">Any item which resides in a publication</param>
        /// <returns>The Root Structure Group in the publication</returns>
        protected StructureGroup GetRootSG(RepositoryLocalObject item)
        {
            Repository pub = item.OwningRepository;

            return GetRootSG(pub);
        }

        /// <summary>
        /// Returns the root structure group for the specified publication
        /// </summary>		
        /// <returns>The Root Structure Group in the publication</returns>
        /// <remarks>copied and modified code from Repository.RootFolder :)</remarks>
        protected StructureGroup GetRootSG(Repository publication)
        {
            //Filter filter = new Filter();
            //filter.Conditions["ItemType"] = ItemType.StructureGroup;
            RepositoryItemsFilter filter = new RepositoryItemsFilter(publication.Session) { ItemTypes = new List<ItemType> { ItemType.StructureGroup } };

            IList<RepositoryLocalObject> items = (IList<RepositoryLocalObject>)publication.GetItems(filter);

            if (items.Count == 0)
                return null;
            else
                return (StructureGroup)items[0];
        }

        protected Component GetComponentValue(String fieldNAme, ItemFields fields)
        {
            if (fields.Contains(fieldNAme))
            {
                ComponentLinkField field = fields[fieldNAme] as ComponentLinkField;
                return field.Value;
            }

            return null;
        }

        protected IList<Component> GetComponentValues(string fieldName, ItemFields fields)
        {
            if (fields.Contains(fieldName))
            {
                ComponentLinkField field = (ComponentLinkField)fields[fieldName];
                return (field.Values.Count > 0) ? field.Values : null;
            }
            else
            {
                return null;
            }
        }

        protected IList<DateTime> GetDateValues(string fieldName, ItemFields fields)
        {
            if (fields.Contains(fieldName))
            {
                DateField field = (DateField)fields[fieldName];
                return (field.Values.Count > 0) ? field.Values : null;
            }
            else
            {
                return null;
            }
        }

        protected IList<Keyword> GetKeywordValues(string fieldName, ItemFields fields)
        {
            if (fields.Contains(fieldName))
            {
                KeywordField field = (KeywordField)fields[fieldName];
                return (field.Values.Count > 0) ? field.Values : null;
            }
            else
            {
                return null;
            }
        }

        protected IList<double> GetNumberValues(string fieldName, ItemFields fields)
        {
            if (fields.Contains(fieldName))
            {
                NumberField field = (NumberField)fields[fieldName];
                return (field.Values.Count > 0) ? field.Values : null;
            }
            else
            {
                return null;
            }
        }

        protected IList<string> GetStringValues(string fieldName, ItemFields fields)
        {
            if (fields.Contains(fieldName))
            {
                TextField field = (TextField)fields[fieldName];
                return (field.Values.Count > 0) ? field.Values : null;
            }
            else
            {
                return null;
            }
        }

        protected String GetSingleStringValue(string fieldName, ItemFields fields)
        {
            if (fields.Contains(fieldName))
            {
                // check if the field is a KeywordField or assume it is a TextField
                if (fields[fieldName].GetType().Equals(typeof(KeywordField)))
                {
                    KeywordField field = fields[fieldName] as KeywordField;
                    if (field != null) return field.Value.Title;
                }
                else
                {
                    TextField field = fields[fieldName] as TextField;
                    if (field != null) return field.Value;
                }
            }

            return null;
        }

        /// <summary>
        /// Extract the reference id from a tcm uri
        /// </summary>
        protected string GetItemReferenceID(String tcmuri)
        {
            int startPos;
            int stopPos;
            startPos = tcmuri.IndexOf("-");
            stopPos = tcmuri.LastIndexOf("-");
            return tcmuri.Substring(startPos + 1, stopPos - startPos - 1);
        }

        /// <summary>
        /// True if the rendering context is a page, rather than component
        /// </summary>
        protected bool IsPageTemplate
        {
            get
            {
                if (RenderContext == -1)
                {
                    if (MEngine.PublishingContext.ResolvedItem.Item is Page)
                        RenderContext = 1;
                    else
                        RenderContext = 0;
                }
                if (RenderContext == 1)
                    return true;
                else
                    return false;
            }
        }

        protected void PutMainComponentOnTop()
        {
            Item mainComponent = MPackage.GetByName("Component");
            if (mainComponent != null)
            {
                MPackage.Remove(mainComponent);
                MPackage.PushItem("Component", mainComponent);
            }
        }

        protected List<KeyValuePair<TcmUri, string>> GetOrganizationalItemContents(OrganizationalItem orgItem, ItemType itemType, bool recursive)
        {
            //Filter filter = new Filter();
            //filter.Conditions.Add("ItemType", itemType);
            //filter.Conditions.Add("Recursive", recursive);
            OrganizationalItemItemsFilter filter = new OrganizationalItemItemsFilter(orgItem.Session) { ItemTypes = new List<ItemType> { itemType }, Recursive = recursive };
            List<KeyValuePair<TcmUri, string>> res = new List<KeyValuePair<TcmUri, string>>();
            foreach (XmlNode item in orgItem.GetListItems(filter).SelectNodes("/*/*"))
            {
                string title = item.Attributes["Title"].Value;
                TcmUri id = new TcmUri(item.Attributes["ID"].Value);
                res.Add(new KeyValuePair<TcmUri, string>(id, title));
            }
            return res;
        }

        protected string GetPageOrStructureGroupTitle(RepositoryLocalObject pageOrSg)
        {
            if (pageOrSg is Page)
                return GetPageTitle(pageOrSg as Page);
            else
                return GetStructureGroupTitle(pageOrSg as StructureGroup);
        }
        protected string GetPageTitle(Page page)
        {
            string title = null;
            //Try to read the page title from the first text field in the first component on the page
            if (page.ComponentPresentations.Count > 0)
            {
                Component comp = page.ComponentPresentations[0].Component;
                ItemFields fields = new ItemFields(comp.Content, comp.Schema);
                TextField firstField = GetFirstTextField(fields);
                if (firstField != null && firstField.Values.Count > 0)
                    title = firstField.Values[0];
            }
            //Fallback, use the Page title
            if (title == null)
                title = page.Title;
            return title;
        }

        protected string GetStructureGroupTitle(StructureGroup sg)
        {
            string title = "";

            // Get metadata field "menuLabel" to use for breadcrumb.
            if (sg.Metadata != null)
            {
                try
                {
                    XmlNodeList nodeList = sg.Metadata.GetElementsByTagName("menuLabel");
                    title = nodeList.Item(0).InnerText;
                }
                catch (Exception e)
                {
                    Logger.Warning("Cannot extract title from SG metadata schema; " + e.ToString());
                }
            }
            return title;
        }

        [ObsoleteAttribute]
        protected bool IsNavigationStructureGroup(string sgTitle)
        {
            if (sgTitle.Length > 4)
            {
                string prefix = sgTitle.Substring(0, 3);
                int order;
                if (Int32.TryParse(prefix, out order) && sgTitle.Substring(3, 1) == " ")
                    return true;
            }
            return false;
        }

        protected bool IsPreview()
        {
            return (MEngine.RenderMode == RenderMode.PreviewDynamic);
        }

        private static TextField GetFirstTextField(ItemFields fields)
        {
            TextField res = null;
            foreach (ItemField field in fields)
            {
                if (field is EmbeddedSchemaField)
                {
                    res = GetFirstTextField((field as EmbeddedSchemaField).Value);
                }
                else
                {
                    res = field as TextField;
                }
                if (res != null)
                    return res;
            }
            return res;
        }

        protected List<TcmUri> GetSchemaComponentTemplatesIds(Schema schema)
        {
            //Filter filter = new Filter();
            //filter.Conditions.Add("ItemType", ItemType.ComponentTemplate);
            UsingItemsFilter filter = new UsingItemsFilter(schema.Session) { ItemTypes = new List<ItemType> { ItemType.ComponentTemplate } };
            List<TcmUri> cts = new List<TcmUri>();
            foreach (XmlNode ctNode in schema.GetListUsingItems(filter))
            {
                string id = ctNode.Attributes["ID"].Value;
                cts.Add(new TcmUri(id));
            }
            return cts;
        }

        /// <summary>
        /// Returns the page object that is defined in the package for this template.
        /// </summary>
        /// <remarks>
        /// This method should only be called when there is an actual Page item in the package. 
        /// It does not currently handle the situation where no such item is available.
        /// </remarks>
        /// <returns>the page object that is defined in the package for this template.</returns>
        protected Page GetPage()
        {
            CheckInitialized();

            Item pageItem = MPackage.GetByType(ContentType.Page);
            if (pageItem != null)
            {
                return MEngine.GetObject(pageItem.GetAsSource().GetValue("ID")) as Page;
            }

            Page page = MEngine.PublishingContext.RenderContext.ContextItem as Page;
            if (page != null)
            {
                return page;
            }

            //final resort - use Where used on component to find a page that uses it
            //this is more for previewing in the template builder than anything else
            //Filter filter = new Filter();
            //filter.Conditions.Add("ItemType", ItemType.Page);
            //filter.Conditions.Add("OnlyLatestVersions", true);
            //foreach (IdentifiableObject page in this.GetComponent().GetUsingItems(filter))
            //{
            //    if (page is Page)
            //    {
            //        Page localPage = MEngine.GetObject(MEngine.LocalizeUri(page.Id)) as Page;
            //        return localPage;
            //    }
            //}
            //Logger.Warning("Could not find a page that the component uses");

            return null;
        }

        #endregion

        #region ITemplate Members

        public virtual void Transform(Engine engine, Package package) { }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Indicates whether system resources used by this instance have been released
        /// </summary>
        protected bool Disposed
        {
            get
            {
                lock (this)
                {
                    return (_disposed);
                }
            }
        }

        /// <summary>
        /// Releases allocated resources
        /// </summary>
        void IDisposable.Dispose()
        {
            lock (this)
            {
                if (_disposed == false)
                {
                    MPackage = null;
                    MEngine = null;
                    _logger = null;
                    _nsm = null;

                    _disposed = true;
                    // take yourself off the finalization queue
                    // to prevent finalization from executing a second time
                    GC.SuppressFinalize(this);
                }
            }
        }

        #endregion
    }
}
