using Szmyd.Orchard.Modules.Sharing.Settings;

namespace Szmyd.Orchard.Modules.Sharing.ViewModels
{
    public class ShareBarViewModel
    {
        public string Title { get; set; }
        public string Link { get; set; }
        public string Account { get; set; }
        public ShareBarMode Mode
        {
            get;
            set;
        }
    }
}