using artm.Fetcher.Core.Entities;
using artm.Fetcher.Core.Models;
using artm.Fetcher.Core.Services;
using Moq;
using System;
using System.Collections.Generic;

namespace artm.Fetcher.Core.Tests.Services.Common
{
    public static class FetcherRepositoryServiceMockFactory
    {
        public static Mock<IFetcherRepositoryService> IFetcherRepositoryServiceWithOutdatedEntries()
        {
            var repository = new Mock<IFetcherRepositoryService>();
            var created = DateTimeOffset.UtcNow - TimeSpan.FromDays(100);

            repository.Setup(x => x.GetUrlCacheInfoForRequest(It.IsAny<IFetcherWebRequest>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync((IFetcherWebRequest request, bool url, bool method, bool headers, bool contenttype, bool body) =>
                UrlCacheInfoFactory(created, request));
            repository.Setup(x => x.InsertUrlAsync(It.IsAny<IFetcherWebRequest>(), It.IsAny<IFetcherWebResponse>()))
                .ReturnsAsync((IFetcherWebRequest request, IFetcherWebResponse response) =>
                UrlCacheInfoFactory(created, new Uri(request.Url)));

            return repository;
        }

        public static Mock<IFetcherRepositoryService> IFetcherRepositoryServiceWitUpToDateEntries()
        {
            var repository = new Mock<IFetcherRepositoryService>();
            repository.Setup(x => x.GetUrlCacheInfoForRequest(It.IsAny<IFetcherWebRequest>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync((IFetcherWebRequest request, bool url, bool method, bool headers, bool contenttype, bool body) =>
                UrlCacheInfoFactory(DateTimeOffset.UtcNow, request));
            repository.Setup(x => x.InsertUrlAsync(It.IsAny<IFetcherWebRequest>(), It.IsAny<IFetcherWebResponse>()))
                .ReturnsAsync((IFetcherWebRequest request, IFetcherWebResponse response) =>
                UrlCacheInfoFactory(DateTimeOffset.UtcNow, new Uri(request.Url)));

            return repository;
        }

        private static List<UrlCacheInfo> UrlCacheInfoFactory(DateTimeOffset created, IFetcherWebRequest request)
        {
            return new List<UrlCacheInfo> { UrlCacheInfoFactory(created, new Uri(request.Url)) };
        }

        private static UrlCacheInfo UrlCacheInfoFactory(DateTimeOffset created, Uri url)
        {
            return new UrlCacheInfo()
            {
                Id = 9,
                FetcherWebRequestId = 3,
                FetcherWebRequest = FetcherStubFactory.FetcherWebRequestGetFactory(url, id: 3),
                FetcherWebResponseId = 5,
                FetcherWebResponse = FetcherStubFactory.FetcherWebResponseSuccessFactory(id: 5),
                Created = created,
                LastAccessed = DateTimeOffset.UtcNow,
                LastUpdated = created,
            };
        }
    }
}
