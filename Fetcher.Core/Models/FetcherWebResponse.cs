using System;

namespace artm.Fetcher.Core.Models
{
    public class FetcherWebResponse
    {
        public bool IsSuccess
        {
            get
            {
                return HttpStatusCode >= 200 && HttpStatusCode < 300;
            }
        }
        public int HttpStatusCode { get; set; }
        public Exception Error { get; set; }
        public string Body { get; set; }
    }
}
