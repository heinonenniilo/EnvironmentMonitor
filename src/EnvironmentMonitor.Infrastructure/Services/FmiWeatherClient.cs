using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using EnvironmentMonitor.Domain.Interfaces;

namespace EnvironmentMonitor.Infrastructure.Services
{
    public class FmiWeatherClient : IFmiWeatherClient
    {
        private const string BaseUrl = "http://opendata.fmi.fi/wfs";

        private readonly HttpClient _http;

        public FmiWeatherClient(HttpClient httpClient)
        {
            _http = httpClient;
        }

        public async Task<Dictionary<string, Dictionary<string, List<(DateTime Time, double Value)>>>> GetSeriesAsync(
            IEnumerable<string> places,
            DateTime startTimeUtc,
            DateTime endTimeUtc,
            IEnumerable<string> parameters)
        {
            var placesList = places.ToList();
            if (!placesList.Any())
                throw new ArgumentException("At least one place must be provided.", nameof(places));

            var paramList = string.Join(",", parameters);
            var query = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string,string>("service","WFS"),
                new KeyValuePair<string,string>("version","2.0.0"),
                new KeyValuePair<string,string>("request","getFeature"),
                new KeyValuePair<string,string>("storedquery_id","fmi::observations::weather::timevaluepair"),
                new KeyValuePair<string,string>("parameters", paramList),
                new KeyValuePair<string,string>("starttime", startTimeUtc.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture)),
                new KeyValuePair<string,string>("endtime", endTimeUtc.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture))
            };
            
            // Add multiple place parameters
            foreach (var place in placesList)
            {
                query.Add(new KeyValuePair<string, string>("place", place));
            }

            var content = new FormUrlEncodedContent(query);
            var requestUri = $"{BaseUrl}?{await content.ReadAsStringAsync()}";

            using var response = await _http.GetAsync(requestUri).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            var xml = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return ParseSeriesByPlace(xml, placesList);
        }

        private static Dictionary<string, Dictionary<string, List<(DateTime Time, double Value)>>> ParseSeriesByPlace(string xml, List<string> places)
        {
            var doc = XDocument.Parse(xml);
            XNamespace wml2 = "http://www.opengis.net/waterml/2.0";
            XNamespace gml = "http://www.opengis.net/gml/3.2";
            XNamespace gmlcov = "http://www.opengis.net/gmlcov/1.0";
            XNamespace swe = "http://www.opengis.net/swe/2.0";
            XNamespace target = "http://xml.fmi.fi/namespace/om/atmosphericfeatures/1.1";
            
            var result = new Dictionary<string, Dictionary<string, List<(DateTime, double)>>>();

            foreach (var ts in doc.Descendants(wml2 + "MeasurementTimeseries"))
            {
                var idAttr = ts.Attribute(gml + "id")?.Value;
                if (string.IsNullOrWhiteSpace(idAttr))
                    continue;

                // Extract parameter name from id (e.g., "obs-obs-1-1-t2m" => "t2m")
                var lastDash = idAttr.LastIndexOf('-');
                var paramName = lastDash >= 0 ? idAttr.Substring(lastDash + 1) : idAttr;

                // Try to extract place name from the measurement timeseries
                // FMI includes location info in the feature members
                var featureMember = ts.Ancestors().FirstOrDefault(a => a.Name.LocalName == "FeatureCollection")
                    ?.Elements().FirstOrDefault(e => e.Descendants(wml2 + "MeasurementTimeseries").Any(mts => mts == ts));
                
                string placeName = null;
                
                // Try to find location name in various possible locations in XML
                var locationElement = featureMember?.Descendants(target + "LocationCollection")
                    .Descendants(target + "Location")
                    .Descendants(gml + "name")
                    .FirstOrDefault();
                
                if (locationElement != null)
                {
                    placeName = locationElement.Value;
                }
                
                // If we couldn't find place name in XML and we only requested one place, use that
                if (string.IsNullOrEmpty(placeName) && places.Count == 1)
                {
                    placeName = places[0];
                }
                
                // Skip if we couldn't determine the place
                if (string.IsNullOrEmpty(placeName))
                    continue;

                // Initialize dictionary for this place if needed
                if (!result.ContainsKey(placeName))
                {
                    result[placeName] = new Dictionary<string, List<(DateTime, double)>>();
                }

                var list = new List<(DateTime, double)>();
                foreach (var tvp in ts.Descendants(wml2 + "MeasurementTVP"))
                {
                    var timeElem = tvp.Element(wml2 + "time");
                    var valueElem = tvp.Element(wml2 + "value");
                    if (timeElem == null || valueElem == null)
                        continue;
                    if (!DateTime.TryParse(
                            timeElem.Value,
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal,
                            out var time))
                        continue;
                    if (!double.TryParse(valueElem.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
                        continue;
                    list.Add((time, value));
                }
                
                result[placeName][paramName] = list;
            }
            
            return result;
        }

        public void Dispose() => _http.Dispose();
    }
}
