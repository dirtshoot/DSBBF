﻿using System;
using System.Web.Routing;
using Autofac;
using JetBrains.Annotations;
using Orchard.Caching;
using Orchard.Commands;
using Orchard.Commands.Builtin;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.Core.Settings.Models;
using Orchard.Data.Migration.Interpreters;
using Orchard.Data.Providers;
using Orchard.Data.Migration;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.Descriptors.ShapeAttributeStrategy;
using Orchard.DisplayManagement.Descriptors.ShapeTemplateStrategy;
using Orchard.DisplayManagement.Implementation;
using Orchard.Environment;
using Orchard.Environment.Extensions.Models;
using Orchard.Localization;
using Orchard.Mvc;
using Orchard.Mvc.ModelBinders;
using Orchard.Mvc.Routes;
using Orchard.Mvc.ViewEngines;
using Orchard.Mvc.ViewEngines.Razor;
using Orchard.Mvc.ViewEngines.ThemeAwareness;
using Orchard.Mvc.ViewEngines.WebForms;
using Orchard.Recipes.Services;
using Orchard.Settings;
using Orchard.Themes;
using Orchard.UI.Notify;
using Orchard.UI.PageClass;
using Orchard.UI.PageTitle;
using Orchard.UI.Resources;
using Orchard.UI.Zones;
using IFilterProvider = Orchard.Mvc.Filters.IFilterProvider;

namespace Orchard.Setup {
    public class SetupMode : Module {
        public Feature Feature { get; set; }

        protected override void Load(ContainerBuilder builder) {

            // standard services needed in setup mode
            builder.RegisterModule(new MvcModule());
            builder.RegisterModule(new CommandModule());
            builder.RegisterModule(new WorkContextModule());
            builder.RegisterModule(new CacheModule());

            builder.RegisterType<RoutePublisher>().As<IRoutePublisher>().InstancePerLifetimeScope();
            builder.RegisterType<ModelBinderPublisher>().As<IModelBinderPublisher>().InstancePerLifetimeScope();
            builder.RegisterType<WebFormViewEngineProvider>().As<IViewEngineProvider>().As<IShapeTemplateViewEngine>().InstancePerLifetimeScope();
            builder.RegisterType<RazorViewEngineProvider>().As<IViewEngineProvider>().As<IShapeTemplateViewEngine>().InstancePerLifetimeScope();
            builder.RegisterType<ThemedViewResultFilter>().As<IFilterProvider>().InstancePerLifetimeScope();
            builder.RegisterType<ThemeFilter>().As<IFilterProvider>().InstancePerLifetimeScope();
            builder.RegisterType<PageTitleBuilder>().As<IPageTitleBuilder>().InstancePerLifetimeScope();
            builder.RegisterType<PageClassBuilder>().As<IPageClassBuilder>().InstancePerLifetimeScope();
            builder.RegisterType<Notifier>().As<INotifier>().InstancePerLifetimeScope();
            builder.RegisterType<NotifyFilter>().As<IFilterProvider>().InstancePerLifetimeScope();
            builder.RegisterType<DataServicesProviderFactory>().As<IDataServicesProviderFactory>().InstancePerLifetimeScope();
            builder.RegisterType<DefaultCommandManager>().As<ICommandManager>().InstancePerLifetimeScope();
            builder.RegisterType<HelpCommand>().As<ICommandHandler>().InstancePerLifetimeScope();
            builder.RegisterType<WorkContextAccessor>().As<IWorkContextAccessor>().InstancePerMatchingLifetimeScope("shell");
            builder.RegisterType<ResourceManager>().As<IResourceManager>().InstancePerLifetimeScope();
            builder.RegisterType<ResourceFilter>().As<IFilterProvider>().InstancePerLifetimeScope();

            // setup mode specific implementations of needed service interfaces
            builder.RegisterType<SafeModeThemeService>().As<IThemeManager>().InstancePerLifetimeScope();
            builder.RegisterType<SafeModeText>().As<IText>().InstancePerLifetimeScope();
            builder.RegisterType<SafeModeSiteService>().As<ISiteService>().InstancePerLifetimeScope();

            builder.RegisterType<DefaultDataMigrationInterpreter>().As<IDataMigrationInterpreter>().InstancePerLifetimeScope();
            builder.RegisterType<DataMigrationManager>().As<IDataMigrationManager>().InstancePerLifetimeScope();

            builder.RegisterType<RecipeHarvester>().As<IRecipeHarvester>().InstancePerLifetimeScope();
            builder.RegisterType<RecipeParser>().As<IRecipeParser>().InstancePerLifetimeScope();

            builder.RegisterType<DefaultCacheHolder>().As<ICacheHolder>().InstancePerLifetimeScope();

            // in progress - adding services for display/shape support in setup
            builder.RegisterType<DisplayHelperFactory>().As<IDisplayHelperFactory>();
            builder.RegisterType<DefaultDisplayManager>().As<IDisplayManager>();
            builder.RegisterType<DefaultShapeFactory>().As<IShapeFactory>();
            builder.RegisterType<DefaultShapeTableManager>().As<IShapeTableManager>();

            builder.RegisterType<ThemeAwareViewEngine>().As<IThemeAwareViewEngine>();
            builder.RegisterType<LayoutAwareViewEngine>().As<ILayoutAwareViewEngine>();
            builder.RegisterType<ConfiguredEnginesCache>().As<IConfiguredEnginesCache>();
            builder.RegisterType<LayoutWorkContext>().As<IWorkContextStateProvider>();
            builder.RegisterType<SafeModeSiteWorkContextProvider>().As<IWorkContextStateProvider>();

            builder.RegisterType<ShapeTemplateBindingStrategy>().As<IShapeTableProvider>();
            builder.RegisterType<BasicShapeTemplateHarvester>().As<IShapeTemplateHarvester>();
            builder.RegisterType<ShapeAttributeBindingStrategy>().As<IShapeTableProvider>();
            builder.RegisterModule(new ShapeAttributeBindingModule());
        }


