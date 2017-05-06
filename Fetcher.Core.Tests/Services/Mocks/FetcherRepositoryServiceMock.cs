using artm.Fetcher.Core.Entities;
using artm.Fetcher.Core.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace artm.Fetcher.Core.Tests.Services.Mocks
{
    public class FetcherRepositoryServiceMock : FetcherRepositoryService
    {
        public FetcherRepositoryServiceMock() : base(GetPathServiceMock())
        {
            _db.DeleteAll<UrlCacheInfo>();
        }

        private static IFetcherRepositoryStoragePathService GetPathServiceMock()
        {
            var mock = new Mock<IFetcherRepositoryStoragePathService>();
            mock.Setup(x => x.GetPath(It.IsAny<string>())).Returns(() => ":memory:");
            return mock.Object;
        }
    }
}
