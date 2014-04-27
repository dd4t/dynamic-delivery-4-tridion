using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using DD4T.ContentModel.Contracts.Factories;
using DD4T.Mvc.Html;

namespace DD4T.Mvc.Controllers
{
    public interface IPageController : IController
    {
        IPageFactory PageFactory { get; set; }
        IComponentPresentationRenderer ComponentPresentationRenderer { get; set; }
    }
}
