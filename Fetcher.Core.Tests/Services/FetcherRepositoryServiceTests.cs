using artm.Fetcher.Core.Tests.Services.Mocks;
using NUnit.Framework;
using System;

namespace artm.Fetcher.Core.Tests.Services
{
    [TestFixture]
    public class FetcherRepositoryServiceTests
    {
        [Test]
        public void GetEntryForUrl_NoEntryExists_NullIsReturned()
        {
            var url = new Uri("https://www.google.com");
            var sut = new FetcherRepositoryServiceMock();

            var entry = sut.GetEntryForUrl(url);

            Assert.IsNull(entry);
        }

        [Test]
        public void GetEntryForUrl_EntryExists_EntryReturned()
        {
            var url = new Uri("https://www.google.com");
            var sut = new FetcherRepositoryServiceMock();

            sut.InsertUrl(url, "myResponse");
            var entry = sut.GetEntryForUrl(url);

            Assert.IsNotNull(entry);
        }

        [Test]
        public void UpdateUrl_UrlExists_UrlEntryIsUpdated()
        {
            var url = new Uri("https://www.google.com");
            var response = "myTestResponse";
            var sut = new FetcherRepositoryServiceMock();

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
            var sut = new FetcherRepositoryServiceMock();

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
            var sut = new FetcherRepositoryServiceMock();

            var isNull = sut.GetEntryForUrl(url);
            sut.PreloadUrl(url, response);
            var notNull = sut.GetEntryForUrl(url);

            Assert.IsNull(isNull);
            Assert.NotNull(notNull);
        }
    }
}
