using artm.Fetcher.Core.Entities;
using Newtonsoft.Json;
using SQLite;
using SQLite.Net.Attributes;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;

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
        public string Body { get; set; }

        public string HeadersSerialized { get; set; }

        [Ignore]
        public Dictionary<string, string> Headers
        {
            get
            {
                if (string.IsNullOrEmpty(HeadersSerialized) == true)
                {
                    return new Dictionary<string, string>();
                }
                return JsonConvert.DeserializeObject<Dictionary<string, string>>(HeadersSerialized);
            }
            set
            {
                if (value != null)
                {
                    HeadersSerialized = JsonConvert.SerializeObject(value);
                }
            }
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
