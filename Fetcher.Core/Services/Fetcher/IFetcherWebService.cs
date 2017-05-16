using artm.Fetcher.Core.Entities;
using artm.Fetcher.Core.Models;
using System;
using System.Net;

namespace artm.Fetcher.Core.Services
{
    public interface IFetcherWebService
    {
        IFetcherWebResponse DoPlatformRequest(IFetcherWebRequest request);
    }
}
