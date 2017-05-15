using artm.Fetcher.Core.Entities;
using artm.Fetcher.Core.Models;
using artm.Fetcher.Core.Services;
using artm.Fetcher.Core.Tests.Services.Common;
using artm.Fetcher.Core.Tests.Services.Mocks;
using artm.Fetcher.Core.Tests.Services.Stubs;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace artm.Fetcher.Core.Tests.Services
{
    [TestFixture]
    public class FetcherServiceTests
    {
        private const string URL = "https://jsonplaceholder.typicode.com/users";
        
        [Test]
        public void Fetch_NullUrl_Throws()
        {
            var sut = new FetcherServiceStub();

            Assert.ThrowsAsync<NullReferenceException>(async () => await sut.Fetch(null));
        }

        [Test]
        public async Task Fetch_Sunshine_TriesToFetchFromWeb()
        {
            var sut = new FetcherServiceStub();
            
            var response = await sut.Fetch(new Uri(URL));

            sut.WebServiceMock.Verify(x => x.DoPlatformRequest(It.IsAny<FetcherWebRequest>()), Times.Once);
        }

        [Test]
        public async Task Fetch_NoEntries_RepositoryInsertUrlIsCalled()
        {
            var repository = new Mock<IFetcherRepositoryService>();
            var sut = new FetcherServiceStub(repository.Object);

            var response = await sut.Fetch(new Uri(URL));

            repository.Verify(x => x.InsertUrl(It.IsAny<Uri>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task Fetch_MultipleCallsToSameUrl_InsertUrlCalledNeverCalledWhenEntryExists()
        {
            var repository = new Mock<IFetcherRepositoryService>();
            repository.Setup(x => x.GetEntryForUrl(It.IsAny<Uri>())).Returns(() => new UrlCacheInfoMock() { Url = URL, Created = DateTimeOffset.UtcNow, LastAccessed = DateTimeOffset.UtcNow, LastUpdated = DateTimeOffset.UtcNow, Response = "myResponse" });
            var sut = new FetcherServiceStub(repository.Object);

            var response = await sut.Fetch(new Uri(URL));
            await sut.Fetch(new Uri(URL));
            await sut.Fetch(new Uri(URL));

            repository.Verify(x => x.InsertUrl(It.IsAny<Uri>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task Fetch_MultipleCallsToSameUrl_LastAccessedIsUpdated()
        {
            var sut = new FetcherServiceStub();

            var response1 = await sut.Fetch(new Uri(URL));
            var access1 = response1.LastAccessed.ToString();
            await Task.Delay(1000);
            var response2 = await sut.Fetch(new Uri(URL));
            var access2 = response2.LastAccessed.ToString();

            var delta = response2.LastAccessed - response1.LastAccessed;
            Assert.IsTrue( delta.TotalMilliseconds > 0);
        }

        [Test]
        public async Task Fetch_WithOutdatedEntries_RepositoryUpdateUrlIsCalled()
        {
            var repository = new Mock<IFetcherRepositoryService>();
            var created = DateTimeOffset.UtcNow - TimeSpan.FromDays(100);
            repository.Setup(x => x.GetEntryForUrl(It.IsAny<Uri>())).Returns(() => new UrlCacheInfoMock() { Url = URL, Created = created, LastAccessed = DateTimeOffset.UtcNow, LastUpdated = created, Response = "myResponse" });

            var sut = new FetcherServiceStub(repository.Object);

            var response = await sut.Fetch(new Uri(URL));

            repository.Verify(x => x.UpdateUrl(It.IsAny<Uri>(), It.IsAny<IUrlCacheInfo>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task Fetch_WithUpToDateEntries_RepositoryUpdateUrlIsNotCalled()
        {
            var repository = new Mock<IFetcherRepositoryService>();
            var created = DateTimeOffset.UtcNow - TimeSpan.FromMilliseconds(1);
            repository.Setup(x => x.GetEntryForUrl(It.IsAny<Uri>())).Returns(() => new UrlCacheInfoMock() { Url = URL, Created = created, LastAccessed = DateTimeOffset.UtcNow, LastUpdated = created, Response = "myResponse" });

            var sut = new FetcherServiceStub(repository.Object);

            var response = await sut.Fetch(new Uri(URL));

            repository.Verify(x => x.UpdateUrl(It.IsAny<Uri>(), It.IsAny<IUrlCacheInfo>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task Preload_InternetUnavailableAndEmptyDatabase_PreloadedDataIsReturned()
        {
            var sut = new FetcherServiceStub(FetcherMockFactory.IFetcherWebServiceInternetOff());
            const string response = "myPreloadResponse";

            sut.Preload(new Uri(URL), response);
            var hero = await sut.Fetch(new Uri(URL));

            Assert.IsNotNull(hero);
            Assert.IsTrue(hero.Response.Equals(response));
        }

        [Test]
        public async Task Preload_InternetUnavailable_PreloadedDataIsConsideredInvalidated()
        {
            var sut = new FetcherServiceStub(FetcherMockFactory.IFetcherWebServiceInternetOff());
            const string response = "myPreloadResponse";

            sut.Preload(new Uri(URL), response);
            var hero = await sut.Fetch(new Uri(URL));

            var isInvalid = FetcherService.ShouldInvalidate(hero, sut.CACHE_FRESHNESS_THRESHOLD);

            Assert.IsTrue(isInvalid);
        }

        [Test]
        public async Task Fetch_EmptyDatabaseNoPreloadInternetUnavailable_NullIsReturned()
        {
            var sut = new FetcherServiceStub(FetcherMockFactory.IFetcherWebServiceInternetOff());

            var hero = await sut.Fetch(new Uri(URL));

            Assert.IsNull(hero);
        }
        
        [Test]
        public async Task Fetch_PreloadedDataInternetUnavailable_FetchedFromIsPreloaded()
        {
            var sut = new FetcherServiceStub(FetcherMockFactory.IFetcherWebServiceInternetOff());
            const string RESPONSE_STRING = "Fetch_PreloadedDataInternetUnavailable_FetchedFromIsPreloaded";

            sut.Preload(new Uri(URL), RESPONSE_STRING);
            var hero = await sut.Fetch(new Uri(URL));

            Assert.NotNull(hero);
            Assert.AreEqual(RESPONSE_STRING, hero.Response);
            Assert.AreEqual(hero.FetchedFrom, CacheSourceType.Preload);
        }

        [Test]
        public async Task Fetch_PreloadedDataWithInternet_FetchedFromWeb()
        {
            var sut = new FetcherServiceStub();

            const string RESPONSE_STRING = "Fetch_PreloadedDataWithInternet_FetchedFromWeb";

            sut.Preload(new Uri(URL), RESPONSE_STRING);
            var hero = await sut.Fetch(new Uri(URL));

            Assert.NotNull(hero);
            Assert.AreNotEqual(RESPONSE_STRING, hero.Response);
            Assert.AreEqual(hero.FetchedFrom, CacheSourceType.Web);
        }

        [Test]
        public async Task Fetch_RecentlyUpdated_FetchedFromLocalCache()
        {
            var sut = new FetcherServiceStub();

            const string RESPONSE_STRING = "Fetch_RecentlyUpdated_FetchedFromLocalCache";
            
            var hero = await sut.Fetch(new Uri(URL));
            hero = await sut.Fetch(new Uri(URL));
            
            Assert.NotNull(hero);
            Assert.AreNotEqual(RESPONSE_STRING, hero.Response);
            Assert.AreEqual(hero.FetchedFrom, CacheSourceType.Local);
        }

        [Test]
        public async Task Fetch_RecentlyUpdatedManyTimes_FetchedFromLocalCache()
        {
            var sut = new FetcherServiceStub();

            const string RESPONSE_STRING = "Fetch_RecentlyUpdated_FetchedFromLocalCache";

            var hero = await sut.Fetch(new Uri(URL));
            hero = await sut.Fetch(new Uri(URL));
            hero = await sut.Fetch(new Uri(URL));
            hero = await sut.Fetch(new Uri(URL));
            hero = await sut.Fetch(new Uri(URL));
            hero = await sut.Fetch(new Uri(URL));

            Assert.NotNull(hero);
            Assert.AreNotEqual(RESPONSE_STRING, hero.Response);
            Assert.AreEqual(hero.FetchedFrom, CacheSourceType.Local);
        }

        [Test]
        public async Task Fetch_MultithreadedToSameUrl_OnlyOneWebRequestIsMade()
        {
            const int THREAD_COUNT = 10;

            var sut = new FetcherServiceStub();
            var tasks = GenerateFetchTasks(sut, THREAD_COUNT, new Uri("https://www.google.com"));

            var results = await Task.WhenAll(tasks.ToArray());

            sut.WebServiceMock.Verify(x => x.DoPlatformRequest(It.IsAny<FetcherWebRequest>()), Times.Once);
        }

        [Test]
        public async Task Fetch_MultithreadedWhenAll_IsHandled()
        {
            const int THREAD_COUNT = 10;

            var sut = new FetcherServiceStub();
            var tasks = GenerateFetchTasks(sut, THREAD_COUNT);
            var taskResults = new List<IUrlCacheInfo>();

            var results = await Task.WhenAll(tasks.ToArray());

            Assert.AreEqual(THREAD_COUNT, results.Length);
            sut.WebServiceMock.Verify(x => x.DoPlatformRequest(It.IsAny<FetcherWebRequest>()), Times.Exactly(THREAD_COUNT));
            Assert.IsTrue(results.All(x => x != null));
        }

        [Test]
        public async Task Fetch_MultithreadedToSameUrl_OnlyOneEntryInDatabase()
        {
            const int THREAD_COUNT = 10;

            var sut = new FetcherServiceStub(new FetcherRepositoryServiceStub());
            var tasks = GenerateFetchTasks(sut, THREAD_COUNT, new Uri("https://www.google.com"));
            var taskResults = new List<IUrlCacheInfo>();

            var results = await Task.WhenAll(tasks.ToArray());
            var data = ((FetcherRepositoryServiceStub)sut.RepositoryService).DatabaseConnection.Table<UrlCacheInfo>();

            Assert.AreEqual(1, data.Count());
        }

        [Test]
        public async Task Fetch_MultithreadedToDifferentUrls_EntriesCreatedInDatabase()
        {
            const int THREAD_COUNT = 10;

            var sut = new FetcherServiceStub(new FetcherRepositoryServiceStub());
            var tasks = GenerateFetchTasks(sut, THREAD_COUNT);
            var taskResults = new List<IUrlCacheInfo>();

            var results = await Task.WhenAll(tasks.ToArray());
            var data = ((FetcherRepositoryServiceStub)sut.RepositoryService).DatabaseConnection.Table<UrlCacheInfo>();

            Assert.AreEqual(THREAD_COUNT, data.Count());
        }

        private List<Task<IUrlCacheInfo>> GenerateFetchTasks(FetcherService fetcher, int amount, Uri targeturl = null)
        {
            var result = new List<Task<IUrlCacheInfo>>();
            for (int i = 0; i < amount; i++)
            {
                if (targeturl == null)
                {
                    AddTask(fetcher, result, new Uri("http://" + i));
                }
                else
                {
                    AddTask(fetcher, result, targeturl);
                }
            }
            return result;
        }

        private static void AddTask(FetcherService fetcher, List<Task<IUrlCacheInfo>> result, Uri url)
        {
            result.Add(Task.FromResult(fetcher.Fetch(url)).Result);
        }
    }
}
