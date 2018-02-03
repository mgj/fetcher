using artm.Fetcher.Core.Policies;
using artm.Fetcher.Core.Tests.Services.Common;
using artm.Fetcher.Core.Tests.Services.Mocks;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace artm.Fetcher.Core.Tests.Policies
{
    [TestFixture]
    public class AlwaysCachePolicyTests
    {
        [Test]
        public async Task ShouldInsertCache_200HttpStatus_IsCached()
        {
            var fetcherService = new FetcherServiceStub(FetcherWebServiceMockFactory.IFetcherWebServiceAlwaysHttpStatus200(), new AlwaysCachePolicy());

            var response = await fetcherService.FetchAsync(new Uri("https://www.google.com"));
            var caches = await fetcherService.RepositoryService.GetAllUrlCacheInfo();

            Assert.NotNull(caches);
            Assert.AreEqual(1, caches.Count());
        }

        [Test]
        public async Task ShouldInsertCache_500HttpStatus_IsCached()
        {
            var fetcherService = new FetcherServiceStub(FetcherWebServiceMockFactory.IFetcherWebServiceAlwaysHttpStatus500(), new AlwaysCachePolicy());

            var response = await fetcherService.FetchAsync(new Uri("https://www.google.com"));
            var caches = await fetcherService.RepositoryService.GetAllUrlCacheInfo();

            Assert.NotNull(caches);
            Assert.AreEqual(1, caches.Count());
        }
    }
}
