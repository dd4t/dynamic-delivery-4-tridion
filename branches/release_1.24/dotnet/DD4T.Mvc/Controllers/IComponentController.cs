using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using DD4T.ContentModel.Factories;

namespace DD4T.Mvc.Controllers
{
    public interface IComponentController : IController
    {
        IComponentFactory ComponentFactory { get; set; }
    }
}
