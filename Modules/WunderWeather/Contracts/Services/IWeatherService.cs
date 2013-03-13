using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WunderWeather.Models;
using Orchard;

namespace WunderWeather.Contracts.Services
{
    public interface IWeatherService : IDependency
    {
        WeatherMessage GetWeatherForLocation(string location);
    }
}


