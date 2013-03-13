namespace Contrib.DateTimeField.Settings {

    public enum DateTimeFieldDisplays {
        DateAndTime,
        DateOnly,
        TimeOnly
    }

    public class DateTimeFieldSettings {
        public DateTimeFieldDisplays Display { get; set; }
    }
}
