using artm.Fetcher.Core.Entities;
using artm.Fetcher.Core.Models;
using artm.Fetcher.Core.Services;
using artm.Fetcher.Core.Tests.Services.Mocks;
using Moq;
using NUnit.Framework;
using System;
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
            var web = new Mock<IFetcherWebService>();
            web.Setup(x => x.DoPlatformWebRequest(It.IsAny<Uri>())).Throws(new Exception("mock web exception"));
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
            var web = new Mock<IFetcherWebService>();
            web.Setup(x => x.DoPlatformWebRequest(It.IsAny<Uri>())).Throws(new Exception("mock web exception"));
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
            var web = new Mock<IFetcherWebService>();
            web.Setup(x => x.DoPlatformWebRequest(It.IsAny<Uri>())).Throws(new Exception("mock web exception"));
            var sut = new FetcherServiceMock(web);

            var hero = await sut.Fetch(new Uri(URL));

            Assert.IsNull(hero);
        }
        
    }
}
