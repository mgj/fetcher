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
            var result = new FetcherRepositoryService();
            result.Initialize(FetcherRepositoryStoragePathService());
            return result;
        }

        private static IFetcherRepositoryStoragePathService FetcherRepositoryStoragePathService()
        {
            var result = new Mock<IFetcherRepositoryStoragePathService>();
            result.Setup(x => x.GetPath(It.IsAny<string>())).Returns(() => ":memory:");
            return result.Object;
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
    }
}
