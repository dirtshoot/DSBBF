using Orchard.UI.Resources;

namespace UserVoice.Widgets {
    public class ResourceManifest : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();
            manifest.DefineStyle("UserVoiceAdmin").SetUrl("uservoice-admin.css");
            manifest.DefineStyle("UserVoiceWidget").SetUrl("uservoice-widget.css");
        }
    }
}
