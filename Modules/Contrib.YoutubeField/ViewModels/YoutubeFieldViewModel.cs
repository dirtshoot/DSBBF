using System;
using System.Web;

namespace Contrib.YoutubeField.ViewModels {
    public class YoutubeFieldViewModel {
        public const int DefaultWidth = 640;
        public const int DefaultHeight = 390;

        public YoutubeFieldViewModel(Fields.YoutubeField youtubeField) {
            Name = youtubeField.Name;
            Url = string.Format("http://www.youtube.com/watch?v={0}", youtubeField.Identifier);
            Width = youtubeField.Width == 0 ? DefaultWidth : youtubeField.Width;
            Height = youtubeField.Height == 0 ? DefaultHeight : youtubeField.Height;
        }

        public string Name { get; set; }
        public string Url { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public string GetIdentifier() {
            Uri uri = new Uri(Url);
            return HttpUtility.ParseQueryString(uri.Query).Get("v");
        }

        public void UpdateField(Fields.YoutubeField youtubeField) {
            youtubeField.Identifier = GetIdentifier();
            youtubeField.Width = Width;
            youtubeField.Height = Height;
        }
    }
}