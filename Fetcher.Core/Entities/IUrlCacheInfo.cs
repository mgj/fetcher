using artm.Fetcher.Core.Models;
using System;

namespace artm.Fetcher.Core.Entities
{
    public interface IUrlCacheInfo
    {
        int Id { get; set; }

        int FetcherWebResponseId { get; set; }
        FetcherWebResponse FetcherWebResponse { get; set; }

        string Url { get; set; }

        DateTimeOffset Created { get; set; }

        DateTimeOffset LastAccessed { get; set; }

        DateTimeOffset LastUpdated { get; set; }

        CacheSourceType FetchedFrom { get; set; }
    }
}
