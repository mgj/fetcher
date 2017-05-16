using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace artm.Fetcher.Core.Models
{
    public class FetcherWebRequest : IFetcherWebRequest
    {
        public Uri Url { get; set; }
        public string Method { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public string Body { get; set; }
        public string ContentType { get; set; }
    }
}
