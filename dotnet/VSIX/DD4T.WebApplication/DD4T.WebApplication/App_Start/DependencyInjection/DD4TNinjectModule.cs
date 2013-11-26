using System;
using DD4T.ContentModel.Contracts.Providers;
using DD4T.ContentModel.Factories;
using DD4T.Factories;
using DD4T.Mvc.Controllers;
using DD4T.Mvc.Html;
using Ninject;
using Ninject.Modules;
using DD4T.WebApplication.Controllers;

// TODO: Change this line to your providers if you are not using 2013
using DD4T.Providers.SDLTridion2013;

namespace DD4T.WebApplication.DependencyInjection
{
    ///<summary>
    /// DD4TNinjectModule is responsible for setting up DD4T bindings for Ninject
    /// 
    /// To change provider, change the using statement at the top of this file to the relevant provider version 
    /// e.g. using DD4T.Providers.SDLTridion2013 to using DD4T.Providers.SDLTridion2011Sp1
    /// 
    /// Author: Robert Stevenson-Leggett
    /// Date: 2012-10-22
    ///</summary>
    public class DD4TNinjectModule : NinjectModule
    {
        private int _publicationId;

        public DD4TNinjectModule(int publicationId)
        {
            _publicationId = publicationId;
        }

        public override void Load()
        {
            Bind<IPageProvider>().ToMethod(context => new TridionPageProvider() { PublicationId = _publicationId });
            Bind<ILinkProvider>().To<TridionLinkProvider>();

            Bind<IPageFactory>().ToMethod(context => new PageFactory()
            {
                PageProvider = context.Kernel.Get<IPageProvider>(),
                ComponentFactory = context.Kernel.Get<IComponentFactory>(),
                LinkFactory = context.Kernel.Get<ILinkFactory>()
            });

            Bind<ILinkFactory>().ToMethod(context => new LinkFactory()
            {
                LinkProvider = context.Kernel.Get<ILinkProvider>()
            });

            Bind<PageController>().ToMethod(context => new PageController()
            {
                PageFactory = context.Kernel.Get<IPageFactory>(),
                ComponentPresentationRenderer = context.Kernel.Get<IComponentPresentationRenderer>()
            });

            Bind<IComponentController>().ToMethod(context => new ComponentController() { ComponentFactory = context.Kernel.Get<IComponentFactory>() });

            Bind<IComponentProvider>().To<TridionComponentProvider>().InSingletonScope();
            Bind<IComponentFactory>().To<ComponentFactory>().InSingletonScope();
            Bind<IComponentPresentationRenderer>().To<DefaultComponentPresentationRenderer>().InSingletonScope();
        }
    }
}