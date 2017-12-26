using System;
using System.Collections.Generic;

namespace artm.Fetcher.Core.Models
{
    public interface IFetcherWebRequest
    {
        int Id { get; set; }
        int UrlCacheInfoId { get; set; }
        string Body { get; set; }
        string ContentType { get; set; }
        Dictionary<string, string> Headers { get; set; }
        string Method { get; set; }
        string Url { get; set; }
    }
}