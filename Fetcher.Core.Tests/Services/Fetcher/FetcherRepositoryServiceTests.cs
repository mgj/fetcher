using artm.Fetcher.Core.Entities;
using artm.Fetcher.Core.Models;
using artm.Fetcher.Core.Services;
using artm.Fetcher.Core.Tests.Services.Common;
using artm.Fetcher.Core.Tests.Services.Mocks;
using artm.Fetcher.Core.Tests.Services.Stubs;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace artm.Fetcher.Core.Tests.Services
{
    [TestFixture]
    public class FetcherRepositoryServiceTests
    {
        private readonly Uri URL = new Uri("https://www.google.com");

        [Test]
        public async Task GetUrlCacheInfoForRequest_NoEntryExists_NoEntriesReturned()
        {
            var sut = new FetcherRepositoryServiceStub();
            var request = FetcherStubFactory.FetcherWebRequestGetFactory(URL);

            var entry = await sut.GetUrlCacheInfoForRequest(request);

            Assert.AreEqual(0, entry.Count());
        }

        [Test]
        public async Task GetUrlCacheInfoForRequest_EntryExists_EntryReturned()
        {
            var sut = new FetcherRepositoryServiceStub();
            var request = FetcherStubFactory.FetcherWebRequestGetFactory(URL);

            await sut.InsertUrlAsync(request, FetcherStubFactory.FetcherWebResponseSuccessFactory());
            var entry = await sut.GetUrlCacheInfoForRequest(request);

            Assert.IsNotNull(entry);
            Assert.Greater(entry.Count(), 0);
        }

        [Test]
        public async Task GetUrlCacheInfoForRequest_EntryExistsNewRequest_EntryReturned()
        {
            var sut = new FetcherRepositoryServiceStub();

            await sut.InsertUrlAsync(new FetcherWebRequest()
            {
                Url = URL.OriginalString
            }, FetcherStubFactory.FetcherWebResponseSuccessFactory());
            var entry = await sut.GetUrlCacheInfoForRequest(new FetcherWebRequest()
            {
                Url = URL.OriginalString
            });

            Assert.IsNotNull(entry);
            Assert.Greater(entry.Count(), 0);
        }

        [Test]
        public async Task UpdateUrl_UrlExists_UrlEntryIsUpdated()
        {
            var response = FetcherStubFactory.FetcherWebResponseSuccessFactory();
            var sut = new FetcherRepositoryServiceStub();
            var request = FetcherStubFactory.FetcherWebRequestGetFactory(URL);

            await sut.InsertUrlAsync(request, response);
            var original = (await sut.GetUrlCacheInfoForRequest(request)).FirstOrDefault();
            var originalResponse = original.FetcherWebResponse;

            await sut.UpdateUrlAsync(request, original, FetcherStubFactory.FetcherWebResponseSuccessFactory("UpdatedTestResponse"));
            var second = (await sut.GetUrlCacheInfoForRequest(request)).FirstOrDefault();

            Assert.AreNotEqual(originalResponse.Body, second.FetcherWebResponse.Body);
        }

        [Test]
        public async Task InsertUrl_ValidInput_UrlIsInserted()
        {
            var sut = new FetcherRepositoryServiceStub();
            var request = FetcherStubFactory.FetcherWebRequestGetFactory(URL);

            var isNull = (await sut.GetUrlCacheInfoForRequest(request)).FirstOrDefault();
            await sut.InsertUrlAsync(request, FetcherStubFactory.FetcherWebResponseSuccessFactory());
            var notNull = (await sut.GetUrlCacheInfoForRequest(request)).FirstOrDefault();

            Assert.IsNull(isNull);
            Assert.NotNull(notNull);
        }

        [Test]
        public async Task PreloadUrl_ValidInput_UrlIsPreloaded()
        {
            var sut = new FetcherRepositoryServiceStub();
            var request = FetcherStubFactory.FetcherWebRequestGetFactory(URL);

            var isNull = (await sut.GetUrlCacheInfoForRequest(request)).FirstOrDefault();
            await sut.PreloadUrlAsync(request, FetcherStubFactory.FetcherWebResponseSuccessFactory());
            var notNull = (await sut.GetUrlCacheInfoForRequest(request)).FirstOrDefault();

            Assert.IsNull(isNull);
            Assert.NotNull(notNull);
        }

        [Test]
        public async Task UpdateUrl_UrlIsUpdated_LastAccessedIsNotUpdated()
        {
            var sut = new FetcherRepositoryServiceStub();
            var request = FetcherStubFactory.FetcherWebRequestGetFactory(URL);

            await sut.PreloadUrlAsync(request, FetcherStubFactory.FetcherWebResponseSuccessFactory());
            var data = (await sut.GetUrlCacheInfoForRequest(request)).FirstOrDefault();

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

        [Test]
        public async Task DeleteEntriesOlderThan_DbContainsOldAndNew_OnlyOldsAreRemoved()
        {
            FetcherRepositoryServiceStub sut = new FetcherRepositoryServiceStub(true);
            var request1 = FetcherStubFactory.FetcherWebRequestGetFactory(new Uri("https://www.google.com"));
            var request2 = FetcherStubFactory.FetcherWebRequestGetFactory(new Uri("https://lorempixel.com/200/400/"));

            IUrlCacheInfo hero1 = await sut.PreloadUrlAsync(request1, FetcherStubFactory.FetcherWebResponseSuccessFactory());
            IUrlCacheInfo hero2 = await sut.PreloadUrlAsync(request2, FetcherStubFactory.FetcherWebResponseSuccessFactory());
            hero1.LastUpdated = DateTimeOffset.MinValue;
            await sut.UpdateAsync(hero1);

            await sut.DeleteEntriesOlderThan(400);

            var requests = await sut.AllWebRequests();
            var responses = await sut.AllWebResponse();
            var caches = await sut.AllCacheInfo();

            UrlCacheInfo cacheHero = caches.FirstOrDefault();

            Assert.AreEqual(1, requests.Count);
            Assert.AreEqual(1, responses.Count);
            Assert.AreEqual(1, caches.Count);
            Assert.IsTrue(cacheHero.FetcherWebRequest.Url.Contains("lorempixel.com"));
        }

        [Test]
        public async Task GetEntryForRequestAsync_PostRequestWithBody_MatchingBodyIsReturned()
        {
            var request1 = new FetcherWebRequest
            {
                Url = "https://jsonplaceholder.typicode.com/posts",
                Method = "POST",
                Body = JsonPlaceholderPostFactory()
            };

            var request2 = new FetcherWebRequest
            {
                Url = "https://jsonplaceholder.typicode.com/posts",
                Method = "POST",
                Body = "OtherRequestBody"
            };

            IFetcherService fetcherService = new FetcherServiceStub();
            IFetcherRepositoryService sut = new FetcherRepositoryServiceStub();

            await fetcherService.FetchAsync(request1);
            await fetcherService.FetchAsync(request2);

            IUrlCacheInfo hero = (await sut.GetUrlCacheInfoForRequest(request1)).FirstOrDefault();

            Assert.NotNull(hero);
            Assert.IsTrue(hero.FetcherWebRequest.Body == request1.Body);
        }

        [Test]
        public async Task GetEntryForRequestAsync_TwoRequestsWithSameMethodAndUrl_TwoUrlCacheInfoCreated()
        {
            var request1 = new FetcherWebRequest
            {
                Url = "https://jsonplaceholder.typicode.com/posts",
                Method = "POST",
                Body = JsonPlaceholderPostFactory()
            };

            var request2 = new FetcherWebRequest
            {
                Url = "https://jsonplaceholder.typicode.com/posts",
                Method = "POST",
                Body = "OtherRequestBody"
            };

            FetcherServiceStub fetcherService = new FetcherServiceStub();
            IFetcherRepositoryService sut = fetcherService.RepositoryService;

            await fetcherService.FetchAsync(request1);
            await fetcherService.FetchAsync(request2);

            var allCaches = await sut.GetAllUrlCacheInfo();

            Assert.AreEqual(2, allCaches.Count());
        }

        [Test]
        public async Task GetUrlCacheInfoCompareOnAll_Sunshine_ValidResponse()
        {
            var request1 = new FetcherWebRequest
            {
                Url = "https://jsonplaceholder.typicode.com/posts",
                Method = "POST",
                Body = JsonPlaceholderPostFactory()
            };

            FetcherServiceStub fetcherService = new FetcherServiceStub();
            IFetcherRepositoryService sut = fetcherService.RepositoryService;

            await fetcherService.FetchAsync(request1);
            IEnumerable<IUrlCacheInfo> data = await sut.GetUrlCacheInfoForRequest(request1);

            Assert.NotNull(data);
            Assert.Greater(data.Count(), 0);
        }

        [Test]
        public async Task GetUrlCacheInfoForRequest_RequestContentTypeIsSet_ContentTypeIsReturned()
        {
            var request1 = new FetcherWebRequest
            {
                Url = "https://jsonplaceholder.typicode.com/posts",
                Method = "POST",
                Body = JsonPlaceholderPostFactory(),
                ContentType = "application/json"
            };

            FetcherServiceStub fetcherService = new FetcherServiceStub();
            IFetcherRepositoryService sut = fetcherService.RepositoryService;

            await fetcherService.FetchAsync(request1);
            IEnumerable<IUrlCacheInfo> data = await sut.GetUrlCacheInfoForRequest(request1);
            FetcherWebRequest dbRequest = data.FirstOrDefault().FetcherWebRequest;

            Assert.NotNull(data);
            Assert.NotNull(dbRequest);
            Assert.AreEqual(dbRequest.ContentType, request1.ContentType);
        }

        [Test]
        public async Task GetUrlCacheInfoForRequest_RequestBodyIsSet_BodyIsReturned()
        {
            var request1 = new FetcherWebRequest
            {
                Url = "https://jsonplaceholder.typicode.com/posts",
                Method = "POST",
                Body = JsonPlaceholderPostFactory(),
                ContentType = "application/json"
            };

            FetcherServiceStub fetcherService = new FetcherServiceStub();
            IFetcherRepositoryService sut = fetcherService.RepositoryService;

            await fetcherService.FetchAsync(request1);
            IEnumerable<IUrlCacheInfo> data = await sut.GetUrlCacheInfoForRequest(request1);
            FetcherWebRequest dbRequest = data.FirstOrDefault().FetcherWebRequest;

            Assert.NotNull(data);
            Assert.NotNull(dbRequest);
            Assert.AreEqual(dbRequest.Body, request1.Body);
        }

        [Test]
        public async Task GetUrlCacheInfoForRequest_RequestHeadersIsSet_HeadersIsReturned()
        {
            var request1 = new FetcherWebRequest
            {
                Url = "https://jsonplaceholder.typicode.com/posts",
                Method = "POST",
                Body = JsonPlaceholderPostFactory(),
                Headers = new Dictionary<string, string>
                {
                    { "X-ZUMO-APPLICATION", "hello world" }
                },
                ContentType = "application/json"
            };

            FetcherServiceStub fetcherService = new FetcherServiceStub();
            IFetcherRepositoryService sut = fetcherService.RepositoryService;

            await fetcherService.FetchAsync(request1);
            IEnumerable<IUrlCacheInfo> data = await sut.GetUrlCacheInfoForRequest(request1);
            FetcherWebRequest dbRequest = data.FirstOrDefault().FetcherWebRequest;

            Assert.NotNull(data);
            Assert.NotNull(dbRequest);
            Assert.AreEqual(dbRequest.Headers, request1.Headers);
        }

        [Test]
        public async Task GetUrlCacheInfoForRequest_RequestMethodIsSet_MethodIsReturned()
        {
            var request1 = new FetcherWebRequest
            {
                Url = "https://jsonplaceholder.typicode.com/posts",
                Method = "POST",
                Body = JsonPlaceholderPostFactory(),
                Headers = new Dictionary<string, string>
                {
                    { "X-ZUMO-APPLICATION", "hello world" }
                },
                ContentType = "application/json"
            };

            FetcherServiceStub fetcherService = new FetcherServiceStub();
            IFetcherRepositoryService sut = fetcherService.RepositoryService;

            await fetcherService.FetchAsync(request1);
            IEnumerable<IUrlCacheInfo> data = await sut.GetUrlCacheInfoForRequest(request1);
            FetcherWebRequest dbRequest = data.FirstOrDefault().FetcherWebRequest;

            Assert.NotNull(data);
            Assert.NotNull(dbRequest);
            Assert.AreEqual(dbRequest.Method, request1.Method);
        }

        [Test]
        public async Task GetUrlCacheInfoForRequest_RequestUrlIsSet_UrlIsReturned()
        {
            var request1 = new FetcherWebRequest
            {
                Url = "https://jsonplaceholder.typicode.com/posts",
                Method = "POST",
                Body = JsonPlaceholderPostFactory(),
                Headers = new Dictionary<string, string>
                {
                    { "X-ZUMO-APPLICATION", "hello world" }
                },
                ContentType = "application/json"
            };

            FetcherServiceStub fetcherService = new FetcherServiceStub();
            IFetcherRepositoryService sut = fetcherService.RepositoryService;

            await fetcherService.FetchAsync(request1);
            IEnumerable<IUrlCacheInfo> data = await sut.GetUrlCacheInfoForRequest(request1);
            FetcherWebRequest dbRequest = data.FirstOrDefault().FetcherWebRequest;

            Assert.NotNull(data);
            Assert.NotNull(dbRequest);
            Assert.AreEqual(dbRequest.Url, request1.Url);
        }

        [Test]
        public async Task GetUrlCacheInfoForRequest_RequestMethodNotSet_NoMethodReturned()
        {
            var request1 = new FetcherWebRequest
            {
                Url = "https://jsonplaceholder.typicode.com/posts",
                Body = JsonPlaceholderPostFactory(),
                Headers = new Dictionary<string, string>
                {
                    { "X-ZUMO-APPLICATION", "hello world" }
                },
                ContentType = "application/json"
            };

            FetcherServiceStub fetcherService = new FetcherServiceStub();
            IFetcherRepositoryService sut = fetcherService.RepositoryService;

            await fetcherService.FetchAsync(request1);
            IEnumerable<IUrlCacheInfo> data = await sut.GetUrlCacheInfoForRequest(request1);
            FetcherWebRequest dbRequest = data.FirstOrDefault().FetcherWebRequest;

            Assert.NotNull(data);
            Assert.NotNull(dbRequest);
            Assert.IsTrue(string.IsNullOrEmpty(dbRequest.Method));
            Assert.AreEqual(dbRequest.Method, request1.Method);
        }

        [Test]
        public async Task GetUrlCacheInfoForRequest_RequestContentTypeNotSet_NoContentTypeReturned()
        {
            var request1 = new FetcherWebRequest
            {
                Url = "https://jsonplaceholder.typicode.com/posts",
                Body = JsonPlaceholderPostFactory(),
                Headers = new Dictionary<string, string>
                {
                    { "X-ZUMO-APPLICATION", "hello world" }
                }
            };

            FetcherServiceStub fetcherService = new FetcherServiceStub();
            IFetcherRepositoryService sut = fetcherService.RepositoryService;

            await fetcherService.FetchAsync(request1);
            IEnumerable<IUrlCacheInfo> data = await sut.GetUrlCacheInfoForRequest(request1);
            FetcherWebRequest dbRequest = data.FirstOrDefault().FetcherWebRequest;

            Assert.NotNull(data);
            Assert.NotNull(dbRequest);
            Assert.IsTrue(string.IsNullOrEmpty(dbRequest.ContentType));
            Assert.AreEqual(dbRequest.ContentType, request1.ContentType);
        }

        [Test]
        public async Task GetUrlCacheInfoForRequest_RequestBodyNotSet_NoBodyReturned()
        {
            var request1 = new FetcherWebRequest
            {
                Url = "https://jsonplaceholder.typicode.com/posts",
                Headers = new Dictionary<string, string>
                {
                    { "X-ZUMO-APPLICATION", "hello world" }
                }
            };

            FetcherServiceStub fetcherService = new FetcherServiceStub();
            IFetcherRepositoryService sut = fetcherService.RepositoryService;

            await fetcherService.FetchAsync(request1);
            IEnumerable<IUrlCacheInfo> data = await sut.GetUrlCacheInfoForRequest(request1);
            FetcherWebRequest dbRequest = data.FirstOrDefault().FetcherWebRequest;

            Assert.NotNull(data);
            Assert.NotNull(dbRequest);
            Assert.IsTrue(string.IsNullOrEmpty(dbRequest.Body));
            Assert.AreEqual(dbRequest.Body, request1.Body);
        }

        [Test]
        public async Task GetUrlCacheInfoForRequest_Sunshine_ReturnedRequestContainsUrlCacheInfoId()
        {
            var request1 = new FetcherWebRequest
            {
                Url = "https://jsonplaceholder.typicode.com/posts",
                Method = "GET"
            };

            FetcherServiceStub fetcherService = new FetcherServiceStub();
            IFetcherRepositoryService sut = fetcherService.RepositoryService;

            await fetcherService.FetchAsync(request1);
            IEnumerable<IUrlCacheInfo> data = await sut.GetUrlCacheInfoForRequest(request1);
            FetcherWebRequest dbRequest = data.FirstOrDefault().FetcherWebRequest;

            Assert.NotNull(data);
            Assert.NotNull(dbRequest);
            Assert.AreEqual(1, dbRequest.UrlCacheInfoId);
        }

        [Test]
        public async Task GetUrlCacheInfoForRequest_Sunshine_ReturnedResponseContainsUrlCacheInfoId()
        {
            var request1 = new FetcherWebRequest
            {
                Url = "https://jsonplaceholder.typicode.com/posts",
                Method = "GET"
            };

            FetcherServiceStub fetcherService = new FetcherServiceStub();
            IFetcherRepositoryService sut = fetcherService.RepositoryService;

            await fetcherService.FetchAsync(request1);
            IEnumerable<IUrlCacheInfo> data = await sut.GetUrlCacheInfoForRequest(request1);
            FetcherWebResponse dbResponse = data.FirstOrDefault().FetcherWebResponse;

            Assert.NotNull(data);
            Assert.NotNull(dbResponse);
            Assert.AreEqual(1, dbResponse.UrlCacheInfoId);
        }

        private static string JsonPlaceholderPostFactory()
        {
            var hero = new JsonPlaceholderPostDto
            {
                title = "MyTitle",
                body = "MyBody",
                userId = "1"
            };

            return JsonConvert.SerializeObject(hero);
        }

        private class JsonPlaceholderPostDto
        {
            public string title { get; set; }
            public string body { get; set; }
            public string userId { get; set; }
        }
    }
}
