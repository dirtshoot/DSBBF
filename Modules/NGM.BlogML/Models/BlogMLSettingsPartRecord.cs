using Orchard.ContentManagement.Records;

namespace NGM.BlogML.Models {
    public class BlogMLSettingsPartRecord : ContentPartRecord {
        public virtual string AttachmentDirectory { get; set; }
        public virtual string ExportDirectory { get; set; }
    }
}