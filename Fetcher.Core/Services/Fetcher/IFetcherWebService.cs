using artm.Fetcher.Core.Models;
using System;
using System.Net;

namespace artm.Fetcher.Core.Services
{
    public interface IFetcherWebService
    {
        FetcherWebResponse DoPlatformRequest(FetcherWebRequest request);
    }
}
