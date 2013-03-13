namespace Contrib.FileField.Settings {
    public class FileFieldSettings {
        public bool Required { get; set; }
        public OpenAction OpenAction { get; set; }
        public string Hint { get; set; }
        public string MediaFolder { get; set; }
    }

    public enum OpenAction {
        _blank,
        _self,
        _parent,
        _top
    }
}