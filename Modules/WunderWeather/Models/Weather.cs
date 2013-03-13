using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;

namespace WunderWeather.Models
{
    public class WeatherPartRecord : ContentPartRecord
    {
        public virtual string Location { get; set; } 
    }

    public class WeatherPart : ContentPart<WeatherPartRecord>
    {
        [Required]
        public string Location
        {
            get { return Record.Location; }
            set { Record.Location = value; }
        }
    }
}