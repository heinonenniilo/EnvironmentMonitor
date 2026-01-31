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

        public async Task<Dictionary<string, List<(DateTime Time, double Value)>>> GetSeriesAsync(
            string place,
            DateTime startTimeUtc,
            DateTime endTimeUtc,
            IEnumerable<string> parameters)
        {
            if (string.IsNullOrWhiteSpace(place))
                throw new ArgumentException("Place must be provided.", nameof(place));

            var paramList = string.Join(",", parameters);
            var query = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string,string>("service","WFS"),
                new KeyValuePair<string,string>("version","2.0.0"),
                new KeyValuePair<string,string>("request","getFeature"),
                new KeyValuePair<string,string>("storedquery_id","fmi::observations::weather::timevaluepair"),
                new KeyValuePair<string,string>("place", place),
                new KeyValuePair<string,string>("parameters", paramList),
                new KeyValuePair<string,string>("starttime", startTimeUtc.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture)),
                new KeyValuePair<string,string>("endtime", endTimeUtc.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture))
            };
            var content = new FormUrlEncodedContent(query);
            var requestUri = $"{BaseUrl}?{await content.ReadAsStringAsync()}";

            using var response = await _http.GetAsync(requestUri).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            var xml = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return ParseSeries(xml);
        }

        private static Dictionary<string, List<(DateTime Time, double Value)>> ParseSeries(string xml)
        {
            var doc = XDocument.Parse(xml);
            XNamespace wml2 = "http://www.opengis.net/waterml/2.0";
            XNamespace gml = "http://www.opengis.net/gml/3.2";
            var result = new Dictionary<string, List<(DateTime, double)>>();

            foreach (var ts in doc.Descendants(wml2 + "MeasurementTimeseries"))
            {
                var idAttr = ts.Attribute(gml + "id")?.Value;
                if (string.IsNullOrWhiteSpace(idAttr))
                    continue;
                var lastDash = idAttr.LastIndexOf('-');
                var paramName = lastDash >= 0 ? idAttr.Substring(lastDash + 1) : idAttr;

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
                result[paramName] = list;
            }
            return result;
        }

        public void Dispose() => _http.Dispose();
    }
}
