[assembly: WebActivator.PreApplicationStartMethod(typeof(SampleWebsite.AppStart_MefContribMVC3), "Start")]

namespace SampleWebsite
{
    using System.ComponentModel.Composition.Hosting;
    using System.Linq;
    using System.Web.Mvc;
    using MefContrib.Hosting.Conventions;
    using MefContrib.Web.Mvc;
    using System.Diagnostics;
    using System;

    public static class AppStart_MefContribMVC3
    {
        public static AggregateCatalog catalog = null;

        // Ivm DI moet mag catalog niet worden opgeruimd, voordat de applicatie afsluit
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")] 
        public static void Start()
        {

            // Register the CompositionContainerLifetimeHttpModule HttpModule.
            // This makes sure everything is cleaned up correctly after each request.
            CompositionContainerLifetimeHttpModule.Register();

            // Create MEF catalog based on the contents of ~/bin.
            //
            // Note that any class in the referenced assemblies implementing in "IController"
            // is automatically exported to MEF. There is no need for explicit [Export] attributes
            // on ASP.NET MVC controllers. When implementing multiple constructors ensure that
            // there is one constructor marked with the [ImportingConstructor] attribute.
            TypeCatalog typeCatalog = new TypeCatalog(typeof(Tridion.Extensions.DynamicDelivery.Factories.TridionBinaryFactory2011),
                    typeof(Tridion.Extensions.DynamicDelivery.Factories.TridionPageFactory),
                    typeof(Tridion.Extensions.DynamicDelivery.Factories.TridionLinkFactory));
            DirectoryCatalog dirCatalog = new DirectoryCatalog(@"bin\mef");
            DirectoryCatalog dirCatalog2 = new DirectoryCatalog(@"D:\Temp\Test");
            MvcApplicationRegistry mvcAppRegistry = null;
            try
            {
                mvcAppRegistry = new MvcApplicationRegistry();
            }
            catch (Exception ex)
            {
                Trace.WriteLine("caught exception " + ex.Message);
            }
            ConventionCatalog conventionCatalog = new ConventionCatalog(mvcAppRegistry);

            catalog = new AggregateCatalog(
                dirCatalog,
                conventionCatalog
                ); // Note: add your own (convention)catalogs here if needed.

            // Tell MVC3 to use MEF as its dependency resolver.
            var dependencyResolver = new CompositionDependencyResolver(catalog);
            DependencyResolver.SetResolver(dependencyResolver);

            // Tell MVC3 to resolve dependencies in controllers
            ControllerBuilder.Current.SetControllerFactory(
                new DefaultControllerFactory(
                    new CompositionControllerActivator(dependencyResolver)));

            // Tell MVC3 to resolve dependencies in filters
            FilterProviders.Providers.Remove(FilterProviders.Providers.Single(f => f is FilterAttributeFilterProvider));
            FilterProviders.Providers.Add(new CompositionFilterAttributeFilterProvider(dependencyResolver));

            // Tell MVC3 to resolve dependencies in model validators
            ModelValidatorProviders.Providers.Remove(ModelValidatorProviders.Providers.OfType<DataAnnotationsModelValidatorProvider>().Single());
            ModelValidatorProviders.Providers.Add(
                new CompositionDataAnnotationsModelValidatorProvider(dependencyResolver));

            // Tell MVC3 to resolve model binders through MEF. Note that a model binder should be decorated
            // with [ModelBinderExport].
            ModelBinderProviders.BinderProviders.Add(
                new CompositionModelBinderProvider(dependencyResolver));
        }
    }
} 