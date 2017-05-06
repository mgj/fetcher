using artm.Fetcher.Core.Services;
using artm.Fetcher.Core.Tests.Services.Mocks;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace artm.Fetcher.Core.Tests.Services
{
    [TestFixture]
    public class FetcherRepositoryServiceTests
    {
        private static FetcherRepositoryService FetcherRepositoryService()
        {
            var mock = new Mock<IFetcherRepositoryStoragePathService>();
            mock.Setup(x => x.GetPath(It.IsAny<string>())).Returns(() => ":memory:");

            return new FetcherRepositoryService(mock.Object);
        }

        [Test]
        public void GetEntryForUrl_NoEntryExists_NullIsReturned()
        {
            var url = new Uri("https://www.google.com");
            var sut = FetcherRepositoryService();

            var entry = sut.GetEntryForUrl(url);

            Assert.IsNull(entry);
        }

        [Test]
        public void GetEntryForUrl_EntryExists_EntryReturned()
        {
            var url = new Uri("https://www.google.com");
            var sut = FetcherRepositoryService();

            sut.InsertUrl(url, "myResponse");
            var entry = sut.GetEntryForUrl(url);

            Assert.IsNotNull(entry);
        }

        [Test]
        public void UpdateUrl_UrlExists_UrlEntryIsUpdated()
        {
            var url = new Uri("https://www.google.com");
            var response = "myTestResponse";
            var sut = FetcherRepositoryService();

            sut.InsertUrl(url, response);
            var original = sut.GetEntryForUrl(url);
            var originalResponse = original.Response;
            sut.UpdateUrl(url, original, "UpdatedTestResponse");
            var second = sut.GetEntryForUrl(url);

            Assert.IsTrue(!originalResponse.Equals(second.Response));
        }

        [Test]
        public void InsertUrl_ValidInput_UrlIsInserted()
        {
            var url = new Uri("https://www.google.com");
            var response = "myTestResponse";
            var sut = FetcherRepositoryService();

            var isNull = sut.GetEntryForUrl(url);
            sut.InsertUrl(url, response);
            var notNull = sut.GetEntryForUrl(url);

            Assert.IsNull(isNull);
            Assert.NotNull(notNull);
        }

        [Test]
        public void PreloadUrl_ValidInput_UrlIsPreloaded()
        {
            var url = new Uri("https://www.google.com");
            var response = "myTestResponse";
            var sut = FetcherRepositoryService();

            var isNull = sut.GetEntryForUrl(url);
            sut.PreloadUrl(url, response);
            var notNull = sut.GetEntryForUrl(url);

            Assert.IsNull(isNull);
            Assert.NotNull(notNull);
        }
    }
}
