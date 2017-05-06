using System;

namespace artm.Fetcher.Core.Entities
{
    public interface IUrlCacheInfo
    {
        string Response { get; set; }

        string Url { get; set; }

        DateTimeOffset Created { get; set; }

        DateTimeOffset LastAccessed { get; set; }

        DateTimeOffset LastUpdated { get; set; }
    }
}
