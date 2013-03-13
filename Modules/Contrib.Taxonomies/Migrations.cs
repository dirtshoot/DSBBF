using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace Contrib.Taxonomies {
    public class Migrations : DataMigrationImpl {

        public int Create() {
            SchemaBuilder.CreateTable("TaxonomyPartRecord", 
                table => table
                    .ContentPartRecord()
                    .Column<string>("TermTypeName", column => column.WithLength(255))
                );

            SchemaBuilder.CreateTable("TermPartRecord", 
                table => table
                    .ContentPartRecord()
                    .Column<string>("Path", column => column.WithLength(255))
                    .Column<int>("TaxonomyId")
                    .Column<int>("Count")
                    .Column<int>("Weight")
                    .Column<bool>("Selectable")
                );

            SchemaBuilder.CreateTable("TermContentItem", 
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<string>("Field", column => column.WithLength(50))
                    .Column<int>("TermRecord_id")
                    .Column<int>("ContentItemRecord_id")
                );

            SchemaBuilder.CreateTable("TaxonomyWidgetPartRecord",
                table => table
                    .ContentPartRecord()
                    .Column<int>("TaxonomyPartRecord_id")
                );

            ContentDefinitionManager.AlterTypeDefinition("Taxonomy",
                 cfg => cfg
                     .WithPart("TaxonomyPart")
                     .WithPart("CommonPart")
                     .WithPart("RoutePart")
                 );

            ContentDefinitionManager.AlterTypeDefinition("TaxonomyWidget",
                cfg => cfg
                    .WithPart("TaxonomyWidgetPart")
                    .WithPart("CommonPart")
                    .WithPart("WidgetPart")
                    .WithSetting("Stereotype", "Widget")
                );

            return 1;
        }

        public int UpdateFrom1() {
            SchemaBuilder.CreateTable("TaxonomyMenuItemPartRecord",
                table => table
                    .ContentPartRecord()
                    .Column<bool>("RenderMenuItem")
                    .Column<string>("Position", c => c.WithLength(30))
                    .Column<string>("Name", c => c.WithLength(255))
                );

            ContentDefinitionManager.AlterPartDefinition("TaxonomyMenuItemPart", builder => builder.Attachable());

            return 2;
        }
    }
}