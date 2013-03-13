using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WunderWeather.Models
{
    //[Serializable]
    public class WeatherMessage
    {
        private string _weather;
        private string _temperature;
        private string _tempF;
        private string _tempC;
        private string _relativeHumidity;
        private string _wind;
        private string _windDir;
        private string _windDegrees;
        private string _windMph;
        private string _windGustMph;
        private string _pressure;
        private string _pressureMb;
        private string _pressureIn;
        private string _dewpoint;
        private string _dewpointF;
        private string _dewpointC;
        private string _heatIndex;
        private string _heatIndexF;
        private string _heatIndexC;
        private string _windchill;
        private string _windchillF;
        private string _windchillC;
        private string _visibilityMi;
        private string _visibilityKm;
        private string[] _icons;
        private string _forecast_url;
        private string _history_url;
        private string _ob_url;
        private string _logo_url;
        private string _observationLocation;

        public string Weather
        {
            get { return _weather; }
            set { _weather = value; }
        }

        public string Temperature
        {
            get { return _temperature; }
            set { _temperature = value; }
        }

        public string TempF
        {
            get { return _tempF; }
            set { _tempF = value; }
        }

        public string TempC
        {
            get { return _tempC; }
            set { _tempC = value; }
        }

        public string RelativeHumidity
        {
            get { return _relativeHumidity; }
            set { _relativeHumidity = value; }
        }

        public string Wind
        {
            get { return _wind; }
            set { _wind = value; }
        }

        public string WindDir
        {
            get { return _windDir; }
            set { _windDir = value; }
        }

        public string WindDegrees
        {
            get { return _windDegrees; }
            set { _windDegrees = value; }
        }

        public string WindMph
        {
            get { return _windMph; }
            set { _windMph = value; }
        }

        public string WindGustMph
        {
            get { return _windGustMph; }
            set { _windGustMph = value; }
        }

        public string Pressure
        {
            get { return _pressure; }
            set { _pressure = value; }
        }

        public string PressureMb
        {
            get { return _pressureMb; }
            set { _pressureMb = value; }
        }

        public string PressureIn
        {
            get { return _pressureIn; }
            set { _pressureIn = value; }
        }

        public string Dewpoint
        {
            get { return _dewpoint; }
            set { _dewpoint = value; }
        }

        public string DewpointF
        {
            get { return _dewpointF; }
            set { _dewpointF = value; }
        }

        public string DewpointC
        {
            get { return _dewpointC; }
            set { _dewpointC = value; }
        }

        public string HeatIndex
        {
            get { return _heatIndex; }
            set { _heatIndex = value; }
        }

        public string HeatIndexF
        {
            get { return _heatIndexF; }
            set { _heatIndexF = value; }
        }

        public string HeatIndexC
        {
            get { return _heatIndexC; }
            set { _heatIndexC = value; }
        }

        public string Windchill
        {
            get { return _windchill; }
            set { _windchill = value; }
        }

        public string WindchillF
        {
            get { return _windchillF; }
            set { _windchillF = value; }
        }

        public string WindchillC
        {
            get { return _windchillC; }
            set { _windchillC = value; }
        }

        public string VisibilityMi
        {
            get { return _visibilityMi; }
            set { _visibilityMi = value; }
        }

        public string VisibilityKm
        {
            get { return _visibilityKm; }
            set { _visibilityKm = value; }
        }

        public string[] Icons
        {
            get { return _icons; }
            set { _icons = value; }
        }

        public string Forecast_url
        {
            get { return _forecast_url; }
            set { _forecast_url = value; }
        }

        public string History_url
        {
            get { return _history_url; }
            set { _history_url = value; }
        }

        public string Ob_url
        {
            get { return _ob_url; }
            set { _ob_url = value; }
        }

        public string Logo_url
        {
            get { return _logo_url; }
            set { _logo_url = value; }
        }

        public string ObservationLocation
        {
            get { return _observationLocation; }
            set { _observationLocation = value; }
        }
    }
} 