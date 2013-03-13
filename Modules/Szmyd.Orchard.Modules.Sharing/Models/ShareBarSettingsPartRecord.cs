using Orchard.ContentManagement.Records;

namespace Szmyd.Orchard.Modules.Sharing.Models {
    public class ShareBarSettingsPartRecord : ContentPartRecord {
        public virtual string AddThisAccount { get; set; }
    }
}