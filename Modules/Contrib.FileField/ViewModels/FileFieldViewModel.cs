using Contrib.FileField.Settings;

namespace Contrib.FileField.ViewModels {
    public class FileFieldViewModel {
        public FileFieldSettings Settings { get; set; }
        public Fields.FileField Field { get; set; }
    }
}