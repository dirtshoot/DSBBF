using System.Collections.Generic;
using NGM.BlogML.Models;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.Data.Migration;

namespace NGM.BlogML {
    public class Migrations : DataMigrationImpl {
        public int Create() {
            SchemaBuilder.CreateTable("BlogMLSettingsPartRecord", table => table
                .ContentPartRecord()
                .Column<string>("AttachmentDirectory")
                .Column<string>("ExportDirectory")
               );

            ContentDefinitionManager.AlterTypeDefinition("Blog",
               cfg => cfg
                   .WithPart("ExportContainerPart")
                );
        
            //ContentDefinitionManager.AlterPartDefinition(typeof(ExportContainerPart).Name, cfg => cfg
            //    .WithLocation(new Dictionary<string, ContentLocation> {
            //        {"DetailAdmin", new ContentLocation { Zone = "manage", Position = null }},
            //    }));

            return 1;
        }
    }
}