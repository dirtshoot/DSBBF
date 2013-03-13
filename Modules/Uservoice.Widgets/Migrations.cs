using System;
using System.Collections.Generic;
using System.Data;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
using UserVoice.Widgets.Models;

namespace UserVoice.Widgets {
    public class Migrations : DataMigrationImpl {


        public int Create() {
		
            SchemaBuilder.CreateTable(typeof(SiteSettingsPartRecord).Name, table => table
				.ContentPartRecord()
                .Column("Account", DbType.String)
				.Column("Host", DbType.String)
                .Column("ApiKey", DbType.String)
			);

            SchemaBuilder.CreateTable(typeof(FeedbackPartRecord).Name, table => table
                .ContentPartRecord()
                .Column("TabLabel", DbType.String)
                .Column("TabColor", DbType.String)
                .Column("TabPosition", DbType.String)
                .Column("TabInverted", DbType.Boolean)
                .Column("TabEnabled", DbType.Boolean)
                .Column("WidgetId", DbType.String)
                );

            SchemaBuilder.CreateTable(typeof(SuggestionsPartRecord).Name, table => table
                .ContentPartRecord()
                .Column("ForumId", DbType.String)
                );

            ContentDefinitionManager.AlterTypeDefinition("FeedbackWidget", cfg => cfg
                .WithPart("FeedbackPart")
                .WithPart("WidgetPart")
                .WithPart("CommonPart")
                .WithSetting("Stereotype", "Widget")
                .WithSetting("Description", "Display the UserVoice Feedback tab on your site.")
                );

            ContentDefinitionManager.AlterTypeDefinition("SuggestionsWidget", cfg => cfg
                .WithPart("SuggestionsPart")
                .WithPart("WidgetPart")
                .WithPart("CommonPart")
                .WithSetting("Stereotype", "Widget")
                .WithSetting("Description", "Display a list of User Suggestions on your site.")
                );

            return 2;
        }

        public int UpdateFrom1()
        {
            SchemaBuilder.AlterTable(typeof(FeedbackPartRecord).Name, table => table
                .AddColumn("WidgetId", DbType.String));
            return 2;
        }

    }
}