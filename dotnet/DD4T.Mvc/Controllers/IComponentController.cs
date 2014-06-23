using System.Web.Mvc;
using DD4T.ContentModel.Contracts.Factories;

namespace DD4T.Mvc.Controllers
{
    public interface IComponentController : IController
    {
        IComponentFactory ComponentFactory { get; set; }
    }
}
