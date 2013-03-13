using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement.Records;

namespace UserVoice.Widgets.Models
{
    public class FeedbackPartRecord : ContentPartRecord
    {
        public virtual string TabLabel { get; set; }
        public virtual string TabColor { get; set; }
        public virtual TabPosition TabPosition { get; set; }
        public virtual bool TabInverted { get; set; }
        public virtual bool TabEnabled { get; set; }
        public virtual string WidgetId { get; set; }
    }
}