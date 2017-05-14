using System;

namespace artm.Fetcher.Core.Models
{
    public class FetcherWebResponse
    {
        public bool IsSuccess { get; set; }
        public Exception Error { get; set; }
        public string Body { get; set; }
    }
}
