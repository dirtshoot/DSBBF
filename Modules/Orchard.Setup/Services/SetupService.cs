﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using Orchard.ContentManagement;
using Orchard.Core.Settings.Descriptor.Records;
using Orchard.Core.Settings.Models;
using Orchard.Data;
using Orchard.Data.Migration;
using Orchard.Data.Migration.Interpreters;
using Orchard.Data.Migration.Schema;
using Orchard.Environment;
using Orchard.Environment.Configuration;
using Orchard.Environment.Descriptor;
using Orchard.Environment.Descriptor.Models;
using Orchard.Environment.ShellBuilders;
using Orchard.Environment.State;
using Orchard.Localization;
using Orchard.Localization.Services;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;
using Orchard.Reports.Services;
using Orchard.Security;
using Orchard.Settings;
using Orchard.Utility.Extensions;

namespace Orchard.Setup.Services {
    public class SetupService : ISetupService {
        private readonly ShellSettings _shellSettings;
        private readonly IOrchardHost _orchardHost;
        private readonly IShellSettingsManager _shellSettingsManager;
        private readonly IShellContainerFactory _shellContainerFactory;
        private readonly ICompositionStrategy _compositionStrategy;
        private readonly IProcessingEngine _processingEngine;
        private readonly IRecipeHarvester _recipeHarvester;
        private readonly IEnumerable<Recipe> _recipes;

