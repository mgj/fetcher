using artm.Fetcher.Core.Entities;
using Newtonsoft.Json;
using SQLite;
using SQLiteNetExtensions.Attributes;
using System.Collections.Generic;

namespace artm.Fetcher.Core.Models
{
    public class FetcherWebRequest : IFetcherWebRequest
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed]
        public string Url { get; set; }

        [Indexed]
        public string Method { get; set; }

        public string HeadersSerialized { get; set; } = string.Empty;

        [Ignore]
        public Dictionary<string, string> Headers
        {
            get
            {
                if (string.IsNullOrEmpty(HeadersSerialized) == true)
                {
                    return new Dictionary<string, string>();
                }

                Dictionary<string, string> result = null;
                try
                {
                    result = JsonConvert.DeserializeObject<Dictionary<string, string>>(HeadersSerialized);
                }
                catch (JsonException je)
                {
                    LogJsonException(je);
                }
                return result;
            }
            set
            {
                if (value != null)
                {
                    try
                    {
                        HeadersSerialized = JsonConvert.SerializeObject(value);
                    }
                    catch (JsonException je)
                    {
                        LogJsonException(je);
                    }
                }
            }
        }

        [ForeignKey(typeof(UrlCacheInfo))]
        public int UrlCacheInfoId { get; set; }

        private void LogJsonException(JsonException je)
        {
            System.Diagnostics.Debug.WriteLine("JSON parsing error: " + je);
        }

        public string Body { get; set; }

        public string ContentType { get; set; }
    }
}
