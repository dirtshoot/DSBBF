using Orchard.ContentManagement;

namespace Szmyd.Orchard.Modules.Sharing.Models
{
    public class ShareBarSettingsPart : ContentPart<ShareBarSettingsPartRecord>
    {
        public string AddThisAccount
        {
            get { return Record.AddThisAccount; }
            set { Record.AddThisAccount = value; }
        }
    }
}