        public SetupService(
            ShellSettings shellSettings,
            IOrchardHost orchardHost,
            IShellSettingsManager shellSettingsManager,
            IShellContainerFactory shellContainerFactory,
            ICompositionStrategy compositionStrategy,
            IProcessingEngine processingEngine,
            IRecipeHarvester recipeHarvester) {
            _shellSettings = shellSettings;
            _orchardHost = orchardHost;
            _shellSettingsManager = shellSettingsManager;
            _shellContainerFactory = shellContainerFactory;
            _compositionStrategy = compositionStrategy;
            _processingEngine = processingEngine;
            _recipeHarvester = recipeHarvester;
            _recipes = _recipeHarvester.HarvestRecipes("Orchard.Setup");
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public ShellSettings Prime() {
            return _shellSettings;
        }

        public IEnumerable<Recipe> Recipes() {
            return _recipes;
        }

        public string Setup(SetupContext context) {
            string executionId = null;
            // The vanilla Orchard distibution has the following features enabled.
            if (context.EnabledFeatures == null || context.EnabledFeatures.Count() == 0) {
                string[] hardcoded = {
                    // Framework
                    "Orchard.Framework",
                    // Core
                    "Common", "Containers", "Contents", "Dashboard", "Feeds", "HomePage", "Navigation", "Reports", "Routable", "Scheduling", "Settings", "Shapes",
                    // Modules
                    "Orchard.Pages", "Orchard.Themes", "Orchard.Users", "Orchard.Roles", "Orchard.Modules", 
                    "PackagingServices", "Orchard.Packaging", "Gallery", "Orchard.Recipes",
                };

                context.EnabledFeatures = hardcoded;
            }

            var shellSettings = new ShellSettings(_shellSettings);

            if (string.IsNullOrEmpty(shellSettings.DataProvider)) {
                shellSettings.DataProvider = context.DatabaseProvider;
                shellSettings.DataConnectionString = context.DatabaseConnectionString;
                shellSettings.DataTablePrefix = context.DatabaseTablePrefix;
            }

            #region Encryption Settings

            shellSettings.EncryptionAlgorithm = "AES";
            // randomly generated key
            shellSettings.EncryptionKey = SymmetricAlgorithm.Create(shellSettings.EncryptionAlgorithm).Key.ToHexString();

            shellSettings.HashAlgorithm = "HMACSHA256";
            // randomly generated key
            shellSettings.HashKey = HMAC.Create(shellSettings.HashAlgorithm).Key.ToHexString();
            
            #endregion

            var shellDescriptor = new ShellDescriptor {
                Features = context.EnabledFeatures.Select(name => new ShellFeature { Name = name })
            };

            var shellBlueprint = _compositionStrategy.Compose(shellSettings, shellDescriptor);

            // initialize database explicitly, and store shell descriptor
            var bootstrapLifetimeScope = _shellContainerFactory.CreateContainer(shellSettings, shellBlueprint);

            using (var environment = bootstrapLifetimeScope.CreateWorkContextScope()) {

                // check if the database is already created (in case an exception occured in the second phase)
                var shellDescriptorRepository = environment.Resolve<IRepository<ShellDescriptorRecord>>();
                try {
                    shellDescriptorRepository.Get(x => true);
                }
                catch {
                    var schemaBuilder = new SchemaBuilder(environment.Resolve<IDataMigrationInterpreter>());
                    var reportsCoordinator = environment.Resolve<IReportsCoordinator>();

                    reportsCoordinator.Register("Data Migration", "Setup", "Orchard installation");

                    schemaBuilder.CreateTable("Orchard_Framework_DataMigrationRecord",
                                              table => table
                                                           .Column<int>("Id", column => column.PrimaryKey().Identity())
                                                           .Column<string>("DataMigrationClass")
                                                           .Column<int>("Version"));

                    var dataMigrationManager = environment.Resolve<IDataMigrationManager>();
                    dataMigrationManager.Update("Settings");

                    foreach (var feature in context.EnabledFeatures) {
                        dataMigrationManager.Update(feature);
                    }

                    environment.Resolve<IShellDescriptorManager>().UpdateShellDescriptor(
                        0,
                        shellDescriptor.Features,
                        shellDescriptor.Parameters);
                }
            }

            // in effect "pump messages" see PostMessage circa 1980
            while ( _processingEngine.AreTasksPending() )
                _processingEngine.ExecuteNextTask();

            // creating a standalone environment. 
            // in theory this environment can be used to resolve any normal components by interface, and those
            // components will exist entirely in isolation - no crossover between the safemode container currently in effect

            // must mark state as Running - otherwise standalone enviro is created "for setup"
            shellSettings.State = new TenantState("Running");
            using (var environment = _orchardHost.CreateStandaloneEnvironment(shellSettings)) {
                try {
                    executionId = CreateTenantData(context, environment);
                }
                catch {
                    environment.Resolve<ITransactionManager>().Cancel();
                    throw;
                }
            }

            _shellSettingsManager.SaveSettings(shellSettings);

            return executionId;
        }

        private string CreateTenantData(SetupContext context, IWorkContextScope environment) {
            string executionId = null;

            // create superuser
            var membershipService = environment.Resolve<IMembershipService>();
            var user =
                membershipService.CreateUser(new CreateUserParams(context.AdminUsername, context.AdminPassword,
                                                                  String.Empty, String.Empty, String.Empty,
                                                                  true));

            // set superuser as current user for request (it will be set as the owner of all content items)
            var authenticationService = environment.Resolve<IAuthenticationService>();
            authenticationService.SetAuthenticatedUserForRequest(user);

            // set site name and settings
            var siteService = environment.Resolve<ISiteService>();
            var siteSettings = siteService.GetSiteSettings().As<SiteSettingsPart>();
            siteSettings.Record.SiteSalt = Guid.NewGuid().ToString("N");
            siteSettings.Record.SiteName = context.SiteName;
            siteSettings.Record.SuperUser = context.AdminUsername;
            siteSettings.Record.SiteCulture = "en-US";

            // add default culture
            var cultureManager = environment.Resolve<ICultureManager>();
            cultureManager.AddCulture("en-US");

            var recipeManager = environment.Resolve<IRecipeManager>();
            executionId = recipeManager.Execute(Recipes().Where(r => r.Name.Equals(context.Recipe, StringComparison.OrdinalIgnoreCase)).FirstOrDefault());

            // null check: temporary fix for running setup in command line
            if (HttpContext.Current != null) {
                authenticationService.SignIn(user, true);
            }

            return executionId;
        }
    }
}