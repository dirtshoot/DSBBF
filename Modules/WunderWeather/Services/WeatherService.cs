using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WunderWeather.Contracts.Services;
using WunderWeather.Models;
using System.Xml;
using System.Net;
using System.Text;
using System.IO;
using Orchard.Caching;
using System.Threading;

namespace WunderWeather.Services
{
    public class WeatherService : IWeatherService
    {
        protected readonly string CacheKeyPrefix = "5C4F8398-E006-47D6-8D3A-DFBD457D8A6B_";

        protected ICacheManager CacheManager { get; private set; }
        protected ISignals Signals { get; private set; }
        protected Timer Timer { get; private set; }

        public WeatherService(ICacheManager cacheManager, ISignals signals)
        {
            this.CacheManager = cacheManager;
            this.Signals = signals;
        }

        #region IWeatherService Members

        public Models.WeatherMessage GetWeatherForLocation(string location)
        {
            // Build cache key
            var cacheKey = CacheKeyPrefix + location;

            return CacheManager.Get(cacheKey, ctx =>
            {
                ctx.Monitor(Signals.When(cacheKey));
                Timer = new Timer(t => Signals.Trigger(cacheKey), location, TimeSpan.FromMinutes(5), TimeSpan.FromMilliseconds(-1));
                return GetWeatherFromRESTService(location);
            });
        }

        #endregion

        public Models.WeatherMessage GetWeatherFromRESTService(string location)
        {
            string addressUrl = String.Format("http://api.wunderground.com/auto/wui/geo/WXCurrentObXML/index.xml?query={0}", location);
            HttpWebRequest loHttp = (HttpWebRequest)WebRequest.Create(addressUrl);
            HttpWebResponse response;

            loHttp.Timeout = 10000;
            loHttp.UserAgent = "Code Sample Web Client";

            response = (HttpWebResponse)loHttp.GetResponse();
            Encoding enc = Encoding.UTF8;
            StreamReader loResponseStream = new StreamReader(response.GetResponseStream(), enc);

            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(loResponseStream);

            return CreaterWeatherFromXDoc(xDoc);
        }

