using artm.Fetcher.Core.Entities;
using artm.Fetcher.Core.Models;
using artm.Fetcher.Core.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace artm.Fetcher.Core.Tests.Services.Common
{
    public static class FetcherMockFactory
    {
        public static Mock<IFetcherRepositoryStoragePathService> IFetcherRepositoryStoragePathServiceMemory()
        {
            var mock = new Mock<IFetcherRepositoryStoragePathService>();
            mock.Setup(x => x.GetPath(It.IsAny<string>())).Returns(() => ":memory:");
            return mock;
        }

        public static Mock<IFetcherWebService> IFetcherWebServiceInternetOn()
        {
            var mock = new Mock<IFetcherWebService>();
            mock.Setup(x => x.DoPlatformRequest(It.IsAny<FetcherWebRequest>())).Returns(() => new FetcherWebResponse() { HttpStatusCode = 200, Body = "DoPlatformWebRequest Default Test Body" });
            return mock;
        }

        public static Mock<IFetcherWebService> IFetcherWebServiceInternetOff()
        {
            var mock = new Mock<IFetcherWebService>();
            mock.Setup(x => x.DoPlatformRequest(It.IsAny<FetcherWebRequest>())).Throws(new Exception("IFetcherWebServiceInternetOff mock web exception"));
            return mock;
        }

        public static Mock<IFetcherRepositoryService> IFetcherRepositoryServiceWithOutdatedEntries()
        {
            var repository = new Mock<IFetcherRepositoryService>();
            var created = DateTimeOffset.UtcNow - TimeSpan.FromDays(100);

            repository.Setup(x => x.GetEntryForRequestAsync(It.IsAny<IFetcherWebRequest>()))
                .ReturnsAsync((IFetcherWebRequest request) =>
                UrlCacheInfoFactory(created, request));
            repository.Setup(x => x.InsertUrlAsync(It.IsAny<IFetcherWebRequest>(), It.IsAny<IFetcherWebResponse>()))
                .ReturnsAsync((IFetcherWebRequest request, IFetcherWebResponse response) => 
                UrlCacheInfoFactory(created, new Uri(request.Url)));

            return repository;
        }

        public static Mock<IFetcherRepositoryService> IFetcherRepositoryServiceWitUpToDateEntries()
        {
            var repository = new Mock<IFetcherRepositoryService>();
            repository.Setup(x => x.GetEntryForRequestAsync(It.IsAny<IFetcherWebRequest>()))
                .ReturnsAsync((IFetcherWebRequest request) =>
                UrlCacheInfoFactory(DateTimeOffset.UtcNow, request));
            repository.Setup(x => x.InsertUrlAsync(It.IsAny<IFetcherWebRequest>(), It.IsAny<IFetcherWebResponse>()))
                .ReturnsAsync((IFetcherWebRequest request, IFetcherWebResponse response) => 
                UrlCacheInfoFactory(DateTimeOffset.UtcNow, new Uri(request.Url)));

            return repository;
        }

        private static UrlCacheInfo UrlCacheInfoFactory(DateTimeOffset created, IFetcherWebRequest request)
        {
            return UrlCacheInfoFactory(created, new Uri(request.Url));
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
