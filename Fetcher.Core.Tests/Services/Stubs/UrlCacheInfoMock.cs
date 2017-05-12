using artm.Fetcher.Core.Entities;
using System;

namespace artm.Fetcher.Core.Tests.Services.Mocks
{
    internal class UrlCacheInfoMock : IUrlCacheInfo
    {
        public string Response { get; set; }

        public string Url { get; set; }

        public DateTimeOffset Created { get; set; }

        public DateTimeOffset LastAccessed { get; set; }

        public DateTimeOffset LastUpdated { get; set; }
        public CacheSourceType FetchedFrom { get; set; }
    }
}
