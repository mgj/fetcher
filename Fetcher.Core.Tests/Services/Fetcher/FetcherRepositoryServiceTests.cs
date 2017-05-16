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

            var entry = await sut.GetEntryForUrlAsync(URL);

            Assert.IsNull(entry);
        }

        [Test]
        public async Task GetEntryForUrl_EntryExists_EntryReturned()
        {
            var sut = new FetcherRepositoryServiceStub();

            await sut.InsertUrlAsync(URL, FetcherStubFactory.FetcherWebResponseFactory());
            var entry = sut.GetEntryForUrlAsync(URL);

            Assert.IsNotNull(entry);
        }

        [Test]
        public async Task UpdateUrl_UrlExists_UrlEntryIsUpdated()
        {
            var response = FetcherStubFactory.FetcherWebResponseFactory();
            var sut = new FetcherRepositoryServiceStub();

            await sut.InsertUrlAsync(URL, response);
            var original = await sut.GetEntryForUrlAsync(URL);
            var originalResponse = original.FetcherWebResponse;
            await sut.UpdateUrlAsync(URL, original, FetcherStubFactory.FetcherWebResponseFactory("UpdatedTestResponse"));
            var second = await sut.GetEntryForUrlAsync(URL);

            Assert.IsTrue(!originalResponse.Equals(second.FetcherWebResponse));
        }

        [Test]
        public async Task InsertUrl_ValidInput_UrlIsInserted()
        {
            var sut = new FetcherRepositoryServiceStub();

            var isNull = await sut.GetEntryForUrlAsync(URL);
            await sut.InsertUrlAsync(URL, FetcherStubFactory.FetcherWebResponseFactory());
            var notNull = await sut.GetEntryForUrlAsync(URL);

            Assert.IsNull(isNull);
            Assert.NotNull(notNull);
        }

        [Test]
        public async Task PreloadUrl_ValidInput_UrlIsPreloaded()
        {
            var sut = new FetcherRepositoryServiceStub();

            var isNull = await sut.GetEntryForUrlAsync(URL);
            await sut.PreloadUrlAsync(URL, FetcherStubFactory.FetcherWebResponseFactory());
            var notNull = await sut.GetEntryForUrlAsync(URL);

            Assert.IsNull(isNull);
            Assert.NotNull(notNull);
        }

        [Test]
        public async Task UpdateUrl_UrlIsUpdated_LastAccessedIsNotUpdated()
        {
            var sut = new FetcherRepositoryServiceStub();

            await sut.PreloadUrlAsync(URL, FetcherStubFactory.FetcherWebResponseFactory());
            var data = await sut.GetEntryForUrlAsync(URL);

            var lastAccess1 = data.LastAccessed;
            var lastUpdated1 = data.LastUpdated;

            await sut.UpdateUrlAsync(URL, data, FetcherStubFactory.FetcherWebResponseFactory("updated-response"));
            var lastAccess2 = data.LastAccessed;
            var lastUpdated2 = data.LastUpdated;

            Assert.AreEqual(lastAccess1, lastAccess2);
            Assert.AreNotEqual(lastUpdated1, lastUpdated2);
        }
        
    }
}
