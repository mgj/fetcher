using artm.Fetcher.Core.Models;
using System;
using System.Net;

namespace artm.Fetcher.Core.Services
{
    public interface IFetcherWebService
    {
        FetcherWebResponse DoPlatformWebRequest(Uri uri);

        FetcherWebResponse DoPlatformRequest(Uri uri, HttpWebRequest request);
    }
}
