using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;

namespace UserVoice.Widgets.Models
{
    public class FeedbackPart : ContentPart<FeedbackPartRecord>
    {
        public string TabLabel {
            get { return Record.TabLabel; }
            set { Record.TabLabel = value; }
        }

        public string TabColor
        {
            get { return Record.TabColor; }
            set { Record.TabColor = value; }
        }

        public TabPosition TabPosition
        {
            get { return Record.TabPosition; }
            set { Record.TabPosition = value; }
        }

        public bool TabInverted
        {
            get { return Record.TabInverted; }
            set { Record.TabInverted = value; }
        }

        public bool TabEnabled
        {
            get { return Record.TabEnabled; }
            set { Record.TabEnabled = value; }
        }


        public string WidgetId 
        {
            get { return Record.WidgetId; }
            set { Record.WidgetId = value;  }
        }
    }
}