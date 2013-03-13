using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace Szmyd.Orchard.Modules.Sharing
{
    public class SharingDataMigration : DataMigrationImpl
    {
        public int Create()
        {
            
            SchemaBuilder.CreateTable("ShareBarSettingsPartRecord",
                                      table => table
                                                   .ContentPartRecord()
                                                   .Column<string>("AddThisAccount")
                );


            ContentDefinitionManager.AlterPartDefinition("ShareBarPart", builder => builder.Attachable());
            
            return 1;
        }
    }
}