using Newtonsoft.Json;
using SQLite.Net.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace artm.Fetcher.Core.Models
{
    public class FetcherWebRequest : IFetcherWebRequest
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed]
        public string Url { get; set; }

        //[Ignore]
        //public Uri Url
        //{
        //    get
        //    {
        //        if (string.IsNullOrEmpty(UrlOriginalString) == true) return null;
        //        return new Uri(UrlOriginalString);
        //    }
        //    set
        //    {
        //        UrlOriginalString = value?.OriginalString;
        //    }
        //}

        [Indexed]
        public string Method { get; set; }
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

                Dictionary<string, string> result = null;
                try
                {
                    result = JsonConvert.DeserializeObject<Dictionary<string, string>>(HeadersSerialized);
                }
                catch (Exception)
                {
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
                    catch (Exception)
                    {
                    }
                }
            }
        }
        public string Body { get; set; }
        public string ContentType { get; set; }
    }
}
