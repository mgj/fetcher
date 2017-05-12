using artm.Fetcher.Core.Entities;
using artm.Fetcher.Core.Models;
using artm.Fetcher.Core.Services;
using artm.Fetcher.Core.Tests.Services.Mocks;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace artm.Fetcher.Core.Tests.Services
{
    [TestFixture]
    public class FetcherServiceTests
    {
        private const string URL = "https://jsonplaceholder.typicode.com/users";
        
        [Test]
        public async Task Fetch_Sunshine_TriesToFetchFromWeb()
        {
            var web = new Mock<IFetcherWebService>();
            web.Setup(x => x.DoPlatformWebRequest(It.IsAny<Uri>())).Returns(() => new FetcherWebResponse() { IsSuccess = true, Body = "myBody" });
            var sut = new FetcherServiceMock(web);
            
            var response = await sut.Fetch(new Uri(URL));

            sut.WebServiceMock.Verify(x => x.DoPlatformWebRequest(It.IsAny<Uri>()), Times.Once);
        }

        [Test]
        public async Task Fetch_NoEntries_RepositoryInsertUrlIsCalled()
        {
            var repository = new Mock<IFetcherRepositoryService>();
            var sut = new FetcherServiceMock(repository.Object);

            var response = await sut.Fetch(new Uri(URL));

            repository.Verify(x => x.InsertUrl(It.IsAny<Uri>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task Fetch_MultipleCallsToSameUrl_InsertUrlCalledNeverCalledWhenEntryExists()
        {
            var repository = new Mock<IFetcherRepositoryService>();
            repository.Setup(x => x.GetEntryForUrl(It.IsAny<Uri>())).Returns(() => new UrlCacheInfoMock() { Url = URL, Created = DateTimeOffset.UtcNow, LastAccessed = DateTimeOffset.UtcNow, LastUpdated = DateTimeOffset.UtcNow, Response = "myResponse" });
            var sut = new FetcherServiceMock(repository.Object);

            var response = await sut.Fetch(new Uri(URL));
            await sut.Fetch(new Uri(URL));
            await sut.Fetch(new Uri(URL));

            repository.Verify(x => x.InsertUrl(It.IsAny<Uri>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task Fetch_MultipleCallsToSameUrl_LastAccessedIsUpdated()
        {
            var sut = new FetcherServiceMock();

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

            var sut = new FetcherServiceMock(repository.Object);

            var response = await sut.Fetch(new Uri(URL));

            repository.Verify(x => x.UpdateUrl(It.IsAny<Uri>(), It.IsAny<IUrlCacheInfo>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task Fetch_WithUpToDateEntries_RepositoryUpdateUrlIsNotCalled()
        {
            var repository = new Mock<IFetcherRepositoryService>();
            var created = DateTimeOffset.UtcNow - TimeSpan.FromMilliseconds(1);
            repository.Setup(x => x.GetEntryForUrl(It.IsAny<Uri>())).Returns(() => new UrlCacheInfoMock() { Url = URL, Created = created, LastAccessed = DateTimeOffset.UtcNow, LastUpdated = created, Response = "myResponse" });

            var sut = new FetcherServiceMock(repository.Object);

            var response = await sut.Fetch(new Uri(URL));

            repository.Verify(x => x.UpdateUrl(It.IsAny<Uri>(), It.IsAny<IUrlCacheInfo>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task Preload_InternetUnavailableAndEmptyDatabase_PreloadedDataIsReturned()
        {
            var web = FetcherWebServiceInternetUnavailableMockFactory();
            var sut = new FetcherServiceMock(web);
            const string response = "myPreloadResponse";

            sut.Preload(new Uri(URL), response);
            var hero = await sut.Fetch(new Uri(URL));

            Assert.IsNotNull(hero);
            Assert.IsTrue(hero.Response.Equals(response));
        }

        [Test]
        public async Task Preload_InternetUnavailable_PreloadedDataIsConsideredInvalidated()
        {
            var web = FetcherWebServiceInternetUnavailableMockFactory();
            var sut = new FetcherServiceMock(web);
            const string response = "myPreloadResponse";

            sut.Preload(new Uri(URL), response);
            var hero = await sut.Fetch(new Uri(URL));

            var isInvalid = FetcherService.ShouldInvalidate(hero, sut.CACHE_FRESHNESS_THRESHOLD);

            Assert.IsTrue(isInvalid);
        }

        [Test]
        public async Task Fetch_EmptyDatabaseNoPreloadInternetUnavailable_NullIsReturned()
        {
            var web = FetcherWebServiceInternetUnavailableMockFactory();
            var sut = new FetcherServiceMock(web);

            var hero = await sut.Fetch(new Uri(URL));

            Assert.IsNull(hero);
        }
        
        [Test]
        public async Task Fetch_PreloadedDataInternetUnavailable_FetchedFromIsPreloaded()
        {
            var web = FetcherWebServiceInternetUnavailableMockFactory();
            var sut = new FetcherServiceMock(web);

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
            var sut = new FetcherServiceMock();

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
            var sut = new FetcherServiceMock();

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
            var sut = new FetcherServiceMock();

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
        public void Fetch_MultithreadedCalls_IsHandled()
        {
            const int THREAD_COUNT = 10;

            var sut = new FetcherServiceMock();
            var tasks = GenerateFetchTasks(sut, THREAD_COUNT);
            var taskResults = new List<IUrlCacheInfo>();

            var result = Parallel.ForEach(tasks, item => taskResults.Add(item.Result));

            Assert.AreEqual(THREAD_COUNT, taskResults.Count);
            sut.WebServiceMock.Verify(x => x.DoPlatformWebRequest(It.IsAny<Uri>()), Times.Exactly(THREAD_COUNT));
        }

        [Test]
        public void Fetch_MultithreadedToSameUrl_OnlyOneWebRequestIsMade()
        {
            const int THREAD_COUNT = 10;

            var sut = new FetcherServiceMock();
            var tasks = GenerateFetchTasks(sut, THREAD_COUNT, new Uri("https://www.google.com"));
            var taskResults = new List<IUrlCacheInfo>();

            var result = Parallel.ForEach(tasks, item => taskResults.Add(item.Result));

            sut.WebServiceMock.Verify(x => x.DoPlatformWebRequest(It.IsAny<Uri>()), Times.Once);
        }

        [Test]
        public async Task Fetch_MultithreadedWhenAll_IsHandled()
        {
            const int THREAD_COUNT = 10;

            var sut = new FetcherServiceMock();
            var tasks = GenerateFetchTasks(sut, THREAD_COUNT);
            var taskResults = new List<IUrlCacheInfo>();

            var results = await Task.WhenAll(tasks.ToArray());

            Assert.AreEqual(THREAD_COUNT, results.Length);
            sut.WebServiceMock.Verify(x => x.DoPlatformWebRequest(It.IsAny<Uri>()), Times.Exactly(THREAD_COUNT));
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


        private static Mock<IFetcherWebService> FetcherWebServiceInternetUnavailableMockFactory()
        {
            var web = new Mock<IFetcherWebService>();
            web.Setup(x => x.DoPlatformWebRequest(It.IsAny<Uri>())).Throws(new Exception("mock web exception"));
            return web;
        }
    }
}
