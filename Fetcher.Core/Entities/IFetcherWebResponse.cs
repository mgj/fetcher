using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace artm.Fetcher.Core.Entities
{
    public interface IFetcherWebResponse
    {
        int Id { get; set; }
        int UrlCacheInfoId { get; set; }
        int HttpStatusCode { get; set; }
        Exception Error { get; set; }
        string Body { get; set; }
        byte[] BodyAsBytes { get; set; }
        string ContentType { get; set; }
        Dictionary<string, string> Headers { get; set; }
        bool IsSuccess { get; }
    }
}
