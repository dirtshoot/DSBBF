using System;
using System.Collections.Generic;
using System.Data;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
using WunderWeather.Models;

namespace WunderWeather {
    public class Migrations : DataMigrationImpl {

        public int Create() {

            SchemaBuilder.CreateTable("WeatherPartRecord", table => table
                .ContentPartRecord()
                .Column("Location", DbType.String)
            );

            ContentDefinitionManager.AlterPartDefinition(
                typeof(WeatherPart).Name, cfg => cfg.Attachable());
            return 1;
        }

        public int UpdateFrom1()
        {
            // Create a new widget content type with our map
            ContentDefinitionManager.AlterTypeDefinition("WeatherWidget", cfg => cfg
                .WithPart("WeatherPart")
                .WithPart("WidgetPart")
                .WithPart("CommonPart")
                .WithSetting("Stereotype", "Widget"));

            return 2;
        }
    }
}