        public static WeatherMessage CreaterWeatherFromXDoc(XmlDocument xDoc)
        {
            var retWeatherMessage = new WeatherMessage();

            XmlNodeList weatherNode = xDoc.GetElementsByTagName("weather");
            XmlNodeList temperatureNode = xDoc.GetElementsByTagName("temperature_string");
            XmlNodeList temp_fNode = xDoc.GetElementsByTagName("temp_f");
            XmlNodeList temp_cNode = xDoc.GetElementsByTagName("temp_c");
            XmlNodeList relative_humidityNode = xDoc.GetElementsByTagName("relative_humidity");
            XmlNodeList wind_stringNode = xDoc.GetElementsByTagName("wind_string");
            XmlNodeList wind_dirNode = xDoc.GetElementsByTagName("wind_dir");
            XmlNodeList wind_degreesNode = xDoc.GetElementsByTagName("wind_degrees");
            XmlNodeList wind_mphNode = xDoc.GetElementsByTagName("wind_mph");
            XmlNodeList wind_gust_mphNode = xDoc.GetElementsByTagName("wind_gust_mph");
            XmlNodeList pressure_stringNode = xDoc.GetElementsByTagName("pressure_string");
            XmlNodeList pressure_mbNode = xDoc.GetElementsByTagName("pressure_mb");
            XmlNodeList pressure_inNode = xDoc.GetElementsByTagName("pressure_in");
            XmlNodeList dewpoint_stringNode = xDoc.GetElementsByTagName("dewpoint_string");
            XmlNodeList dewpoint_fNode = xDoc.GetElementsByTagName("dewpoint_f");
            XmlNodeList dewpoint_cNode = xDoc.GetElementsByTagName("dewpoint_c");
            XmlNodeList heat_index_stringNode = xDoc.GetElementsByTagName("heat_index_string");
            XmlNodeList heat_index_fNode = xDoc.GetElementsByTagName("heat_index_f");
            XmlNodeList heat_index_cNode = xDoc.GetElementsByTagName("heat_index_c");
            XmlNodeList windchill_stringNode = xDoc.GetElementsByTagName("windchill_string");
            XmlNodeList windchill_fNode = xDoc.GetElementsByTagName("windchill_f");
            XmlNodeList windchill_CNode = xDoc.GetElementsByTagName("windchill_c");
            XmlNodeList visibility_miNode = xDoc.GetElementsByTagName("visibility_mi");
            XmlNodeList visibility_kmNode = xDoc.GetElementsByTagName("visibility_km");
            XmlNodeList icon_urlNode = xDoc.GetElementsByTagName("icon_url");

            XmlNodeList forecast_urlNode = xDoc.GetElementsByTagName("forecast_url");
            XmlNodeList history_urlNode = xDoc.GetElementsByTagName("history_url");
            XmlNodeList ob_urlNode = xDoc.GetElementsByTagName("ob_url");
            XmlNodeList urlNode = xDoc.GetElementsByTagName("url");
            XmlNodeList observationNode = xDoc.SelectNodes("//observation_location/full");

            retWeatherMessage.Weather = weatherNode[0].InnerText;
            retWeatherMessage.Temperature = temperatureNode[0].InnerText;
            retWeatherMessage.TempF = temp_fNode[0].InnerText;
            retWeatherMessage.TempC = temp_cNode[0].InnerText;
            retWeatherMessage.RelativeHumidity = relative_humidityNode[0].InnerText;
            retWeatherMessage.Wind = wind_stringNode[0].InnerText;
            retWeatherMessage.WindDir = wind_dirNode[0].InnerText;
            retWeatherMessage.WindDegrees = wind_degreesNode[0].InnerText;
            retWeatherMessage.WindMph = wind_mphNode[0].InnerText;
            retWeatherMessage.WindGustMph = wind_gust_mphNode[0].InnerText;
            retWeatherMessage.Pressure = pressure_stringNode[0].InnerText;
            retWeatherMessage.PressureMb = pressure_mbNode[0].InnerText;
            retWeatherMessage.PressureIn = pressure_inNode[0].InnerText;
            retWeatherMessage.Dewpoint = dewpoint_stringNode[0].InnerText;
            retWeatherMessage.DewpointF = dewpoint_fNode[0].InnerText;
            retWeatherMessage.DewpointC = dewpoint_cNode[0].InnerText;
            retWeatherMessage.HeatIndex = heat_index_stringNode[0].InnerText;
            retWeatherMessage.HeatIndexF = heat_index_fNode[0].InnerText;
            retWeatherMessage.HeatIndexC = heat_index_cNode[0].InnerText;
            retWeatherMessage.Windchill = windchill_stringNode[0].InnerText;
            retWeatherMessage.WindchillF = windchill_fNode[0].InnerText;
            retWeatherMessage.WindchillC = windchill_CNode[0].InnerText;
            retWeatherMessage.VisibilityMi = visibility_miNode[0].InnerText;
            retWeatherMessage.VisibilityKm = visibility_kmNode[0].InnerText;
            retWeatherMessage.Forecast_url = forecast_urlNode[0].InnerText;
            retWeatherMessage.History_url = history_urlNode[0].InnerText;
            retWeatherMessage.Ob_url = ob_urlNode[0].InnerText;
            retWeatherMessage.Logo_url = urlNode[0].InnerText;

            string[] iconArray = new string[icon_urlNode.Count + 1];

            for (int i = 0; i < icon_urlNode.Count; i++)
            {
                iconArray[i] = icon_urlNode[i].InnerText;
            }

            retWeatherMessage.Icons = iconArray;
            retWeatherMessage.ObservationLocation = observationNode[0].InnerText;

            return retWeatherMessage;
        }
    }
}