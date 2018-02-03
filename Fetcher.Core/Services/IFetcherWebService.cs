using artm.Fetcher.Core.Entities;
using artm.Fetcher.Core.Models;

namespace artm.Fetcher.Core.Services
{
    public interface IFetcherWebService
    {
        IFetcherWebResponse DoPlatformRequest(IFetcherWebRequest request);
    }
}
