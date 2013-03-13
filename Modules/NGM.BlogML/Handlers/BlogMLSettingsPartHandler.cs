using JetBrains.Annotations;
using NGM.BlogML.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Localization;

namespace NGM.BlogML.Handlers {
    [UsedImplicitly]
    public class BlogMLSettingsPartHandler : ContentHandler {
        public BlogMLSettingsPartHandler(IRepository<BlogMLSettingsPartRecord> repository) {
            T = NullLocalizer.Instance;
            Filters.Add(new ActivatingFilter<BlogMLSettingsPart>("Site"));
            Filters.Add(StorageFilter.For(repository));
            OnActivated<BlogMLSettingsPart>(DefaultSettings);
        }

        public Localizer T { get; set; }

        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            if (context.ContentItem.ContentType != "Site")
                return;
            base.GetItemMetadata(context);
            context.Metadata.EditorGroupInfo.Add(new GroupInfo(T("BlogML")));
        }

        private static void DefaultSettings(ActivatedContentContext context, BlogMLSettingsPart settings) {
            settings.Record.AttachmentDirectory = "Attachments";
            settings.Record.ExportDirectory = "Blog Exports";
        }
    }
}