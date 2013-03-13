namespace Contrib.ChoiceListField.Settings
{

    public class ChoiceListFieldSettings
    {
        public ChoiceListFieldSettings() {
            // Set defaults
            Options = string.Empty;
            ListMode = "dropdown";
        }
        public string Options { get; set; }
        public string ListMode { get; set; }
    }
}
