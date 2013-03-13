using System.Data;
using Orchard.ContentManagement.MetaData;
using Orchard.Data.Migration;

namespace FeaturedItemSlider {
    public class Migrations : DataMigrationImpl {

        public int Create() {
            SchemaBuilder.CreateTable("FeaturedItemGroupPartRecord", builder => builder
                .ContentPartRecord()
                .Column<string>("Name", col => col.WithLength(100))
                .Column<int>("GroupWidth")
                .Column<int>("GroupHeight")
                .Column<int>("ImageWidth")
                .Column<int>("ImageHeight")
                .Column<string>("BackgroundColor")
                .Column<string>("ForegroundColor"));

            ContentDefinitionManager.AlterTypeDefinition("FeaturedItemGroup", builder => builder
                .DisplayedAs("Featured Item Group")
                .WithPart("FeaturedItemGroupPart")
                .WithPart("CommonPart")
                .WithPart("IdentityPart")
                );

            SchemaBuilder.CreateTable("FeaturedItemPartRecord", builder => builder
                    .ContentPartRecord()
                    .Column<string>("Headline", col => col.WithLength(100))
                    .Column<string>("SubHeadline", col => col.WithLength(100))
                    .Column<string>("LinkUrl", col => col.WithLength(500))
                    .Column<string>("GroupName", col => col.WithLength(100))
                );

            ContentDefinitionManager.AlterTypeDefinition("FeaturedItem", builder => builder
                    .DisplayedAs("Featured Item")
                    .WithPart("FeaturedItemPart")
                    .WithPart("CommonPart")
                    .WithPart("IdentityPart")
                );

            ContentDefinitionManager.AlterPartDefinition("FeaturedItemPart", builder => builder
                    .WithField("Picture", cfg => cfg.OfType("MediaPickerField"))
                );

            SchemaBuilder.CreateTable("FeaturedItemSliderWidgetPartRecord", builder => builder
                .ContentPartRecord()
                .Column<string>("GroupName", col => col.WithLength(100)));

            ContentDefinitionManager.AlterTypeDefinition("FeaturedItemSliderWidget", builder => builder
                    .WithPart("FeaturedItemSliderWidgetPart")
                    .WithPart("CommonPart")
                    .WithPart("WidgetPart")
                    .WithPart("IdentityPart")
                    .WithSetting("Stereotype", "Widget")
                );

            return 1;
        }

        public int UpdateFrom1() {
            SchemaBuilder.AlterTable("FeaturedItemGroupPartRecord", builder => builder.AddColumn<int>("SlideSpeed", cfg => cfg.WithDefault(300)));
            SchemaBuilder.AlterTable("FeaturedItemGroupPartRecord", builder => builder.AddColumn<int>("SlidePause", cfg => cfg.WithDefault(6000)));

            return 2;
        }

        public int UpdateFrom2() {
            SchemaBuilder.AlterTable("FeaturedItemPartRecord", builder => builder.AddColumn<string>("HeadlineTemp", cfg => cfg.WithLength(100)));
            SchemaBuilder.AlterTable("FeaturedItemPartRecord", builder => builder.AddColumn<string>("SubHeadlineTemp", cfg => cfg.WithLength(100)));
            SchemaBuilder.AlterTable("FeaturedItemPartRecord", builder => builder.AddColumn<string>("LinkUrlTemp", cfg => cfg.WithLength(500)));

            SchemaBuilder.ExecuteSql("UPDATE FeaturedItemSlider_FeaturedItemPartRecord SET HeadlineTemp = Headline");
            SchemaBuilder.ExecuteSql("UPDATE FeaturedItemSlider_FeaturedItemPartRecord SET SubHeadlineTemp = SubHeadline");
            SchemaBuilder.ExecuteSql("UPDATE FeaturedItemSlider_FeaturedItemPartRecord SET LinkUrlTemp = LinkUrl");

            SchemaBuilder.AlterTable("FeaturedItemPartRecord", builder => builder.DropColumn("Headline"));
            SchemaBuilder.AlterTable("FeaturedItemPartRecord", builder => builder.DropColumn("SubHeadline"));
            SchemaBuilder.AlterTable("FeaturedItemPartRecord", builder => builder.DropColumn("LinkUrl"));

            SchemaBuilder.AlterTable("FeaturedItemPartRecord", builder => builder.AddColumn<string>("Headline", cfg => cfg.Unlimited()));
            SchemaBuilder.AlterTable("FeaturedItemPartRecord", builder => builder.AddColumn<string>("SubHeadline", cfg => cfg.Unlimited()));
            SchemaBuilder.AlterTable("FeaturedItemPartRecord", builder => builder.AddColumn<string>("LinkUrl", cfg => cfg.Unlimited()));

            SchemaBuilder.ExecuteSql("UPDATE FeaturedItemSlider_FeaturedItemPartRecord SET Headline = HeadlineTemp");
            SchemaBuilder.ExecuteSql("UPDATE FeaturedItemSlider_FeaturedItemPartRecord SET SubHeadline = SubHeadlineTemp");
            SchemaBuilder.ExecuteSql("UPDATE FeaturedItemSlider_FeaturedItemPartRecord SET LinkUrl = LinkUrlTemp");

            SchemaBuilder.AlterTable("FeaturedItemPartRecord", builder => builder.DropColumn("HeadlineTemp"));
            SchemaBuilder.AlterTable("FeaturedItemPartRecord", builder => builder.DropColumn("SubHeadlineTemp"));
            SchemaBuilder.AlterTable("FeaturedItemPartRecord", builder => builder.DropColumn("LinkUrlTemp"));

            return 3;
        }

        public int UpdateFrom3() {
            SchemaBuilder.AlterTable("FeaturedItemPartRecord", builder =>
                builder.AddColumn<int>("SlideOrder", col => {
                    col.WithDefault(0);
                    col.NotNull();
                }));

            return 4;
        }

        public int UpdateFrom4() {
            SchemaBuilder.AlterTable("FeaturedItemGroupPartRecord", builder =>
                builder.AddColumn<bool>("ShowSlideNumbers", col => {
                    col.WithDefault(false);
                    col.NotNull();
                }));

            return 5;
        }
    }
}