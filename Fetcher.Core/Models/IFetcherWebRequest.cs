using System;
using System.Collections.Generic;

namespace artm.Fetcher.Core.Models
{
    public interface IFetcherWebRequest
    {
        string Body { get; set; }
        string ContentType { get; set; }
        Dictionary<string, string> Headers { get; set; }
        string Method { get; set; }
        Uri Url { get; set; }
    }
}