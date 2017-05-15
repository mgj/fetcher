using artm.Fetcher.Core.Services;
using artm.Fetcher.Core.Tests.Services.Mocks;
using artm.Fetcher.Core.Tests.Services.Stubs;
using NUnit.Framework;
using System;

namespace artm.Fetcher.Core.Tests.Services
{
    [TestFixture]
    public class FetcherRepositoryServiceTests
    {
        private readonly Uri URL = new Uri("https://www.google.com");

        [Test]
        public void GetEntryForUrl_NoEntryExists_NullIsReturned()
        {
            var sut = new FetcherRepositoryServiceStub();

            var entry = sut.GetEntryForUrl(URL);

            Assert.IsNull(entry);
        }

        [Test]
        public void GetEntryForUrl_EntryExists_EntryReturned()
        {
            var sut = new FetcherRepositoryServiceStub();

            sut.InsertUrl(URL, "myResponse");
            var entry = sut.GetEntryForUrl(URL);

            Assert.IsNotNull(entry);
        }

        [Test]
        public void UpdateUrl_UrlExists_UrlEntryIsUpdated()
        {
            var response = "myTestResponse";
            var sut = new FetcherRepositoryServiceStub();

            sut.InsertUrl(URL, response);
            var original = sut.GetEntryForUrl(URL);
            var originalResponse = original.Response;
            sut.UpdateUrl(URL, original, "UpdatedTestResponse");
            var second = sut.GetEntryForUrl(URL);

            Assert.IsTrue(!originalResponse.Equals(second.Response));
        }

        [Test]
        public void InsertUrl_ValidInput_UrlIsInserted()
        {
            var response = "myTestResponse";
            var sut = new FetcherRepositoryServiceStub();

            var isNull = sut.GetEntryForUrl(URL);
            sut.InsertUrl(URL, response);
            var notNull = sut.GetEntryForUrl(URL);

            Assert.IsNull(isNull);
            Assert.NotNull(notNull);
        }

        [Test]
        public void PreloadUrl_ValidInput_UrlIsPreloaded()
        {
            var response = "myTestResponse";
            var sut = new FetcherRepositoryServiceStub();

            var isNull = sut.GetEntryForUrl(URL);
            sut.PreloadUrl(URL, response);
            var notNull = sut.GetEntryForUrl(URL);

            Assert.IsNull(isNull);
            Assert.NotNull(notNull);
        }

        [Test]
        public void UpdateUrl_UrlIsUpdated_LastAccessedIsNotUpdated()
        {
            var response = "myTestResponse";
            var sut = new FetcherRepositoryServiceStub();

            sut.PreloadUrl(URL, response);
            var data = sut.GetEntryForUrl(URL);

            var lastAccess1 = data.LastAccessed;
            var lastUpdated1 = data.LastUpdated;

            sut.UpdateUrl(URL, data, "updated-response");
            var lastAccess2 = data.LastAccessed;
            var lastUpdated2 = data.LastUpdated;

            Assert.AreEqual(lastAccess1, lastAccess2);
            Assert.AreNotEqual(lastUpdated1, lastUpdated2);
        }
        
    }
}
