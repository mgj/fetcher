using artm.Fetcher.Core.Entities;
using Newtonsoft.Json;
using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace artm.Fetcher.Core.Models
{
    public class FetcherWebResponse : IFetcherWebResponse
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int HttpStatusCode { get; set; }

        public string ErrorSerialized { get; set; }

        [Ignore]
        public Exception Error
        {
            get
            {
                if (string.IsNullOrEmpty(ErrorSerialized) == true)
                {
                    return new Exception();
                }
                return JsonConvert.DeserializeObject<Exception>(ErrorSerialized);
            }
            set
            {
                if (value != null)
                {
                    ErrorSerialized = JsonConvert.SerializeObject(value);
                }
            }
        }

        public string HeadersSerialized { get; set; }

        public string Body { get; set; }

        public byte[] BodyAsBytes
        {
            get;
            set;
        }

        public string ContentType { get; set; }

        [ForeignKey(typeof(UrlCacheInfo))]
        public int UrlCacheInfoId { get; set; }

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

        private void LogJsonException(JsonException je)
        {
            System.Diagnostics.Debug.WriteLine("JSON parsing error: " + je);
        }

        [Ignore]
        public bool IsSuccess
        {
            get
            {
                return HttpStatusCode >= 200 && HttpStatusCode < 300;
            }
        }

    }
}
