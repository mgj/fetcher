using artm.Fetcher.Core.Models;
using System;

namespace artm.Fetcher.Core.Services
{
    public interface IFetcherWebService
    {
        FetcherWebResponse DoPlatformWebRequest(Uri uri);
    }
}