        [UsedImplicitly]
        class SafeModeText : IText {
            public LocalizedString Get(string textHint, params object[] args) {
                if (args == null || args.Length == 0) {
                    return new LocalizedString(textHint);
                }
                return new LocalizedString(string.Format(textHint, args));
            }
        }

        [UsedImplicitly]
        class SafeModeThemeService : IThemeManager {
            private readonly ExtensionDescriptor _theme = new ExtensionDescriptor {
                Id = "SafeMode",
                Name = "SafeMode",
                Location = "~/Themes",
            };

            public ExtensionDescriptor GetRequestTheme(RequestContext requestContext) { return _theme; }
        }

        [UsedImplicitly]
        class SafeModeSiteWorkContextProvider : IWorkContextStateProvider {
            public Func<WorkContext, T> Get<T>(string name) {
                if (name == "CurrentSite") {
                    ISite safeModeSite = new SafeModeSite();
                    return ctx => (T)safeModeSite;
                }
                return null;
            }
        }

        [UsedImplicitly]
        class SafeModeSiteService : ISiteService {
            public ISite GetSiteSettings() {
                var siteType = new ContentTypeDefinitionBuilder().Named("Site").Build();
                var site = new ContentItemBuilder(siteType)
                    .Weld<SafeModeSite>()
                    .Build();

                return site.As<ISite>();
            }
        }

        class SafeModeSite : ContentPart, ISite {
            public string PageTitleSeparator {
                get { return "*"; }
            }

            public string SiteName {
                get { return "Orchard Setup"; }
            }

            public string SiteSalt {
                get { return "42"; }
            }

            public string SiteUrl {
                get { return "/"; }
            }

            public string SuperUser {
                get { return ""; }
            }

            public string HomePage {
                get { return ""; }
                set { throw new NotImplementedException(); }
            }

            public string SiteCulture {
                get { return ""; }
                set { throw new NotImplementedException(); }
            }

            public ResourceDebugMode ResourceDebugMode {
                get { return ResourceDebugMode.FromAppSetting; }
                set { throw new NotImplementedException(); }
            }

            public int PageSize {
                get { return SiteSettingsPartRecord.DefaultPageSize; }
                set { throw new NotImplementedException(); }
            }

            public string BaseUrl {
                get { return ""; }
            }
        }
    }
}
