using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using WunderWeather.Models;


namespace WunderWeather.Handlers
{
    public class WeatherHandler : ContentHandler {
        public WeatherHandler(IRepository<WeatherPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}