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
        private readonly Uri URL = new Uri("https://jsonplaceholder.typicode.com/users");

        private static IFetcherWebRequest IFetcherWebRequestFactory(Uri url)
        {
            return new FetcherWebRequest()
            {
                Url = url.OriginalString,
                Method = "GET"
            };
        }

        [Test]
        public void Fetch_NullUrl_Throws()
        {
            var sut = new FetcherServiceStub();
            Uri url = null;
            Assert.ThrowsAsync<NullReferenceException>(async () => await sut.FetchAsync(url));
        }

        [Test]
        public void Fetch_NullRequest_Throws()
        {
            var sut = new FetcherServiceStub();
            IFetcherWebRequest request = null;
            Assert.ThrowsAsync<NullReferenceException>(async () => await sut.FetchAsync(request));
        }

        [Test]
        public async Task Fetch_Sunshine_TriesToFetchFromWeb()
        {
            var sut = new FetcherServiceStub();
            var response = await sut.FetchAsync(URL);

            sut.WebServiceMock.Verify(x => x.DoPlatformRequest(It.IsAny<FetcherWebRequest>()), Times.Once);
        }

        [Test]
        public async Task Fetch_NoEntries_RepositoryInsertUrlIsCalled()
        {
            var repository = new Mock<IFetcherRepositoryService>();
            var sut = new FetcherServiceStub(repository.Object);

            var response = await sut.FetchAsync(URL);

            repository.Verify(x => x.InsertUrlAsync(It.IsAny<IFetcherWebRequest>(), It.IsAny<IFetcherWebResponse>()), Times.Once);
        }

        [Test]
        public async Task Fetch_MultipleCallsToSameUrl_InsertUrlCalledNeverCalledWhenEntryExists()
        {
            var repository = FetcherMockFactory.IFetcherRepositoryServiceWitUpToDateEntries();
            var sut = new FetcherServiceStub(repository);

            var response = await sut.FetchAsync(URL);
            await sut.FetchAsync(URL);
            await sut.FetchAsync(URL);

            repository.Verify(x => x.InsertUrlAsync(It.IsAny<IFetcherWebRequest>(), It.IsAny<IFetcherWebResponse>()), Times.Never);
        }

        [Test]
        public async Task Fetch_MultipleCallsToSameUrl_LastAccessedIsUpdated()
        {
            var sut = new FetcherServiceStub();

            var response1 = await sut.FetchAsync(URL);
            var access1 = response1.LastAccessed.ToString();
            await Task.Delay(10);
            var response2 = await sut.FetchAsync(URL);
            var access2 = response2.LastAccessed.ToString();

            var delta = response2.LastAccessed - response1.LastAccessed;
            Assert.IsTrue( delta.TotalMilliseconds > 0);
        }

        [Test]
        public async Task Fetch_WithOutdatedEntries_RepositoryUpdateUrlIsCalled()
        {
            var repository = FetcherMockFactory.IFetcherRepositoryServiceWithOutdatedEntries();

            var sut = new FetcherServiceStub(repository.Object);

            var response = await sut.FetchAsync(URL);

            repository.Verify(x => x.UpdateUrlAsync(It.IsAny<IFetcherWebRequest>(), It.IsAny<IUrlCacheInfo>(), It.IsAny<IFetcherWebResponse>()), Times.Once);
        }

        [Test]
        public async Task Fetch_WithUpToDateEntries_RepositoryUpdateUrlIsNotCalled()
        {
            var repository = FetcherMockFactory.IFetcherRepositoryServiceWitUpToDateEntries();
            var created = DateTimeOffset.UtcNow - TimeSpan.FromMilliseconds(1);
            
            var sut = new FetcherServiceStub(repository.Object);
            var response = await sut.FetchAsync(URL);

            repository.Verify(x => x.UpdateUrlAsync(It.IsAny<IFetcherWebRequest>(), It.IsAny<IUrlCacheInfo>(), It.IsAny<IFetcherWebResponse>()), Times.Never);
        }

        [Test]
        public void Fetch_NoInternet_DoesNotThrow()
        {
            var sut = new FetcherServiceStub(FetcherMockFactory.IFetcherWebServiceInternetOff());

            Assert.DoesNotThrowAsync(async () => await sut.FetchAsync(new Uri("http://test")));
        }

        [Test]
        public async Task Preload_InternetUnavailableAndEmptyDatabase_PreloadedDataIsReturned()
        {
            var sut = new FetcherServiceStub(FetcherMockFactory.IFetcherWebServiceInternetOff());
            const string response = "myPreloadResponse";

            await sut.PreloadAsync(FetcherStubFactory.FetcherWebRequestGetFactory(URL), FetcherStubFactory.FetcherWebResponseSuccessFactory(response));
            var hero = await sut.FetchAsync(URL);

            Assert.IsNotNull(hero);
            Assert.IsTrue(hero.FetcherWebResponse.Body.Equals(response));
        }

        [Test]
        public async Task Preload_InternetUnavailable_PreloadedDataIsConsideredInvalidated()
        {
            var sut = new FetcherServiceStub(FetcherMockFactory.IFetcherWebServiceInternetOff());
            const string response = "myPreloadResponse";
            var asStub = sut.RepositoryService as FetcherRepositoryServiceStub;

            await sut.PreloadAsync(FetcherStubFactory.FetcherWebRequestGetFactory(URL), FetcherStubFactory.FetcherWebResponseSuccessFactory(response));

            var caches = await asStub.AllCacheInfo();
            var responses = await asStub.AllWebResponse();

            var hero = await sut.FetchAsync(URL);

            caches = await asStub.AllCacheInfo();
            responses = await asStub.AllWebResponse();

            var isInvalid = FetcherService.ShouldInvalidate(hero, TimeSpan.FromDays(1));

            Assert.IsTrue(isInvalid);
        }

        [Test]
        public async Task Fetch_EmptyDatabaseNoPreloadInternetUnavailable_NullIsReturned()
        {
            var sut = new FetcherServiceStub(FetcherMockFactory.IFetcherWebServiceInternetOff());

            var hero = await sut.FetchAsync(URL);

            Assert.IsNull(hero);
        }
        
        [Test]
        public async Task Fetch_PreloadedDataInternetUnavailable_FetchedFromIsPreloaded()
        {
            var sut = new FetcherServiceStub(FetcherMockFactory.IFetcherWebServiceInternetOff());
            const string RESPONSE_STRING = "Fetch_PreloadedDataInternetUnavailable_FetchedFromIsPreloaded";
            
            await sut.PreloadAsync(FetcherStubFactory.FetcherWebRequestGetFactory(URL), FetcherStubFactory.FetcherWebResponseSuccessFactory(RESPONSE_STRING));
            var hero = await sut.FetchAsync(URL);

            Assert.NotNull(hero);
            Assert.AreEqual(RESPONSE_STRING, hero.FetcherWebResponse.Body);
            Assert.AreEqual(hero.FetchedFrom, CacheSourceType.Preload);
        }

        [Test]
        public async Task Fetch_PreloadedDataWithInternet_FetchedFromWeb()
        {
            var sut = new FetcherServiceStub();

            const string RESPONSE_STRING = "Fetch_PreloadedDataWithInternet_FetchedFromWeb";

            await sut.PreloadAsync(FetcherStubFactory.FetcherWebRequestGetFactory(URL), FetcherStubFactory.FetcherWebResponseSuccessFactory(RESPONSE_STRING));
            var hero = await sut.FetchAsync(URL);

            Assert.NotNull(hero);
            Assert.AreNotEqual(RESPONSE_STRING, hero.FetcherWebResponse.Body);
            Assert.AreEqual(hero.FetchedFrom, CacheSourceType.Web);
        }

        [Test]
        public async Task Fetch_RecentlyUpdated_FetchedFromLocalCache()
        {
            var sut = new FetcherServiceStub();

            const string RESPONSE_STRING = "Fetch_RecentlyUpdated_FetchedFromLocalCache";
            
            var hero = await sut.FetchAsync(URL);
            hero = await sut.FetchAsync(URL);
            
            Assert.NotNull(hero);
            Assert.AreNotEqual(RESPONSE_STRING, hero.FetcherWebResponse.Body);
            Assert.AreEqual(hero.FetchedFrom, CacheSourceType.Local);
        }

        [Test]
        public async Task Fetch_RecentlyUpdatedManyTimes_FetchedFromLocalCache()
        {
            var sut = new FetcherServiceStub();

            const string RESPONSE_STRING = "Fetch_RecentlyUpdated_FetchedFromLocalCache";

            var hero = await sut.FetchAsync(URL);
            hero = await sut.FetchAsync(URL);
            hero = await sut.FetchAsync(URL);
            hero = await sut.FetchAsync(URL);
            hero = await sut.FetchAsync(URL);
            hero = await sut.FetchAsync(URL);
            hero = await sut.FetchAsync(URL);

            Assert.NotNull(hero);
            Assert.AreNotEqual(RESPONSE_STRING, hero.FetcherWebResponse);
            Assert.AreEqual(hero.FetchedFrom, CacheSourceType.Local);
        }

        [Test]
        public async Task Fetch_MultipleCallsToSameUrl_OnlyOneDatabaseEntryIsCreated()
        {
            var sut = new FetcherServiceStub();
            var asStub = sut.RepositoryService as FetcherRepositoryServiceStub;

            var hero = await sut.FetchAsync(URL);
            hero = await sut.FetchAsync(URL);
            hero = await sut.FetchAsync(URL);
            hero = await sut.FetchAsync(URL);
            hero = await sut.FetchAsync(URL);
            hero = await sut.FetchAsync(URL);
            hero = await sut.FetchAsync(URL);

            var caches = await asStub.AllCacheInfo();
            var responses = await asStub.AllWebResponse();

            Assert.AreEqual(1, caches.Count);
            Assert.AreEqual(1, responses.Count);
        }

        [Test]
        public async Task Fetch_MultithreadedToSameUrl_OnlyOneDatabaseEntryIsCreated()
        {
            const int THREAD_COUNT = 10;

            var sut = new FetcherServiceStub();
            var tasks = GenerateFetchTasks(sut, THREAD_COUNT, new Uri("https://www.google.com"));

            var results = await Task.WhenAll(tasks.ToArray());

            var asStub = sut.RepositoryService as FetcherRepositoryServiceStub;
            var caches = await asStub.AllCacheInfo();
            var responses = await asStub.AllWebResponse();

            Assert.AreEqual(1, caches.Count);
            Assert.AreEqual(1, responses.Count);
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
            var data = await ((FetcherRepositoryServiceStub)sut.RepositoryService).AllCacheInfo();

            Assert.AreEqual(1, data.Count());
        }

        [Test]
        public async Task Fetch_MultithreadedToDifferentUrls_EntriesCreatedInDatabase()
        {
            const int THREAD_COUNT = 10;

            var sut = new FetcherServiceStub();
            var tasks = GenerateFetchTasks(sut, THREAD_COUNT);
            var taskResults = new List<IUrlCacheInfo>();

            var results = await Task.WhenAll(tasks.ToArray());
            var data = await ((FetcherRepositoryServiceStub)sut.RepositoryService).AllCacheInfo();

            Assert.AreEqual(THREAD_COUNT, data.Count());
        }

        [Test]
        public async Task Fetch_MultipleCalls_OnlyOneEntryInDatabase()
        {
            var sut = new FetcherServiceStub();

            var hero = await sut.FetchAsync(URL);
            hero = await sut.FetchAsync(URL);
            hero = await sut.FetchAsync(URL);
            hero = await sut.FetchAsync(URL);
            hero = await sut.FetchAsync(URL);

            FetcherRepositoryServiceStub repository = (FetcherRepositoryServiceStub)sut.RepositoryService;
            var caches = await repository.AllCacheInfo();
            var responses = await repository.AllWebResponse();
            var requests = await repository.AllWebRequests();

            Assert.AreEqual(1, caches.Count);
            Assert.AreEqual(1, responses.Count);
            Assert.AreEqual(1, requests.Count);
        }

        [Test]
        public async Task Fetch_MultipleGetRequests_OnlyOneEntryInDatabase()
        {
            var sut = new FetcherServiceStub();
            var request = new FetcherWebRequest()
            {
                Url = URL.OriginalString,
                Method = "GET"
            };
            var hero = await sut.FetchAsync(request);
            hero = await sut.FetchAsync(request);
            hero = await sut.FetchAsync(request);
            hero = await sut.FetchAsync(request);
            hero = await sut.FetchAsync(request);

            FetcherRepositoryServiceStub repository = (FetcherRepositoryServiceStub)sut.RepositoryService;
            var caches = await repository.AllCacheInfo();
            var responses = await repository.AllWebResponse();
            var requests = await repository.AllWebRequests();

            Assert.AreEqual(1, caches.Count);
            Assert.AreEqual(1, responses.Count);
            Assert.AreEqual(1, requests.Count);
        }

        [Test]
        public async Task Fetch_MultiplePostRequests_OnlyOneEntryInDatabase()
        {
            var sut = new FetcherServiceStub();
            var request = new FetcherWebRequest()
            {
                Url = URL.OriginalString,
                Method = "POST"
            };
            var hero = await sut.FetchAsync(request);
            hero = await sut.FetchAsync(request);
            hero = await sut.FetchAsync(request);
            hero = await sut.FetchAsync(request);
            hero = await sut.FetchAsync(request);

            FetcherRepositoryServiceStub repository = (FetcherRepositoryServiceStub)sut.RepositoryService;
            var caches = await repository.AllCacheInfo();
            var responses = await repository.AllWebResponse();
            var requests = await repository.AllWebRequests();

            Assert.AreEqual(1, caches.Count);
            Assert.AreEqual(1, responses.Count);
            Assert.AreEqual(1, requests.Count);
        }

        [Test]
        public async Task Fetch_MultipleDifferentMethods_CorrectAmountOfEntriesInDatabase()
        {
            var sut = new FetcherServiceStub();

            await sut.FetchAsync(new FetcherWebRequest()
            {
                Url = URL.OriginalString,
                Method = "POST",
                Body = "My test request body"
            });
            await sut.FetchAsync(new FetcherWebRequest()
            {
                Url = URL.OriginalString,
                Method = "GET"
            });
            await sut.FetchAsync(new FetcherWebRequest()
            {
                Url = URL.OriginalString,
                Method = "DELETE",
                Body = "My test request body"
            });

            FetcherRepositoryServiceStub repository = (FetcherRepositoryServiceStub)sut.RepositoryService;
            var caches = await repository.AllCacheInfo();
            var responses = await repository.AllWebResponse();
            var requests = await repository.AllWebRequests();

            Assert.AreEqual(3, caches.Count);
            Assert.AreEqual(3, responses.Count);
            Assert.AreEqual(3, requests.Count);
        }

        [Test]
        public async Task Fetch_MultipleDifferentMethodsMultipleCalls_CorrectAmountOfEntriesInDatabase()
        {
            var sut = new FetcherServiceStub();

            var postRequest = new FetcherWebRequest()
            {
                Url = URL.OriginalString,
                Method = "POST",
                Body = "My test request body"
            };
            var getRequest = new FetcherWebRequest()
            {
                Url = URL.OriginalString,
                Method = "GET"
            };
            var deleteRequest = new FetcherWebRequest()
            {
                Url = URL.OriginalString,
                Method = "DELETE",
                Body = "My test request body"
            };

            await sut.FetchAsync(postRequest);
            await sut.FetchAsync(postRequest);
            await sut.FetchAsync(postRequest);
            await sut.FetchAsync(postRequest);

            await sut.FetchAsync(getRequest);
            await sut.FetchAsync(getRequest);
            await sut.FetchAsync(getRequest);
            await sut.FetchAsync(getRequest);

            await sut.FetchAsync(deleteRequest);
            await sut.FetchAsync(deleteRequest);
            await sut.FetchAsync(deleteRequest);
            await sut.FetchAsync(deleteRequest);

            FetcherRepositoryServiceStub repository = (FetcherRepositoryServiceStub)sut.RepositoryService;
            var caches = await repository.AllCacheInfo();
            var responses = await repository.AllWebResponse();
            var requests = await repository.AllWebRequests();

            Assert.AreEqual(3, caches.Count);
            Assert.AreEqual(3, responses.Count);
            Assert.AreEqual(3, requests.Count);
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
            result.Add(fetcher.FetchAsync(url));
        }
    }
}
