using artm.Fetcher.Core.Entities;
using artm.Fetcher.Core.Models;
using artm.Fetcher.Core.Services;
using artm.Fetcher.Core.Tests.Services.Common;
using artm.Fetcher.Core.Tests.Services.Mocks;
using artm.Fetcher.Core.Tests.Services.Stubs;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace artm.Fetcher.Core.Tests.Services
{
    [TestFixture]
    public class FetcherRepositoryServiceTests
    {
        private readonly Uri URL = new Uri("https://www.google.com");

        [Test]
        public async Task GetEntryForUrl_NoEntryExists_NullIsReturned()
        {
            var sut = new FetcherRepositoryServiceStub();
            var request = FetcherStubFactory.FetcherWebRequestGetFactory(URL);

            var entry = await sut.GetEntryForRequestAsync(request);

            Assert.IsNull(entry);
        }

        [Test]
        public async Task GetEntryForUrl_EntryExists_EntryReturned()
        {
            var sut = new FetcherRepositoryServiceStub();
            var request = FetcherStubFactory.FetcherWebRequestGetFactory(URL);

            await sut.InsertUrlAsync(request, FetcherStubFactory.FetcherWebResponseSuccessFactory());
            var entry = sut.GetEntryForRequestAsync(request);

            Assert.IsNotNull(entry);
        }

        [Test]
        public async Task GetEntryForUrl_EntryExistsNewRequest_EntryReturned()
        {
            var sut = new FetcherRepositoryServiceStub();

            await sut.InsertUrlAsync(new FetcherWebRequest()
            {
                Url = URL.OriginalString
            }, FetcherStubFactory.FetcherWebResponseSuccessFactory());
            var entry = sut.GetEntryForRequestAsync(new FetcherWebRequest()
            {
                Url = URL.OriginalString
            });

            Assert.IsNotNull(entry);
        }

        [Test]
        public async Task UpdateUrl_UrlExists_UrlEntryIsUpdated()
        {
            var response = FetcherStubFactory.FetcherWebResponseSuccessFactory();
            var sut = new FetcherRepositoryServiceStub();
            var request = FetcherStubFactory.FetcherWebRequestGetFactory(URL);

            await sut.InsertUrlAsync(request, response);
            var original = await sut.GetEntryForRequestAsync(request);
            var originalResponse = original.FetcherWebResponse;

            await sut.UpdateUrlAsync(request, original, FetcherStubFactory.FetcherWebResponseSuccessFactory("UpdatedTestResponse"));
            var second = await sut.GetEntryForRequestAsync(request);

            Assert.AreNotEqual(originalResponse.Body, second.FetcherWebResponse.Body);
        }

        [Test]
        public async Task InsertUrl_ValidInput_UrlIsInserted()
        {
            var sut = new FetcherRepositoryServiceStub();
            var request = FetcherStubFactory.FetcherWebRequestGetFactory(URL);

            var isNull = await sut.GetEntryForRequestAsync(request);
            await sut.InsertUrlAsync(request, FetcherStubFactory.FetcherWebResponseSuccessFactory());
            var notNull = await sut.GetEntryForRequestAsync(request);

            Assert.IsNull(isNull);
            Assert.NotNull(notNull);
        }

        [Test]
        public async Task PreloadUrl_ValidInput_UrlIsPreloaded()
        {
            var sut = new FetcherRepositoryServiceStub();
            var request = FetcherStubFactory.FetcherWebRequestGetFactory(URL);

            var isNull = await sut.GetEntryForRequestAsync(request);
            await sut.PreloadUrlAsync(request, FetcherStubFactory.FetcherWebResponseSuccessFactory());
            var notNull = await sut.GetEntryForRequestAsync(request);

            Assert.IsNull(isNull);
            Assert.NotNull(notNull);
        }

        [Test]
        public async Task UpdateUrl_UrlIsUpdated_LastAccessedIsNotUpdated()
        {
            var sut = new FetcherRepositoryServiceStub();
            var request = FetcherStubFactory.FetcherWebRequestGetFactory(URL);

            await sut.PreloadUrlAsync(request, FetcherStubFactory.FetcherWebResponseSuccessFactory());
            var data = await sut.GetEntryForRequestAsync(request);

            var lastAccess1 = data.LastAccessed;
            var lastUpdated1 = data.LastUpdated;

            await sut.UpdateUrlAsync(request, data, FetcherStubFactory.FetcherWebResponseSuccessFactory("updated-response"));
            var lastAccess2 = data.LastAccessed;
            var lastUpdated2 = data.LastUpdated;

            Assert.AreEqual(lastAccess1, lastAccess2);
            Assert.AreNotEqual(lastUpdated1, lastUpdated2);
        }
        
        [Test]
        public async Task DeleteEntry_ContainsElements_ValidResponse()
        {
            IFetcherRepositoryService sut = new FetcherRepositoryServiceStub();
            var request = FetcherStubFactory.FetcherWebRequestGetFactory(URL);

            IUrlCacheInfo hero = await sut.PreloadUrlAsync(request, FetcherStubFactory.FetcherWebResponseSuccessFactory());

            bool response = await sut.DeleteEntry(hero);

            Assert.IsTrue(response);
        }

        [Test]
        public async Task DeleteEntry_ContainsElements_ElementsAreRemoved()
        {
            FetcherRepositoryServiceStub sut = new FetcherRepositoryServiceStub(true);
            var request = FetcherStubFactory.FetcherWebRequestGetFactory(URL);

            IUrlCacheInfo hero = await sut.PreloadUrlAsync(request, FetcherStubFactory.FetcherWebResponseSuccessFactory());
            bool response = await sut.DeleteEntry(hero);

            var requests = await sut.AllWebRequests();
            var responses = await sut.AllWebResponse();
            var caches = await sut.AllCacheInfo();

            Assert.AreEqual(0, requests.Count);
            Assert.AreEqual(0, responses.Count);
            Assert.AreEqual(0, caches.Count);
        }
    }
}
