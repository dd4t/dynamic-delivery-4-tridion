using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Security;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Specialized;
using System.Configuration;
using Microsoft.Practices.Unity;
using DD4T.Mvc.Controllers;
using DD4T.ContentModel;
using DD4T.ContentModel.Exceptions;
using DD4T.Mvc;
using DD4T.Mvc.Html;
using DD4T.Examples.Unity;
using DD4T.Utils;
using DD4T.ContentModel.Factories;
using Trivident.DD4T.Examples.PublicationResolvers;

namespace DD4T.Examples.Controllers
{
    public class PageController : TridionControllerBase
    {



        private IPageFactory _pageFactory = null;
        public override ContentModel.Factories.IPageFactory PageFactory
        {
            get
            {
                if (_pageFactory == null)
                {
                    _pageFactory = base.PageFactory;
                    _pageFactory.PublicationResolver = new HostNamePublicationResolver();
                }
                return _pageFactory;
            }
        }

        #region private members
        //private Regex reDefaultPage = new Regex(@".*/[^/\.]*$");
        private Regex reDefaultPage = new Regex(@".*/[^/\.]*(/?)$");
        private string defaultPageFileName = null;
        #endregion

        #region constructor(s)
        public PageController()
            : base()
        {
            componentPresentationRenderer = UnityHelper.Container.Resolve<IComponentPresentationRenderer>();
        }
        #endregion

        #region public properties
        public string DefaultPageFileName
        {
            get
            {
                if (defaultPageFileName == null)
                {
                    defaultPageFileName = ConfigurationManager.AppSettings["DD4T.DefaultPage"];
                }
                return defaultPageFileName;
            }
        }
        #endregion

        #region MVC
        /// <summary>
        /// Create IPage from XML in the broker and forward to the view
        /// </summary>
        /// <param name="pageId"></param>
        /// <returns></returns>
#if (!DEBUG)
        [OutputCache(CacheProfile = "ControllerCache")]
#endif
        [HandleError]
        [AcceptVerbs(HttpVerbs.Get)]
        public override System.Web.Mvc.ActionResult Page(string pageId)
        {
            if (string.IsNullOrEmpty(pageId))
            {
                pageId = DefaultPageFileName;
            }
            else
            {
                if (reDefaultPage.IsMatch("/" + pageId))
                {
                    if (pageId.EndsWith("/"))
                    {
                        pageId += DefaultPageFileName;
                    }
                    else
                    {
                        pageId += "/" + DefaultPageFileName;
                    }
                }
            }
            try
            {
                return base.Page(pageId);
            }
            catch (SecurityException se)
            {
                throw new HttpException(403, se.Message);
            }
        }

        /// <summary>
        /// Create IPage from XML and forward to the view
        /// </summary>
        /// <remarks>Todo: include this in framework, URL rewriting for images, JS, CSS, etc</remarks>
        /// <returns></returns>
        [HandleError]
        [AcceptVerbs(HttpVerbs.Post)]
        public System.Web.Mvc.ActionResult PreviewPage()
        {
            try
            {
                using (StreamReader reader = new StreamReader(this.Request.InputStream))
                {
                    string pageXml = reader.ReadToEnd();
                    IPage model = this.PageFactory.GetIPageObject(pageXml);
                    if (model == null)
                    {
                        throw new ModelNotCreatedException("--unknown--");
                    }
                    ViewBag.Title = model.Title;
                    ViewBag.Renderer = ComponentPresentationRenderer;
                    return GetView(model);
                }
            }
            catch (SecurityException se)
            {
                throw new HttpException(403, se.Message);
            }
        }

        /// <summary>
        /// Execute search, add results to viewbag and execute standard action result
        /// </summary>
        /// <param name="pageId"></param>
        /// <returns></returns>
        [HandleError]
        [AcceptVerbs(HttpVerbs.Post)]
        public System.Web.Mvc.ActionResult Search(string pageId)
        {
            using (StreamReader reader = new StreamReader(this.Request.InputStream))
            {
                NameValueCollection queryString = HttpUtility.ParseQueryString(reader.ReadToEnd());
                string query = queryString.Get("query");         
                List<string> searchResults = new List<string>();

                // ToDo: implement actual search
                if (query.ToLower().Equals("test"))
                {
                    searchResults.Add("first example result");
                    searchResults.Add("second example result");
                    searchResults.Add("third example result");
                }

                // add result to viewbag
                ViewBag.SearchResults = searchResults;
                ViewBag.SearchQuery = query;
                ViewBag.ShowSearchResults = true;

                return Page("Search/" + pageId);
            }
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public System.Web.Mvc.ActionResult Xml(string pageId)
        {
            LoggerService.Information(">>PageController.Xml (url={0})", pageId);

            pageId = ParseUrl(pageId);
            if (!pageId.StartsWith("/"))
                pageId = "/" + pageId;

            return GetPlainContent(pageId, "text/xml", Encoding.UTF8);
        }



        #endregion

        #region private

        public System.Web.Mvc.ContentResult GetPlainContent(string url, string contentType, System.Text.Encoding encoding)
        {
            try
            {

                string raw = PageFactory.FindPageContent(url);
                return new ContentResult
                {
                    ContentType = contentType,
                    Content = raw,
                    ContentEncoding = encoding
                };
            }
            catch (PageNotFoundException)
            {
                throw new HttpException(404, "Page cannot be found");
            }
            catch (SecurityException se)
            {
                throw new HttpException(403, se.Message);
            }
        }

        private string ParseUrl(string pageId)
        {
            if (string.IsNullOrEmpty(pageId))
            {
                pageId = DefaultPageFileName;
            }
            else
            {
                if (reDefaultPage.IsMatch("/" + pageId))
                {
                    if (pageId.EndsWith("/"))
                    {
                        pageId += DefaultPageFileName;
                    }
                    else
                    {
                        pageId += "/" + DefaultPageFileName;
                    }
                }
            }
            return pageId;

        }
        #endregion
    }
}
