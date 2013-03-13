using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using WunderWeather.Models;
using WunderWeather.Services;
using WunderWeather.Contracts.Services;

namespace WunderWeather.Drivers
{
    public class WeatherDriver : ContentPartDriver<WeatherPart>
    {
        protected IWeatherService WeatherRetrievalService { get; private set; }

        public WeatherDriver(IWeatherService weatherRetrievalService)
        {
            this.WeatherRetrievalService = weatherRetrievalService;
        }

        protected override DriverResult Display(WeatherPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_Weather", 
                () => shapeHelper.Parts_Weather(Location: part.Location,
                                                WeatherMessage: WeatherRetrievalService.GetWeatherForLocation(part.Location)));
        }

        protected override DriverResult Editor(WeatherPart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_Weather_Edit",
                () => shapeHelper.EditorTemplate(
                    TemplateName: "Parts/Weather",
                    Model: part,
                    Prefix: Prefix));
        }

        protected override DriverResult Editor(WeatherPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            updater.TryUpdateModel(part, Prefix, null, null);
            return Editor(part, shapeHelper);
        }
    }
}